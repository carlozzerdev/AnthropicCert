using AnthropicCert.Application;
using AnthropicCert.Infrastructure;

namespace AnthropicCert.Desktop;
internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var root = AppContext.BaseDirectory;
            var catalog = ExamCatalog.Load(Path.Combine(root, "Content"));
            // Missing banks disable new sessions, but historical snapshots remain reviewable.
            var bank = catalog.FirstOrDefault(e => e.Bank != null)?.Bank ?? new Domain.QuestionBank { Certification = catalog[0].Entry.Certification };
            if (args.Length >= 2 && args[0] == "--ui-test") return UiVerification.Run(bank, args[1], catalog);
            if (args.Contains("--smoke-test"))
            {
                bank.Validate();
                var data = Path.Combine(Path.GetTempPath(), "AnthropicCert-Smoke-" + Guid.NewGuid());
                using var repo = new FileAttemptRepository(data);
                var service = new ExamService(bank, repo, new SystemClock(), new Random(42));
                var a = service.Start(Domain.ExamMode.Practice, 2);
                service.SaveResponse(a, a.Questions[0], a.Questions[0].Correct, true, false);
                service.Complete(a);
                foreach (var entry in catalog.Where(e => e.Bank != null && e.Bank != bank))
                {
                    service.SelectBank(entry.Bank!);
                    var other = service.Start(Domain.ExamMode.Practice, 2);
                    service.SaveResponse(other, other.Questions[0], other.Questions[0].Correct, true, false);
                    service.Complete(other);
                    if (other.Result?.Correct != 1) return 2;
                }
                using var form = new MainForm(service, catalog);
                form.Show(); form.Refresh(); System.Windows.Forms.Application.DoEvents();
                if (a.Result?.Correct != 1 || repo.Load().Count != catalog.Count(e => e.Bank != null)) return 2;
                if (args.Length >= 2)
                    File.WriteAllText(args[1], System.Text.Json.JsonSerializer.Serialize(new
                    {
                        runtimeDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
                        formsAssembly = typeof(Form).Assembly.Location,
                        baseDirectory = AppContext.BaseDirectory,
                        questions = bank.Questions.Count, exams = catalog.Select(e => new { e.Entry.Certification, questions = e.Bank?.Questions.Count, e.Error })
                    }));
                form.Close(); return 0;
            }
            using var storage = new FileAttemptRepository(Path.Combine(root, "Data"));
            var exams = new ExamService(bank, storage, new SystemClock(), new Random());
            // Validate data at startup before allowing a new session.
            foreach (var a in exams.History.Where(a => a.State == Domain.AttemptState.InProgress)) exams.Resume(a.Id);
            System.Windows.Forms.Application.Run(new MainForm(exams, catalog)); return 0;
        }
        catch (Exception ex)
        {
            if (args.Contains("--smoke-test")) { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "smoke-error.txt"), ex.ToString()); return 1; }
            MessageBox.Show(ex.Message + "\n\nExtrae la aplicación en una carpeta donde puedas escribir. Conserva Data para recuperar tu progreso.",
                "No se pudo iniciar AnthropicCert", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1;
        }
    }
}
