using AnthropicCert.Domain;
using System.Text.Json;

namespace AnthropicCert.Application;

public interface IAttemptRepository
{
    List<Attempt> Load();
    void Save(Attempt attempt);
}
public interface IClock { DateTimeOffset UtcNow { get; } }
public sealed class SystemClock : IClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }
public sealed class ExamService(QuestionBank bank, IAttemptRepository repository, IClock clock, Random random)
{
    public QuestionBank Bank { get; private set; } = bank;
    public IReadOnlyList<Attempt> History => repository.Load();
    public IReadOnlyList<Attempt> SelectedHistory => History.Where(a => a.Certification == Bank.Certification).ToList();
    public void SelectBank(QuestionBank value) { value.Validate(); Bank = value; }
    public Attempt Start(ExamMode mode, int count, string? domain = null, IReadOnlySet<string>? onlyIds = null)
    {
        if (History.Any(a => a.State == AttemptState.InProgress))
            throw new InvalidOperationException("Reanuda o abandona el intento pendiente antes de crear otro.");
        if (mode == ExamMode.Mock) { count = Bank.MockQuestionCount; domain = null; onlyIds = null; }
        var selected = ExamRules.Select(Bank, count, domain, mode == ExamMode.Mock, random, onlyIds);
        // Snapshots are independent from the live bank, including their study aids and references.
        var snapshot = JsonSerializer.Deserialize<List<Question>>(JsonSerializer.Serialize(selected))!;
        foreach (var q in snapshot) q.Options = q.Options.OrderBy(_ => random.Next()).ToList();
        var attempt = new Attempt
        {
            Mode = mode, Certification = Bank.Certification, BankId = Bank.Id, BankVersion = Bank.Version,
            Profile = Bank.ProfileLabel, ProfileId = Bank.ProfileId, ProfileVersion = Bank.ProfileVersion,
            BlueprintVersion = Bank.BlueprintVersion, Domains = Bank.Domains.ToList(), Started = clock.UtcNow, LastSeen = clock.UtcNow,
            Deadline = mode == ExamMode.Mock ? clock.UtcNow.AddMinutes(Bank.DurationMinutes) : null, Questions = snapshot,
            Responses = snapshot.ToDictionary(q => q.Id, _ => new Response())
        };
        repository.Save(attempt); return attempt;
    }
    public Attempt Resume(Guid id)
    {
        var a = History.Single(a => a.Id == id);
        if (a.State == AttemptState.InProgress) Tick(a);
        return a;
    }
    public bool Tick(Attempt a)
    {
        if (a.State != AttemptState.InProgress) return false;
        var now = clock.UtcNow;
        if (now < a.LastSeen.AddMinutes(-1)) throw new InvalidOperationException("El reloj ha retrocedido. Corrige la hora de Windows para continuar.");
        a.LastSeen = now < a.LastSeen ? a.LastSeen : now;
        if (a.Deadline is { } deadline && a.LastSeen >= deadline) { Complete(a); return true; }
        return false;
    }
    public void SaveResponse(Attempt a, Question q, IEnumerable<string> ids, bool confirm, bool flagged)
    {
        EnsureActive(a); if (Tick(a)) return;
        var r = a.Answer(q); var selected = ids.Distinct().ToList();
        if (a.Mode == ExamMode.Practice && r.Confirmed) throw new InvalidOperationException("Respuesta ya confirmada.");
        if (selected.Any(id => !q.Options.Any(o => o.Id == id)) || (q.Correct.Count == 1 && selected.Count > 1))
            throw new InvalidOperationException("Selección no válida.");
        if (confirm && selected.Count == 0) throw new InvalidOperationException("Selecciona una respuesta antes de confirmar.");
        var prior = new Response { Selected = r.Selected.ToList(), Flagged = r.Flagged, Confirmed = r.Confirmed };
        r.Selected = selected; r.Flagged = flagged; r.Confirmed = a.Mode == ExamMode.Practice && confirm;
        try { repository.Save(a); }
        catch { a.Responses[q.Id] = prior; throw; }
    }
    public void SavePosition(Attempt a, int position)
    {
        if (position < 0 || position >= a.Questions.Count) throw new ArgumentOutOfRangeException(nameof(position));
        var prior = a.Position; a.Position = position;
        if (a.State == AttemptState.InProgress)
        {
            try { Tick(a); repository.Save(a); }
            catch { a.Position = prior; throw; }
        }
    }
    public void Complete(Attempt a)
    {
        if (a.State != AttemptState.InProgress) return;
        var prior = JsonSerializer.Deserialize<Attempt>(JsonSerializer.Serialize(a))!;
        var now = clock.UtcNow > a.LastSeen ? clock.UtcNow : a.LastSeen;
        a.Finished = a.Deadline is { } d && d < now ? d : now;
        a.LastSeen = a.Finished.Value;
        if (a.Mode == ExamMode.Practice)
            foreach (var r in a.Responses.Values.Where(r => !r.Confirmed)) r.Selected.Clear();
        a.State = AttemptState.Completed; a.Result = ExamRules.Score(a);
        try { repository.Save(a); }
        catch
        {
            a.State = prior.State; a.Finished = prior.Finished; a.LastSeen = prior.LastSeen;
            a.Result = prior.Result; a.Responses = prior.Responses; throw;
        }
    }
    public void Abandon(Attempt a)
    {
        EnsureActive(a); if (Tick(a)) return;
        a.State = AttemptState.Abandoned; a.Finished = clock.UtcNow;
        try { repository.Save(a); }
        catch { a.State = AttemptState.InProgress; a.Finished = null; throw; }
    }
    public HashSet<string> Errors() => SelectedHistory.Where(a => a.State == AttemptState.Completed)
        .SelectMany(a => a.Questions.Where(q => !ExamRules.IsCorrect(q, a.Answer(q))))
        .Select(q => q.Id).ToHashSet();
    static void EnsureActive(Attempt a)
    {
        if (a.State != AttemptState.InProgress) throw new InvalidOperationException("El intento ya no está activo.");
    }
}
