using AnthropicCert.Application;
using AnthropicCert.Domain;
using AnthropicCert.Infrastructure;
using Xunit;

namespace AnthropicCert.Tests;
public sealed class Clock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 10, 21, 10, 0, 0, TimeSpan.Zero);
}
public sealed class ExamTests : IDisposable
{
    readonly string path = Path.Combine(Path.GetTempPath(), "AnthropicCert-tests-" + Guid.NewGuid());
    readonly QuestionBank bank = JsonFormat.LoadBank(Path.Combine(AppContext.BaseDirectory, "bank.json"));
    public void Dispose() { if (Directory.Exists(path)) Directory.Delete(path, true); }
    [Fact] public void AllContentValidAndWeightedMockCovered()
    {
        bank.Validate(); Assert.True(bank.Questions.Count >= 63);
        var qs = ExamRules.Select(bank, 63, null, true, new Random(1));
        Assert.Equal(63, qs.Select(q => q.Id).Distinct().Count());
        foreach (var (id, count) in ExamRules.Quotas(63, bank.Domains)) Assert.Equal(count, qs.Count(q => q.DomainId == id));
        Assert.Contains(bank.Questions, q => q.Correct.Count > 1);
    }
    [Fact] public void InvalidBanksAreRejected()
    {
        bank.Questions.Add(bank.Questions[0]); Assert.Throws<InvalidDataException>(bank.Validate);
        bank.Questions.RemoveAt(bank.Questions.Count - 1); bank.Questions[0].Correct = ["ZZ"];
        Assert.Throws<InvalidDataException>(bank.Validate);
    }
    [Fact] public void NonHttpReferencesAreRejected()
    {
        bank.Questions[0].References = [new("bad", "file:///C:/test", "")];
        Assert.Throws<InvalidDataException>(bank.Validate);
    }
    [Theory]
    [InlineData("A,C", true)] [InlineData("C,A", true)] [InlineData("A", false)]
    [InlineData("A,B,C", false)] [InlineData("", false)]
    public void MultiResponseIsExactSet(string value, bool expected)
    {
        var q = new Question { Correct = ["A", "C"] };
        Assert.Equal(expected, ExamRules.IsCorrect(q, new Response { Selected = value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() }));
    }
    [Fact] public void SampleScoreCountsBlanksSeparately()
    {
        var a = new Attempt { Mode = ExamMode.Mock, Started = DateTimeOffset.UtcNow };
        a.Finished = a.Started.AddMinutes(30);
        for (var i = 0; i < 20; i++)
        {
            var q = new Question { Id = i.ToString(), Correct = ["A"] }; a.Questions.Add(q);
            a.Responses[q.Id] = new Response { Selected = i < 14 ? ["A"] : i < 18 ? ["B"] : [] };
        }
        var s = ExamRules.Score(a);
        Assert.Equal((14, 4, 2), (s.Correct, s.Wrong, s.Blank)); Assert.Equal(70, s.Percentage);
        Assert.Equal(77.777777, s.Precision!.Value, 5); Assert.Equal(90, s.AverageSeconds);
    }
    [Fact] public void ConfirmedPracticeIsLockedAndDraftsScoreBlank()
    {
        using var repo = new FileAttemptRepository(path); var clock = new Clock();
        var service = new ExamService(bank, repo, clock, new Random(2)); var a = service.Start(ExamMode.Practice, 2);
        var q = a.Questions[0]; service.SaveResponse(a, q, q.Correct, true, false);
        Assert.Throws<InvalidOperationException>(() => service.SaveResponse(a, q, [], false, false));
        service.SaveResponse(a, a.Questions[1], a.Questions[1].Correct, false, false);
        service.Complete(a); Assert.Equal(1, a.Result!.Correct); Assert.Equal(1, a.Result.Blank);
        Assert.Empty(a.Answer(a.Questions[1]).Selected); Assert.Single(repo.Load());
    }
    [Fact] public void DeadlineFinalizationIsIdempotentAfterRestart()
    {
        var clock = new Clock(); Guid id;
        using (var repo = new FileAttemptRepository(path))
        {
            var service = new ExamService(bank, repo, clock, new Random(3)); var a = service.Start(ExamMode.Mock, 63);
            id = a.Id; service.SaveResponse(a, a.Questions[0], a.Questions[0].Correct, false, true);
        }
        clock.UtcNow = clock.UtcNow.AddDays(3);
        using var repo2 = new FileAttemptRepository(path); var service2 = new ExamService(bank, repo2, clock, new Random());
        var resumed = service2.Resume(id); service2.Complete(resumed);
        Assert.Equal(AttemptState.Completed, resumed.State); Assert.Equal(7200, resumed.Result!.Seconds);
        Assert.Equal(1, resumed.Result.Correct); Assert.Single(service2.History);
        Assert.True(resumed.Answer(resumed.Questions[0]).Flagged);
    }
    [Fact] public void SnapshotAndOrderSurviveBankUpdate()
    {
        using var repo = new FileAttemptRepository(path); var service = new ExamService(bank, repo, new Clock(), new Random());
        var a = service.Start(ExamMode.Practice, 2); var text = a.Questions[0].Text;
        var order = a.Questions[0].Options.Select(o => o.Id).ToArray();
        bank.Questions.Single(q => q.Id == a.Questions[0].Id).Text = "changed";
        var loaded = service.Resume(a.Id); Assert.Equal(text, loaded.Questions[0].Text);
        Assert.Equal(order, loaded.Questions[0].Options.Select(o => o.Id));
    }
    [Fact] public void SecondInstanceAndIncompatibleSchemaFailSafely()
    {
        using (var repo = new FileAttemptRepository(path)) Assert.Throws<IOException>(() => new FileAttemptRepository(path));
        File.WriteAllText(Path.Combine(path, "schema-version.txt"), "99");
        Assert.Throws<InvalidDataException>(() => new FileAttemptRepository(path));
        Assert.Equal("99", File.ReadAllText(Path.Combine(path, "schema-version.txt")));
    }
    [Fact] public void AtomicSaveCreatesBackupAndCorruptionIsNotSilentlyReset()
    {
        using var repo = new FileAttemptRepository(path); var service = new ExamService(bank, repo, new Clock(), new Random());
        var a = service.Start(ExamMode.Practice, 2); service.SavePosition(a, 1);
        Assert.Single(Directory.GetFiles(path, "*.bak", SearchOption.AllDirectories));
        File.WriteAllText(Directory.GetFiles(path, "*.attempt.json", SearchOption.AllDirectories).Single(), "broken");
        Assert.Throws<InvalidDataException>(() => repo.Load()); Assert.Single(Directory.GetFiles(path, "*.bak", SearchOption.AllDirectories));
    }
    [Fact] public void WeightedStatisticsAreNotMeanOfPercentages()
    {
        Attempt Finished(int n, int correct) => new() { State = AttemptState.Completed, Mode = ExamMode.Mock,
            BankVersion = bank.Version, Finished = DateTimeOffset.UtcNow, Result = new(n, correct, n-correct, 0, 100) };
        var a = Finished(2, 1); var b = Finished(10, 9);
        var ignored = Finished(10, 10); ignored.State = AttemptState.Abandoned;
        var filter = new StatisticsFilter(ExamMode.Mock, null, null, bank.Version, null);
        var s = Statistics.Calculate([a, b, ignored], bank, filter);
        Assert.Equal(83.333333, s.Global!.Value, 5); Assert.Equal(70, s.Mean); Assert.Equal(90, s.Best);
        Assert.Equal(2, s.Attempts.Count);
        var empty = Statistics.Calculate([a, b], bank, filter with { Mode = ExamMode.Practice });
        Assert.Null(empty.Global); Assert.Null(empty.Best); Assert.All(empty.Domains, d => Assert.Equal(0, d.Score.Total));
    }
    [Fact] public void CoverageCountsRepeatedQuestionsOnceAndFiltersDates()
    {
        using var repo = new FileAttemptRepository(path); var clock = new Clock(); var service = new ExamService(bank, repo, clock, new Random(5));
        var a = service.Start(ExamMode.Practice, 1); service.Complete(a);
        var s = Statistics.Calculate([a, a], bank, new(ExamMode.Practice, null, null, bank.Version, null));
        Assert.Equal(1, s.Unique); Assert.Equal(2, s.Total);
        var empty = Statistics.Calculate([a], bank, new(null, clock.UtcNow.AddDays(1), null, null, null));
        Assert.Empty(empty.Attempts);
    }
    [Fact] public void InsufficientDomainAndQuotaTiesAreExplicit()
    {
        var quota = ExamRules.Quotas(1, [new("B", "B", 50), new("A", "A", 50)]);
        Assert.Equal(1, quota["A"]); Assert.Equal(0, quota["B"]);
        Assert.Throws<InvalidOperationException>(() => ExamRules.Select(bank, 1000, "D7", false, new Random()));
    }
    [Fact] public void ClockRollbackDoesNotExtendExam()
    {
        using var repo = new FileAttemptRepository(path); var clock = new Clock(); var service = new ExamService(bank, repo, clock, new Random());
        var a = service.Start(ExamMode.Mock, 63); clock.UtcNow = clock.UtcNow.AddMinutes(-5);
        Assert.Throws<InvalidOperationException>(() => service.Tick(a)); Assert.Equal(AttemptState.InProgress, a.State);
    }
    sealed class FailingRepo : IAttemptRepository
    {
        public bool Fail { get; set; }
        public List<Attempt> Items { get; } = [];
        public List<Attempt> Load() => Items;
        public void Save(Attempt a)
        {
            if (Fail) throw new IOException("simulated full disk");
            if (!Items.Contains(a)) Items.Add(a);
        }
    }
    [Fact] public void FailedSaveDoesNotConfirmOrFinalizeInMemory()
    {
        var repo = new FailingRepo(); var service = new ExamService(bank, repo, new Clock(), new Random());
        var a = service.Start(ExamMode.Practice, 1); repo.Fail = true;
        Assert.Throws<IOException>(() => service.SaveResponse(a, a.Questions[0], a.Questions[0].Correct, true, false));
        Assert.False(a.Answer(a.Questions[0]).Confirmed); Assert.Empty(a.Answer(a.Questions[0]).Selected);
        Assert.Throws<IOException>(() => service.Complete(a)); Assert.Equal(AttemptState.InProgress, a.State); Assert.Null(a.Result);
    }
}
