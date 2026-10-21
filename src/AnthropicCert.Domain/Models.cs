namespace AnthropicCert.Domain;

public sealed record Option(string Id, string Text, string Explanation);
public sealed record Reference(string Title, string Url, string Section);
public sealed record StudyHelp(string Id, string Title, string DomainId, string Text, List<Reference> References);
public sealed record DomainInfo(string Id, string Title, int Weight);
public sealed class Question
{
    public string Id { get; set; } = "";
    public int Revision { get; set; } = 1;
    public string DomainId { get; set; } = "";
    public string Text { get; set; } = "";
    public List<Option> Options { get; set; } = [];
    public List<string> Correct { get; set; } = [];
    public string Explanation { get; set; } = "";
    public StudyHelp Help { get; set; } = new("", "", "", "", []);
    public List<Reference> References { get; set; } = [];
    public string Origin { get; set; } = "";
    public string Review { get; set; } = "";
    public string Language { get; set; } = "en";
    public string Family { get; set; } = "";
}
public sealed class QuestionBank
{
    public int SchemaVersion { get; set; } = 1;
    public string Id { get; set; } = "ccarp-local";
    public string Version { get; set; } = "1.0.0";
    public string Certification { get; set; } = "CCAR-P";
    public string ProfileStatus { get; set; } = "Provisional: guía oficial pendiente de acceso directo";
    public List<DomainInfo> Domains { get; set; } = [];
    public List<Question> Questions { get; set; } = [];
    public void Validate()
    {
        if (SchemaVersion != 1 || Certification != "CCAR-P" || string.IsNullOrWhiteSpace(Version))
            throw new InvalidDataException("Versión de banco o certificación no compatible.");
        if (Domains.Count == 0 || Domains.Select(d => d.Id).Distinct().Count() != Domains.Count ||
            Domains.Any(d => d.Weight <= 0) || Domains.Sum(d => d.Weight) != 100)
            throw new InvalidDataException("Los dominios deben ser únicos y sus pesos sumar 100.");
        if (Questions.Count == 0 || Questions.Select(q => q.Id).Distinct().Count() != Questions.Count)
            throw new InvalidDataException("Banco vacío o IDs de pregunta duplicados.");
        foreach (var q in Questions)
        {
            if (string.IsNullOrWhiteSpace(q.Id) || string.IsNullOrWhiteSpace(q.Text) || q.Revision < 1 ||
                !Domains.Any(d => d.Id == q.DomainId) || q.Options.Count < 2 ||
                q.Options.Select(o => o.Id).Distinct().Count() != q.Options.Count ||
                q.Options.Any(o => string.IsNullOrWhiteSpace(o.Text) || string.IsNullOrWhiteSpace(o.Explanation)) ||
                q.Correct.Count == 0 || q.Correct.Distinct().Count() != q.Correct.Count ||
                q.Correct.Any(c => !q.Options.Any(o => o.Id == c)) ||
                string.IsNullOrWhiteSpace(q.Explanation) || string.IsNullOrWhiteSpace(q.Help.Text) ||
                string.IsNullOrWhiteSpace(q.Origin) || string.IsNullOrWhiteSpace(q.Review) || q.References.Count == 0)
                throw new InvalidDataException($"Pregunta inválida: {q.Id}.");
            foreach (var r in q.References.Concat(q.Help.References))
                if (!Uri.TryCreate(r.Url, UriKind.Absolute, out var u) || u.Scheme is not ("http" or "https"))
                    throw new InvalidDataException($"URL no permitida en {q.Id}.");
        }
    }
}
public enum ExamMode { Practice, Mock }
public enum AttemptState { InProgress, Completed, Abandoned }
public sealed class Response
{
    public List<string> Selected { get; set; } = [];
    public bool Confirmed { get; set; }
    public bool Flagged { get; set; }
}
public sealed class Attempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ExamMode Mode { get; set; }
    public AttemptState State { get; set; }
    public string BankVersion { get; set; } = "";
    public string Profile { get; set; } = "CCAR-P · provisional v1";
    public string ScoringVersion { get; set; } = "exact-1";
    public DateTimeOffset Started { get; set; }
    public DateTimeOffset? Deadline { get; set; }
    public DateTimeOffset LastSeen { get; set; }
    public DateTimeOffset? Finished { get; set; }
    public List<Question> Questions { get; set; } = [];
    public Dictionary<string, Response> Responses { get; set; } = [];
    public int Position { get; set; }
    public Score? Result { get; set; }
    public Response Answer(Question q) => Responses[q.Id];
}
public sealed record Score(int Total, int Correct, int Wrong, int Blank, double Seconds)
{
    public double Percentage => Total == 0 ? 0 : 100d * Correct / Total;
    public double? Precision => Correct + Wrong == 0 ? null : 100d * Correct / (Correct + Wrong);
    public double AverageSeconds => Total == 0 ? 0 : Seconds / Total;
}
