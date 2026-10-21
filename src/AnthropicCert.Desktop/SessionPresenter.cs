using AnthropicCert.Application;
using AnthropicCert.Domain;

namespace AnthropicCert.Desktop;
internal sealed class SessionPresenter(ExamService service, Attempt attempt)
{
    public Attempt Attempt { get; } = attempt;
    public Question Current => Attempt.Questions[Attempt.Position];
    public Response Response => Attempt.Answer(Current);
    public bool Revealed => Attempt.State == AttemptState.Completed ||
        (Attempt.Mode == ExamMode.Practice && Response.Confirmed);
    public bool Editable => Attempt.State == AttemptState.InProgress && !Revealed;
    public string DomainTitle => service.Bank.Domains.Single(d => d.Id == Current.DomainId).Title;
    public void Select(IEnumerable<string> ids, bool confirm, bool flagged) => service.SaveResponse(Attempt, Current, ids, confirm, flagged);
    public void Move(int position) => service.SavePosition(Attempt, position);
    public void Complete() => service.Complete(Attempt);
    public bool Tick() => service.Tick(Attempt);
    public string Status()
    {
        var answered = Attempt.Responses.Values.Count(r => r.Selected.Count > 0 &&
            (Attempt.Mode == ExamMode.Mock || r.Confirmed));
        var time = Attempt.Deadline is { } d && Attempt.State == AttemptState.InProgress
            ? $" · Restante {Ui.Duration((d - DateTimeOffset.UtcNow).TotalSeconds)}" : "";
        return $"{Ui.Mode(Attempt.Mode)} · {Ui.State(Attempt.State)} · {Attempt.Position + 1}/{Attempt.Questions.Count} · Contestadas {answered} · Pendientes {Attempt.Questions.Count - answered} · Marcadas {Attempt.Responses.Values.Count(r => r.Flagged)}{time}";
    }
    public string Explanation()
    {
        if (!Revealed) throw new InvalidOperationException("La solución no está disponible durante este intento.");
        var lines = Current.Options.OrderBy(o => o.Id).Select(o => $"{o.Id} · {(Current.Correct.Contains(o.Id) ? "CORRECTA" : "INCORRECTA")}\n{o.Text}\n{o.Explanation}");
        return $"Tu respuesta: {(Response.Selected.Count == 0 ? "Sin responder" : string.Join(", ", Response.Selected))}\n" +
            $"Solución: {string.Join(", ", Current.Correct)}\n\n{Current.Explanation}\n\n" + string.Join("\n\n", lines) +
            $"\n\nProcedencia: {Current.Origin}\nRevisión: {Current.Review}";
    }
}
