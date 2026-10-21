namespace AnthropicCert.Domain;

public static class ExamRules
{
    public static bool IsCorrect(Question q, Response response) => response.Selected.ToHashSet().SetEquals(q.Correct);
    public static Score Score(Attempt attempt, string? domain = null)
    {
        var items = attempt.Questions.Where(q => domain == null || q.DomainId == domain).ToList();
        var correct = 0; var wrong = 0; var blank = 0;
        foreach (var q in items)
        {
            var r = attempt.Answer(q);
            if (r.Selected.Count == 0 || (attempt.Mode == ExamMode.Practice && !r.Confirmed)) blank++;
            else if (IsCorrect(q, r)) correct++;
            else wrong++;
        }
        var end = attempt.Finished ?? attempt.LastSeen;
        var seconds = Math.Max(0, (end - attempt.Started).TotalSeconds);
        return new(items.Count, correct, wrong, blank, seconds);
    }

    public static Dictionary<string, int> Quotas(int count, IReadOnlyList<DomainInfo> domains)
    {
        if (count <= 0 || domains.Sum(d => d.Weight) != 100) throw new ArgumentException("Perfil no válido.");
        var quotas = domains.ToDictionary(d => d.Id, d => count * d.Weight / 100);
        foreach (var d in domains.OrderByDescending(d => count * d.Weight % 100).ThenBy(d => d.Id, StringComparer.Ordinal)
                     .Take(count - quotas.Values.Sum())) quotas[d.Id]++;
        return quotas;
    }

    public static List<Question> Select(QuestionBank bank, int count, string? domain, bool weighted, Random random,
        IReadOnlySet<string>? onlyIds = null)
    {
        var pool = bank.Questions.Where(q => (domain == null || q.DomainId == domain) &&
            (onlyIds == null || onlyIds.Contains(q.Id))).OrderBy(_ => random.Next()).ToList();
        if (count <= 0 || pool.Count < count) throw new InvalidOperationException($"Hay {pool.Count} preguntas disponibles para esta selección; reduce la cantidad.");
        if (!weighted) return pool.Take(count).ToList();
        var selected = new List<Question>();
        foreach (var (key, needed) in Quotas(count, bank.Domains))
        {
            var subset = pool.Where(q => q.DomainId == key).Take(needed).ToList();
            if (subset.Count != needed)
                throw new InvalidOperationException($"El dominio {key} necesita {needed} preguntas y solo dispone de {subset.Count}. Usa una práctica reducida.");
            selected.AddRange(subset);
        }
        return selected.OrderBy(_ => random.Next()).ToList();
    }
}
