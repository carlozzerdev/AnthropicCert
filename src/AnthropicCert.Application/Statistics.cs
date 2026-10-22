using AnthropicCert.Domain;

namespace AnthropicCert.Application;
public sealed record StatisticsFilter(ExamMode? Mode, DateTimeOffset? From, DateTimeOffset? To,
    string? BankVersion, string? Profile);
public sealed record DomainResult(string Id, Score Score);
public sealed record StatisticsResult(List<Attempt> Attempts, int Total, int Correct, int Wrong, int Blank,
    double? Global, double? Mean, double? Best, double? Last, double Seconds,
    List<DomainResult> Domains, int Unique, int Eligible);
public static class Statistics
{
    public static StatisticsResult Calculate(IEnumerable<Attempt> history, QuestionBank bank, StatisticsFilter filter)
    {
        var attempts = history.Where(a => a.Certification == bank.Certification && a.State == AttemptState.Completed && a.Result != null &&
            a.Result.Total > 0 && a.Result.Correct + a.Result.Wrong + a.Result.Blank == a.Result.Total &&
            (filter.Mode == null || a.Mode == filter.Mode) &&
            (filter.From == null || a.Finished >= filter.From) && (filter.To == null || a.Finished < filter.To) &&
            (filter.BankVersion == null || a.BankVersion == filter.BankVersion) &&
            (filter.Profile == null || a.Profile == filter.Profile))
            .OrderBy(a => a.Finished).ToList();
        var total = attempts.Sum(a => a.Result!.Total); var correct = attempts.Sum(a => a.Result!.Correct);
        var domains = bank.Domains.Select(d =>
        {
            var scores = attempts.Select(a => ExamRules.Score(a, d.Id)).ToList();
            return new DomainResult(d.Id, new(scores.Sum(s => s.Total), scores.Sum(s => s.Correct),
                scores.Sum(s => s.Wrong), scores.Sum(s => s.Blank), 0));
        }).ToList();
        var eligibleIds = bank.Questions.Select(q => q.Id).ToHashSet();
        var unique = attempts.Where(a => a.BankVersion == bank.Version).SelectMany(a => a.Questions)
            .Select(q => q.Id).Where(eligibleIds.Contains).Distinct().Count();
        return new(attempts, total, correct, attempts.Sum(a => a.Result!.Wrong), attempts.Sum(a => a.Result!.Blank),
            total == 0 ? null : 100d * correct / total,
            attempts.Count == 0 ? null : attempts.Average(a => a.Result!.Percentage),
            attempts.Count == 0 ? null : attempts.Max(a => a.Result!.Percentage), attempts.LastOrDefault()?.Result?.Percentage,
            attempts.Sum(a => a.Result!.Seconds), domains, unique, bank.Questions.Count);
    }
}
