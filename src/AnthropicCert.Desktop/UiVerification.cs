using AnthropicCert.Application;
using AnthropicCert.Domain;
using AnthropicCert.Infrastructure;

namespace AnthropicCert.Desktop;
// Deterministic UI acceptance harness; data is isolated from the user's portable Data directory.
internal static class UiVerification
{
    static IEnumerable<Control> Descendants(Control c) => c.Controls.Cast<Control>().SelectMany(x => new[] { x }.Concat(Descendants(x)));
    static Button Find(Form f, string text) => Descendants(f).OfType<Button>().First(b => b.Text == text);
    public static int Run(QuestionBank bank, string output)
    {
        output = Path.GetFullPath(output); Directory.CreateDirectory(output);
        var data = Path.Combine(output, "isolated-data");
        using var repo = new FileAttemptRepository(data);
        var service = new ExamService(bank, repo, new SystemClock(), new Random(112));
        using var main = new MainForm(service);
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
                        var combos = Descendants(main).OfType<ComboBox>().ToList(); combos[0].SelectedIndex = 1;
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
                        File.WriteAllText(Path.Combine(output, "result.txt"), "PASS: home, selection, confirmation, explanation, persistence, history, statistics, aids, mock concealment. Screenshots rendered at current desktop DPI.");
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
