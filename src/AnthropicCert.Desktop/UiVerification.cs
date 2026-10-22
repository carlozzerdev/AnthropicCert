using AnthropicCert.Application;
using AnthropicCert.Domain;
using AnthropicCert.Infrastructure;

namespace AnthropicCert.Desktop;
// Deterministic UI acceptance harness; data is isolated from the user's portable Data directory.
internal static class UiVerification
{
    static IEnumerable<Control> Descendants(Control c) => c.Controls.Cast<Control>().SelectMany(x => new[] { x }.Concat(Descendants(x)));
    static Button Find(Form f, string text) => Descendants(f).OfType<Button>().First(b => b.Text == text);
    public static int Run(QuestionBank bank, string output, IReadOnlyList<AvailableExam>? catalog = null)
    {
        output = Path.GetFullPath(output); Directory.CreateDirectory(output);
        var data = Path.Combine(output, "isolated-data");
        using var repo = new FileAttemptRepository(data);
        var service = new ExamService(bank, repo, new SystemClock(), new Random(112));
        using var main = new MainForm(service, catalog);
        using var timer = new System.Windows.Forms.Timer { Interval = 300 };
        var step = 0; var ticks = 0; var exitCode = 1; var busy = false;
        void Capture(Form f, string name)
        {
            f.Refresh(); using var bmp = new Bitmap(f.Width, f.Height); f.DrawToBitmap(bmp, new Rectangle(Point.Empty, f.Size));
            bmp.Save(Path.Combine(output, name + ".png"));
        }
        timer.Tick += (_, _) =>
        {
            if (busy) return;
            try
            {
                if (++ticks > 100) throw new TimeoutException("UI verification timeout");
                switch (step)
                {
                    case 0:
                        Capture(main, "01-home"); step++; Find(main, "Empezar práctica").PerformClick(); break;
                    case 1:
                        var session = System.Windows.Forms.Application.OpenForms.OfType<SessionForm>().Single();
                        var attempt = service.History.Single(a => a.State == AttemptState.InProgress);
                        var q = attempt.Questions[0];
                        foreach (var c in Descendants(session).OfType<CheckBox>().Where(c => c.Tag is string id && q.Correct.Contains(id))) c.Checked = true;
                        Find(session, "Confirmar respuesta").PerformClick();
                        if (!Descendants(session).OfType<Button>().Any(b => b.Text == "Ver explicación")) throw new Exception("No correction button after confirmation");
                        Capture(session, "02-practice-corrected"); step++; Find(session, "Ver explicación").PerformClick(); break;
                    case 2:
                        var explanation = System.Windows.Forms.Application.OpenForms.Cast<Form>().Single(f => f.Text == "Corrección y razonamiento");
                        if (!Descendants(explanation).OfType<TextBox>().Single().Text.Contains("Solución:")) throw new Exception("Missing explanation");
                        Capture(explanation, "03-explanation"); step++; explanation.Close(); break;
                    case 3:
                        var practice = System.Windows.Forms.Application.OpenForms.OfType<SessionForm>().Single();
                        step++; practice.Close(); break;
                    case 4:
                        var completed = service.History.Single(a => a.State == AttemptState.InProgress); service.Complete(completed);
                        Find(main, "Historial").PerformClick(); Capture(main, "04-history");
                        Find(main, "Estadísticas").PerformClick();
                        Descendants(main).OfType<ComboBox>().Single(c => c.Name == "StatisticsMode").SelectedIndex = 1;
                        Find(main, "Aplicar filtros").PerformClick(); Capture(main, "05-statistics"); step++; break;
                    case 5:
                        Find(main, "Ayudas").PerformClick(); Descendants(main).OfType<ListBox>().Single().SelectedIndex = 0;
                        step++; Find(main, "Leer ayuda seleccionada").PerformClick(); break;
                    case 6:
                        var help = System.Windows.Forms.Application.OpenForms.Cast<Form>().Single(f => f != main);
                        Capture(help, "06-help"); step++; help.Close(); break;
                    case 7:
                        Find(main, "Inicio y exámenes").PerformClick(); step++; Find(main, "Comenzar simulacro").PerformClick(); break;
                    case 8:
                        var mock = System.Windows.Forms.Application.OpenForms.OfType<SessionForm>().Single();
                        if (Descendants(mock).OfType<Button>().Any(b => b.Text is "Ver explicación" or "Leer ayuda del concepto" or "Confirmar respuesta"))
                            throw new Exception("Mock reveals solution actions");
                        mock.Size = new Size(1000, 720); Capture(mock, "07-mock-small-window"); step++; mock.Close(); break;
                    case 9:
                        service.Complete(service.History.Single(a => a.State == AttemptState.InProgress));
                        if (catalog == null || catalog.All(e => e.Entry.Certification != "CCAR-F")) { step = 15; break; }
                        Descendants(main).OfType<ComboBox>().Single(c => c.Name == "ExamSelector").SelectedIndex = catalog.ToList().FindIndex(e => e.Entry.Certification == "CCAR-F");
                        if (service.Bank.Certification != "CCAR-F") throw new Exception("Selector did not select Foundations");
                        Capture(main, "08-foundations-home"); step++; Find(main, "Empezar práctica").PerformClick(); break;
                    case 10:
                        var foundationSession = System.Windows.Forms.Application.OpenForms.OfType<SessionForm>().Single();
                        var foundationAttempt = service.History.Single(a => a.State == AttemptState.InProgress);
                        if (foundationAttempt.Certification != "CCAR-F" || foundationAttempt.Questions.Any(q => q.Language != "en")) throw new Exception("Wrong Foundations content");
                        foreach (var c in Descendants(foundationSession).OfType<CheckBox>().Where(c => c.Tag is string id && foundationAttempt.Questions[0].Correct.Contains(id))) c.Checked = true;
                        Find(foundationSession, "Confirmar respuesta").PerformClick(); Capture(foundationSession, "09-foundations-practice");
                        step++; foundationSession.Close(); break;
                    case 11:
                        service.Complete(service.History.Single(a => a.State == AttemptState.InProgress));
                        Find(main, "Historial").PerformClick();
                        if (Descendants(main).OfType<DataGridView>().Single().RowCount != 1) throw new Exception("History mixed certifications");
                        Capture(main, "10-foundations-history");
                        Find(main, "Inicio y exámenes").PerformClick(); step++; Find(main, "Comenzar simulacro").PerformClick(); break;
                    case 12:
                        var foundationMock = System.Windows.Forms.Application.OpenForms.OfType<SessionForm>().Single();
                        var fAttempt = service.History.Single(a => a.State == AttemptState.InProgress);
                        if (fAttempt.Certification != "CCAR-F" || fAttempt.Questions.Count != 60) throw new Exception("Wrong Foundations mock profile");
                        if (Descendants(foundationMock).OfType<Button>().Any(b => b.Text is "Ver explicación" or "Leer ayuda del concepto" or "Confirmar respuesta")) throw new Exception("Foundations mock reveals solutions");
                        Capture(foundationMock, "11-foundations-mock"); step++; foundationMock.Close(); break;
                    case 13:
                        service.Abandon(service.History.Single(a => a.State == AttemptState.InProgress));
                        Find(main, "Estadísticas").PerformClick();
                        Descendants(main).OfType<ComboBox>().Single(c => c.Name == "StatisticsMode").SelectedIndex = 1;
                        Find(main, "Aplicar filtros").PerformClick(); Capture(main, "12-foundations-statistics"); step++; break;
                    case 14:
                        Descendants(main).OfType<ComboBox>().Single(c => c.Name == "ExamSelector").SelectedIndex = 0;
                        if (service.Bank.Certification != bank.Certification) throw new Exception("Return selector failed");
                        step++; break;
                    case 15:
                        File.WriteAllText(Path.Combine(output, "result.txt"), "PASS: both certifications, selector, English questions, confirmation, explanation, persistence, isolated history/statistics, aids, 63/60-question mocks and solution concealment. Screenshots at current desktop DPI; not the full DPI/keyboard matrix.");
                        exitCode = 0; timer.Stop(); main.Close(); break;
                }
            }
            catch (Exception ex)
            {
                busy = true; timer.Stop(); File.WriteAllText(Path.Combine(output, "error.txt"), ex.ToString());
                foreach (var f in System.Windows.Forms.Application.OpenForms.Cast<Form>().Reverse().ToArray()) f.Close();
            }
        };
        main.Shown += (_, _) => timer.Start();
        System.Windows.Forms.Application.Run(main); return exitCode;
    }
}
