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
            var bank = JsonFormat.LoadBank(Path.Combine(root, "Content", "bank.json"));
            if (args.Length >= 2 && args[0] == "--ui-test") return UiVerification.Run(bank, args[1]);
            if (args.Contains("--smoke-test"))
            {
                var data = Path.Combine(Path.GetTempPath(), "AnthropicCert-Smoke-" + Guid.NewGuid());
                using var repo = new FileAttemptRepository(data);
                var service = new ExamService(bank, repo, new SystemClock(), new Random(42));
                var a = service.Start(Domain.ExamMode.Practice, 2);
                service.SaveResponse(a, a.Questions[0], a.Questions[0].Correct, true, false);
                service.Complete(a);
                using var form = new MainForm(service);
                form.Show(); form.Refresh(); System.Windows.Forms.Application.DoEvents();
                if (a.Result?.Correct != 1 || repo.Load().Count != 1) return 2;
                if (args.Length >= 2)
                    File.WriteAllText(args[1], System.Text.Json.JsonSerializer.Serialize(new
                    {
                        runtimeDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
                        formsAssembly = typeof(Form).Assembly.Location,
                        baseDirectory = AppContext.BaseDirectory,
                        questions = bank.Questions.Count
                    }));
                form.Close(); return 0;
            }
            using var storage = new FileAttemptRepository(Path.Combine(root, "Data"));
            var exams = new ExamService(bank, storage, new SystemClock(), new Random());
            // Validate data at startup before allowing a new session.
            foreach (var a in exams.History.Where(a => a.State == Domain.AttemptState.InProgress)) exams.Resume(a.Id);
            System.Windows.Forms.Application.Run(new MainForm(exams)); return 0;
        }
        catch (Exception ex)
        {
            if (args.Contains("--smoke-test")) { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "smoke-error.txt"), ex.ToString()); return 1; }
            MessageBox.Show(ex.Message + "\n\nExtrae la aplicación en una carpeta donde puedas escribir. Conserva Data para recuperar tu progreso.",
                "No se pudo iniciar AnthropicCert", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1;
        }
    }
}
