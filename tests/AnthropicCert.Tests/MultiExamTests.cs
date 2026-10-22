using System.Text.Json;
using System.Text.Json.Nodes;
using AnthropicCert.Application;
using AnthropicCert.Domain;
using AnthropicCert.Infrastructure;
using Xunit;

namespace AnthropicCert.Tests;

public sealed class MultiExamTests : IDisposable
{
    readonly string path = Path.Combine(Path.GetTempPath(), "AnthropicCert-multi-" + Guid.NewGuid());
    readonly QuestionBank professional = JsonFormat.LoadBank(Path.Combine(AppContext.BaseDirectory, "bank.json"));
    readonly QuestionBank foundations = JsonFormat.LoadBank(Path.Combine(AppContext.BaseDirectory, "foundations.json"));
    public void Dispose() { if (Directory.Exists(path)) Directory.Delete(path, true); }
    string CatalogDirectory()
    {
        var content = Path.Combine(path, "Content"); Directory.CreateDirectory(content);
        foreach (var name in new[] { "exams.json", "bank.json", "foundations.json" })
            File.Copy(Path.Combine(AppContext.BaseDirectory, name), Path.Combine(content, name));
        return content;
    }
    [Fact] public void CatalogAndBothWeightedMocksAreUsableInEnglish()
    {
        var entries = ExamCatalog.Load(CatalogDirectory()); Assert.Equal(2, entries.Count);
        foreach (var entry in entries)
        {
            Assert.Null(entry.Error); var bank = entry.Bank!; bank.Validate();
            Assert.All(bank.Questions, q => Assert.Equal("en", q.Language));
            var selected = ExamRules.Select(bank, bank.MockQuestionCount, null, true, new Random(12));
            Assert.Equal(bank.MockQuestionCount, selected.Select(q => q.Id).Distinct().Count());
            foreach (var (id, count) in ExamRules.Quotas(bank.MockQuestionCount, bank.Domains))
                Assert.Equal(count, selected.Count(q => q.DomainId == id));
        }
        Assert.Contains(entries, e => e.Entry.Aliases.Contains("CCA-F") && e.Entry.Certification == "CCAR-F");
        Assert.All(professional.Questions.Where(q => q.Id.StartsWith("original-")), q => Assert.Equal(2, q.Revision));
    }
    [Fact] public void BrokenBankIsIsolatedAndUnknownCatalogIdentitiesAreRejected()
    {
        var content = CatalogDirectory(); File.WriteAllText(Path.Combine(content, "foundations.json"), "{}");
        var entries = ExamCatalog.Load(content);
        Assert.NotNull(entries.Single(e => e.Entry.Certification == "CCAR-P").Bank);
        Assert.Null(entries.Single(e => e.Entry.Certification == "CCAR-F").Bank);
        var catalog = Path.Combine(content, "exams.json");
        File.WriteAllText(catalog, File.ReadAllText(catalog).Replace("CCAR-F", "CCAR-P"));
        Assert.Throws<InvalidDataException>(() => ExamCatalog.Load(content));
    }
    [Theory] [InlineData("../outside.json")] [InlineData("C:/outside.json")]
    public void CatalogCannotEscapeContent(string badPath)
    {
        var content = CatalogDirectory(); var file = Path.Combine(content, "exams.json");
        var doc = JsonNode.Parse(File.ReadAllText(file))!;
        doc["entries"]![0]!["bankPath"] = badPath; File.WriteAllText(file, doc.ToJsonString());
        Assert.Throws<InvalidDataException>(() => ExamCatalog.Load(content));
    }
    [Theory] [InlineData("language")] [InlineData("objective")] [InlineData("help")] [InlineData("license")]
    public void InvalidQuestionMetadataIsRejected(string error)
    {
        var q = foundations.Questions[0];
        if (error == "language") q.Language = "es";
        if (error == "objective") q.ObjectiveId = "D1.unknown";
        if (error == "help") q.Help = q.Help with { DomainId = "another" };
        if (error == "license") q.License = "";
        Assert.Throws<InvalidDataException>(foundations.Validate);
    }
    [Fact] public void ActiveAttemptIsGlobalButSwitchingAndResumeDoNotChangeItsProfile()
    {
        using var repo = new FileAttemptRepository(path); var clock = new Clock();
        var service = new ExamService(professional, repo, clock, new Random(1));
        var a = service.Start(ExamMode.Mock, 63); var deadline = a.Deadline;
        service.SelectBank(foundations); Assert.Empty(service.SelectedHistory);
        Assert.Throws<InvalidOperationException>(() => service.Start(ExamMode.Practice, 1));
        var resumed = service.Resume(a.Id); Assert.Equal("CCAR-P", resumed.Certification);
        Assert.Equal(deadline, resumed.Deadline); Assert.Equal(63, resumed.Questions.Count);
        service.Complete(resumed);
        foundations.DurationMinutes = 17;
        var f = service.Start(ExamMode.Mock, 999);
        Assert.Equal(60, f.Questions.Count); Assert.Equal(clock.UtcNow.AddMinutes(17), f.Deadline);
        Assert.Equal("CCAR-F", f.Certification); Assert.Equal(foundations.ProfileLabel, f.Profile);
        Assert.All(f.Questions, q => Assert.StartsWith("F", q.DomainId));
    }
    [Fact] public void SameQuestionIdAndBankVersionNeverCrossCertificationStatisticsOrErrors()
    {
        professional.Questions[0].Id = foundations.Questions[0].Id;
        using var repo = new FileAttemptRepository(path);
        var service = new ExamService(professional, repo, new Clock(), new Random(2));
        var a = service.Start(ExamMode.Practice, 1, null, new HashSet<string> { professional.Questions[0].Id });
        service.Complete(a); Assert.Single(service.Errors()); service.SelectBank(foundations);
        Assert.Empty(service.Errors()); Assert.Empty(service.SelectedHistory);
        var stats = Statistics.Calculate(service.History, foundations, new(null, null, null, null, null));
        Assert.Equal(0, stats.Total); Assert.Equal(0, stats.Unique); Assert.Null(stats.Global);
        var b = service.Start(ExamMode.Practice, 1, null, new HashSet<string> { foundations.Questions[0].Id });
        service.SaveResponse(b, b.Questions[0], b.Questions[0].Correct, true, false); service.Complete(b);
        stats = Statistics.Calculate(service.History, foundations, new(null, null, null, null, null));
        Assert.Equal(1, stats.Total); Assert.Equal(100, stats.Global); Assert.Equal(1, stats.Unique);
    }
    [Fact] public void QuotaShortageDoesNotCreateAnAttemptOrBorrowAnotherDomain()
    {
        foundations.Questions.RemoveAll(q => q.DomainId == "F2");
        using var repo = new FileAttemptRepository(path);
        var service = new ExamService(foundations, repo, new Clock(), new Random(1));
        Assert.Throws<InvalidOperationException>(() => service.Start(ExamMode.Mock, 60)); Assert.Empty(repo.Load());
        Assert.Single(service.Start(ExamMode.Practice, 1).Questions);
    }
    Attempt WriteLegacy(string profile = "CCAR-P · provisional v1", AttemptState state = AttemptState.InProgress)
    {
        Directory.CreateDirectory(path); File.WriteAllText(Path.Combine(path, "schema-version.txt"), "1");
        var q = professional.Questions[0]; q.Text = "Texto histórico en español"; q.Language = "es";
        var clock = new Clock(); var a = new Attempt { Profile = profile, BankVersion = "1.0.0", State = state,
            Mode = ExamMode.Mock, Started = clock.UtcNow, LastSeen = clock.UtcNow, Deadline = clock.UtcNow.AddHours(2),
            Questions = [q], Responses = new() { [q.Id] = new() { Selected = q.Correct, Flagged = true } } };
        if (state == AttemptState.Completed) { a.Finished = clock.UtcNow; a.Result = ExamRules.Score(a); }
        var node = JsonSerializer.SerializeToNode(a, JsonFormat.Options)!;
        foreach (var key in new[] { "schemaVersion", "certification", "bankId", "profileId", "profileVersion", "blueprintVersion", "domains" }) node.AsObject().Remove(key);
        File.WriteAllText(Path.Combine(path, $"{a.Id}.attempt.json"), node.ToJsonString());
        return a;
    }
    [Theory] [InlineData(AttemptState.InProgress)] [InlineData(AttemptState.Completed)] [InlineData(AttemptState.Abandoned)]
    public void MigrationPreservesOriginalsSnapshotsAndResultsAndIsIdempotent(AttemptState state)
    {
        var a = WriteLegacy(state: state); var file = Path.Combine(path, $"{a.Id}.attempt.json"); var original = File.ReadAllBytes(file);
        using (var repo = new FileAttemptRepository(path))
        {
            var migrated = Assert.Single(repo.Load()); Assert.Equal(a.Id, migrated.Id);
            Assert.Equal("CCAR-P", migrated.Certification); Assert.Equal(a.Questions[0].Text, migrated.Questions[0].Text);
            Assert.Equal("es", migrated.Questions[0].Language); Assert.Equal(a.Deadline, migrated.Deadline);
            Assert.Equal(a.State, migrated.State); Assert.Equal(a.Result, migrated.Result);
            Assert.Equal(a.Responses[a.Questions[0].Id].Selected, migrated.Responses[a.Questions[0].Id].Selected);
            Assert.True(migrated.Responses[a.Questions[0].Id].Flagged);
        }
        Assert.Equal(original, File.ReadAllBytes(file));
        Assert.Equal(original, File.ReadAllBytes(Path.Combine(path, "migration-v1-backup", Path.GetFileName(file))));
        Assert.Equal("2", File.ReadAllText(Path.Combine(path, "schema-version.txt")));
        using var again = new FileAttemptRepository(path); Assert.Single(again.Load());
    }
    [Fact] public void InterruptedMigrationCanRestageBeforeCommit()
    {
        var a = WriteLegacy(); Directory.CreateDirectory(Path.Combine(path, "attempts-v2"));
        File.WriteAllText(Path.Combine(path, "attempts-v2", $"{a.Id}.attempt.json"), "incomplete-stage");
        using var repo = new FileAttemptRepository(path); Assert.Equal(a.Id, Assert.Single(repo.Load()).Id);
        Assert.Equal("2", File.ReadAllText(Path.Combine(path, "schema-version.txt")));
    }
    [Fact] public void UnknownLegacyProvenanceDoesNotModifyOriginalsOrVersion()
    {
        var a = WriteLegacy("CCAR-F unknown"); var file = Path.Combine(path, $"{a.Id}.attempt.json"); var raw = File.ReadAllText(file);
        Assert.Throws<InvalidDataException>(() => new FileAttemptRepository(path));
        Assert.Equal(raw, File.ReadAllText(file)); Assert.Equal("1", File.ReadAllText(Path.Combine(path, "schema-version.txt")));
        Assert.False(Directory.Exists(Path.Combine(path, "attempts-v2")));
    }
    [Fact] public void FailedStagingLeavesV1UsableAndCanBeRetried()
    {
        WriteLegacy(); File.WriteAllText(Path.Combine(path, "attempts-v2"), "blocking file simulates unavailable stage");
        Assert.Throws<IOException>(() => new FileAttemptRepository(path));
        Assert.Equal("1", File.ReadAllText(Path.Combine(path, "schema-version.txt")));
        Assert.Single(Directory.GetFiles(path, "*.attempt.json"));
        File.Delete(Path.Combine(path, "attempts-v2"));
        using var repo = new FileAttemptRepository(path); Assert.Single(repo.Load());
    }
    [Fact] public void MissingCurrentBankDoesNotPreventReadingOrCompletingHistoricalSnapshot()
    {
        var legacy = WriteLegacy();
        using var repo = new FileAttemptRepository(path); var clock = new Clock();
        var service = new ExamService(foundations, repo, clock, new Random());
        var old = service.Resume(legacy.Id); service.Complete(old);
        Assert.Equal("CCAR-P", old.Certification); Assert.Equal(1, old.Result!.Correct);
        Assert.Equal("Texto histórico en español", old.Questions[0].Text);
    }
}
