using AnthropicCert.Application;
using AnthropicCert.Domain;

namespace AnthropicCert.Desktop;
internal sealed class SessionForm : Form
{
    readonly SessionPresenter presenter;
    readonly Label status = Ui.Label("");
    readonly FlowLayoutPanel content = Ui.Column();
    readonly System.Windows.Forms.Timer timer = new() { Interval = 1000 };
    bool rendering;
    public SessionForm(ExamService service, Attempt attempt)
    {
        presenter = new(service, attempt); Ui.Style(this, $"AnthropicCert · {attempt.Certification} · {attempt.BankVersion}");
        status.Dock = DockStyle.Top; status.Padding = new Padding(16, 4, 8, 4); status.MaximumSize = Size.Empty;
        status.AutoSize = false; status.Height = 62;
        Controls.Add(content); Controls.Add(status);
        timer.Tick += (_, _) =>
        {
            try { if (presenter.Tick() || presenter.Attempt.State != AttemptState.InProgress) { timer.Stop(); Render(); ShowResult(); } else status.Text = presenter.Status(); }
            catch (Exception ex) { timer.Stop(); MessageBox.Show(ex.Message, "No se pudo actualizar el intento", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        FormClosing += (_, _) => { timer.Stop(); if (attempt.State == AttemptState.InProgress) Ui.Guard(() => presenter.Move(attempt.Position)); };
        FormClosed += (_, _) => timer.Dispose();
        Render(); if (attempt.State == AttemptState.InProgress) timer.Start();
    }
    IEnumerable<string> Selection() => content.Controls.OfType<CheckBox>().Where(c => c.Checked).Select(c => (string)c.Tag!);
    void Render()
    {
        rendering = true; content.SuspendLayout();
        foreach (Control control in content.Controls.Cast<Control>().ToArray()) control.Dispose();
        var q = presenter.Current; var answer = presenter.Response;
        status.Text = presenter.Status();
        content.Controls.Add(Ui.Label(presenter.DomainTitle, 14));
        content.Controls.Add(Ui.Label(q.Text, 14));
        content.Controls.Add(Ui.Label(q.Correct.Count == 1 ? "Select one answer." : $"Select {q.Correct.Count} answers. Select the exact set."));
        var flag = new CheckBox { Text = "Marcar para revisar", AutoSize = true, Checked = answer.Flagged, Enabled = presenter.Editable, Margin = new Padding(5, 12, 5, 12) };
        foreach (var o in q.Options)
        {
            var check = new CheckBox { Text = $"{o.Id}. {o.Text}", Tag = o.Id, AutoSize = true,
                MaximumSize = new Size(1050, 0), Padding = new Padding(10), Margin = new Padding(4),
                BackColor = Color.White, Checked = answer.Selected.Contains(o.Id), Enabled = presenter.Editable, UseMnemonic = false,
                AccessibleName = $"Opción {o.Id}" };
            check.CheckedChanged += (_, _) =>
            {
                if (rendering) return;
                Ui.Guard(() =>
                {
                    if (q.Correct.Count == 1 && check.Checked)
                    {
                        rendering = true;
                        foreach (var other in content.Controls.OfType<CheckBox>().Where(c => c != check)) other.Checked = false;
                        rendering = false;
                    }
                    presenter.Select(Selection(), false, flag.Checked); status.Text = presenter.Status();
                });
            };
            content.Controls.Add(check);
        }
        // Keep the flag nested so it can never be confused with answer checkboxes.
        var flagRow = Ui.Row(); flagRow.Controls.Add(flag); content.Controls.Add(flagRow);
        flag.CheckedChanged += (_, _) => { if (!rendering) Ui.Guard(() => presenter.Select(Selection(), false, flag.Checked)); };
        var actions = Ui.Row();
        if (presenter.Editable && presenter.Attempt.Mode == ExamMode.Practice)
            actions.Controls.Add(Ui.Button("Confirmar respuesta", () => { presenter.Select(Selection(), true, flag.Checked); Render(); }, true));
        if (presenter.Revealed)
        {
            var correct = ExamRules.IsCorrect(q, answer);
            content.Controls.Add(Ui.Label(answer.Selected.Count == 0 ? "SIN RESPONDER" : correct ? "ACIERTO · 1 punto" : "FALLO · 0 puntos", 13));
            actions.Controls.Add(Ui.Button("Ver explicación", () => Ui.ReadText("Corrección y razonamiento", presenter.Explanation(), q.References, this), true));
            actions.Controls.Add(Ui.Button("Leer ayuda del concepto", () => Ui.ReadText(q.Help.Title, q.Help.Text, q.Help.References, this)));
        }
        content.Controls.Add(actions);
        var navigation = Ui.Row();
        var previous = Ui.Button("← Anterior", () => { presenter.Move(presenter.Attempt.Position - 1); Render(); }); previous.Enabled = presenter.Attempt.Position > 0;
        var next = Ui.Button("Siguiente →", () => { presenter.Move(presenter.Attempt.Position + 1); Render(); }); next.Enabled = presenter.Attempt.Position < presenter.Attempt.Questions.Count - 1;
        navigation.Controls.Add(previous); navigation.Controls.Add(next);
        navigation.Controls.Add(Ui.Button("Ir a pregunta…", Navigate));
        if (presenter.Attempt.State == AttemptState.InProgress)
            navigation.Controls.Add(Ui.Button("Finalizar y puntuar", () =>
            {
                if (MessageBox.Show("¿Finalizar el intento? Las preguntas pendientes puntuarán 0.", "Finalizar examen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                presenter.Complete(); timer.Stop(); Render(); ShowResult();
            }));
        else if (presenter.Attempt.State == AttemptState.Completed) navigation.Controls.Add(Ui.Button("Ver resultado", ShowResult));
        navigation.Controls.Add(Ui.Button("Guardar y volver", Close)); content.Controls.Add(navigation);
        content.Controls.Add(Ui.Label($"{q.Id} · r{q.Revision} · Idioma: {q.Language}\nLas preguntas son material de práctica independiente. Nota sobre 100, sin equivalencia a la puntuación oficial.", 9));
        AdaptWidth(); content.ResumeLayout(true); rendering = false;
    }
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e); if (content != null) AdaptWidth();
    }
    void AdaptWidth()
    {
        var width = Math.Max(300, content.ClientSize.Width - 65);
        foreach (var label in content.Controls.OfType<Label>()) label.MaximumSize = new Size(width, 0);
        foreach (var check in content.Controls.OfType<CheckBox>())
        {
            // Native checkbox AutoSize measures a single line even with MaximumSize.
            // Give wrapped text an explicit measured height, including glyph and padding.
            check.AutoSize = false; check.MaximumSize = Size.Empty; check.Width = width;
            var text = TextRenderer.MeasureText(check.Text, check.Font,
                new Size(Math.Max(100, width - check.Padding.Horizontal - 32), int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            check.Height = Math.Max(44, text.Height + check.Padding.Vertical + 8);
        }
        foreach (var row in content.Controls.OfType<FlowLayoutPanel>()) row.MaximumSize = new Size(width, 0);
    }
    void ShowResult()
    {
        var a = presenter.Attempt; var s = a.Result;
        if (s == null) return;
        var domains = a.Questions.Select(q => q.DomainId).Distinct().Select(d =>
        { var ds = ExamRules.Score(a, d); return $"{d}: {ds.Correct}/{ds.Total} · {Ui.Percent(ds.Percentage)}"; });
        Ui.ReadText("Resultado del examen", $"PUNTUACIÓN: {s.Correct}/{s.Total} puntos · {s.Percentage:0.0}/100\n\n" +
            $"Aciertos: {s.Correct}\nFallos: {s.Wrong}\nSin responder: {s.Blank}\nPrecisión en contestadas: {Ui.Percent(s.Precision)}\n" +
            $"Tiempo: {Ui.Duration(s.Seconds)}\nMedia por pregunta: {s.AverageSeconds:0.0} s\n\n" +
            string.Join("\n", domains) + "\n\nUsa Anterior/Siguiente para revisar cada pregunta y su ayuda.", owner: this);
    }
    void Navigate()
    {
        using var dialog = new Form(); Ui.Style(dialog, "Navegación del intento"); dialog.Size = new Size(720, 650);
        var list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        for (var i = 0; i < presenter.Attempt.Questions.Count; i++)
        {
            var r = presenter.Attempt.Answer(presenter.Attempt.Questions[i]);
            list.Items.Add($"Pregunta {i + 1:00} · {(r.Selected.Count > 0 ? "Con selección" : "Pendiente")} {(r.Flagged ? "· MARCADA" : "")}");
        }
        list.SelectedIndex = presenter.Attempt.Position;
        var go = Ui.Button("Ir a seleccionada", () => { presenter.Move(list.SelectedIndex); dialog.Close(); Render(); }); go.Dock = DockStyle.Bottom;
        dialog.Controls.Add(list); dialog.Controls.Add(go); dialog.ShowDialog(this);
    }
}
