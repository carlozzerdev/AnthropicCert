using AnthropicCert.Application;
using AnthropicCert.Domain;
using AnthropicCert.Infrastructure;

namespace AnthropicCert.Desktop;
internal sealed class MainForm : Form
{
    readonly ExamService exams;
    readonly IReadOnlyList<AvailableExam> catalog;
    readonly ComboBox examSelector = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 430, Name = "ExamSelector", AccessibleName = "Certificación" };
    AvailableExam Selected => catalog[examSelector.SelectedIndex];
    readonly Panel body = new() { Dock = DockStyle.Fill, Padding = new Padding(12) };
    public MainForm(ExamService exams, IReadOnlyList<AvailableExam>? catalog = null)
    {
        this.exams = exams;
        this.catalog = catalog ?? [new(new ExamEntry { Certification = exams.Bank.Certification, Name = exams.Bank.Certification }, exams.Bank, null)];
        Ui.Style(this, "AnthropicCert · Preparación Architect");
        var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Ui.Ink, Padding = new Padding(20, 10, 20, 10) };
        header.Controls.Add(new Label { Text = "ANTHROPICCERT   /   ARCHITECT\nPractica · comprende · mide tu progreso", Dock = DockStyle.Fill,
            ForeColor = Color.White, Font = new Font("Segoe UI", 15), AutoSize = false });
        var nav = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12, 5, 12, 5), BackColor = Color.White };
        nav.Controls.Add(Ui.Button("Inicio y exámenes", Home, true)); nav.Controls.Add(Ui.Button("Historial", History));
        nav.Controls.Add(Ui.Button("Estadísticas", () => { RequireBank(); Stats(); })); nav.Controls.Add(Ui.Button("Ayudas", () => { RequireBank(); Helps(); }));
        nav.Controls.Add(Ui.Button("Acerca del banco", () => { RequireBank(); About(); }));
        var chooser = Ui.Row(); chooser.Dock = DockStyle.Top; chooser.BackColor = Color.White;
        chooser.Controls.Add(Ui.Label("Certificación:")); chooser.Controls.Add(examSelector);
        examSelector.Items.AddRange(this.catalog.Cast<object>().ToArray());
        examSelector.SelectedIndex = Math.Max(0, this.catalog.ToList().FindIndex(e => e.Bank == exams.Bank));
        examSelector.SelectedIndexChanged += (_, _) => Ui.Guard(() =>
        {
            if (Selected.Bank is { } bank) exams.SelectBank(bank);
            Home();
        });
        Controls.Add(body); Controls.Add(nav); Controls.Add(chooser); Controls.Add(header); Home();
    }
    void RequireBank()
    {
        if (Selected.Bank == null) throw new InvalidOperationException(Selected.Error ?? "Banco pendiente de incorporación.");
    }
    void Clear()
    {
        foreach (Control c in body.Controls.Cast<Control>().ToArray()) c.Dispose();
    }
    void Home()
    {
        Clear(); var column = Ui.Column(); body.Controls.Add(column);
        column.Controls.Add(Ui.Label("Tu próxima sesión de estudio", 22));
        column.Controls.Add(Ui.Label(Selected.ToString(), 15));
        if (Selected.Bank is { } selectedBank)
            column.Controls.Add(Ui.Label($"{selectedBank.Questions.Count} preguntas · {selectedBank.Domains.Count} dominios · banco {selectedBank.Version} · {selectedBank.ProfileLabel}", 12));
        column.Controls.Add(Ui.Label("Practica con corrección inmediata o haz un simulacro cronometrado. Lee el razonamiento y las ayudas para entender cada decisión."));
        var pending = exams.History.FirstOrDefault(a => a.State == AttemptState.InProgress);
        if (pending != null)
        {
            column.Controls.Add(Ui.Label($"Tienes un intento pendiente: {pending.Certification} · {Ui.Mode(pending.Mode)}, {pending.Questions.Count} preguntas.", 13));
            var pendingActions = Ui.Row();
            pendingActions.Controls.Add(Ui.Button("Reanudar intento", () => Open(pending.Id), true));
            pendingActions.Controls.Add(Ui.Button("Abandonar intento", () =>
            {
                if (MessageBox.Show("¿Abandonar sin guardar puntuación? El intento permanecerá en el historial.", "Abandonar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                { exams.Abandon(pending); Home(); }
            })); column.Controls.Add(pendingActions);
        }
        if (Selected.Bank == null)
        {
            column.Controls.Add(Ui.Label("Banco pendiente o no válido: " + Selected.Error));
            return;
        }
        column.Controls.Add(Ui.Label("Configura tu práctica", 16));
        var domain = Ui.Combo(new[] { "Todos los dominios" }.Concat(exams.Bank.Domains.Select(d => $"{d.Id} · {d.Title}")));
        domain.Width = 720; column.Controls.Add(domain);
        var amountRow = Ui.Row(); amountRow.Controls.Add(Ui.Label("Preguntas:"));
        var count = new NumericUpDown { Minimum = 1, Maximum = exams.Bank.Questions.Count, Value = Math.Min(20, exams.Bank.Questions.Count), Width = 100, Margin = new Padding(8) };
        amountRow.Controls.Add(count); column.Controls.Add(amountRow);
        var actions = Ui.Row();
        actions.Controls.Add(Ui.Button("Empezar práctica", () =>
        { var id = domain.SelectedIndex == 0 ? null : exams.Bank.Domains[domain.SelectedIndex - 1].Id; Start(ExamMode.Practice, (int)count.Value, id); }, true));
        actions.Controls.Add(Ui.Button("Practicar errores", () =>
        {
            var errors = exams.Errors(); if (errors.Count == 0) throw new InvalidOperationException("Todavía no hay errores en exámenes finalizados.");
            var id = domain.SelectedIndex == 0 ? null : exams.Bank.Domains[domain.SelectedIndex - 1].Id;
            var available = exams.Bank.Questions.Count(q => errors.Contains(q.Id) && (id == null || q.DomainId == id));
            if (available == 0) throw new InvalidOperationException("No hay errores disponibles en este dominio.");
            Start(ExamMode.Practice, Math.Min((int)count.Value, available), id, errors);
        })); column.Controls.Add(actions);
        column.Controls.Add(Ui.Label("Simulacro de práctica", 16));
        column.Controls.Add(Ui.Label($"{exams.Bank.MockQuestionCount} preguntas · {exams.Bank.DurationMinutes} minutos · selección por pesos de dominio · respuestas ocultas hasta finalizar.\n{exams.Bank.ProfileStatus}"));
        column.Controls.Add(Ui.Button("Comenzar simulacro", () => Start(ExamMode.Mock, exams.Bank.MockQuestionCount), true));
        column.Controls.Add(Ui.Label("Puntuación: 1 punto por acierto, 0 por fallo o sin responder. En preguntas múltiples debes acertar todo el conjunto.\nLas preguntas importadas están en inglés; las ayudas locales están en español. No requiere cuenta ni API de IA.", 10));
    }
    void Start(ExamMode mode, int count, string? domain = null, IReadOnlySet<string>? ids = null)
    { var a = exams.Start(mode, count, domain, ids); using var f = new SessionForm(exams, a); f.ShowDialog(this); Home(); }
    void Open(Guid id)
    {
        var a = exams.Resume(id);
        if (a.State == AttemptState.Completed && exams.History.Any(x => x.Mode == ExamMode.Mock && x.State == AttemptState.InProgress))
            throw new InvalidOperationException("Finaliza o abandona el simulacro activo antes de revisar soluciones de otros intentos.");
        using var f = new SessionForm(exams, a); f.ShowDialog(this); History();
    }
    void History()
    {
        Clear(); var grid = Ui.Grid(); var attempts = exams.History.Where(a => a.Certification == Selected.Entry.Certification).OrderByDescending(a => a.Started).ToList();
        grid.DataSource = attempts.Select(a => new
        {
            Examen = a.Certification, Fecha = a.Started.LocalDateTime.ToString("g"), Modo = Ui.Mode(a.Mode), Estado = Ui.State(a.State),
            Preguntas = a.Questions.Count, Aciertos = a.Result?.Correct.ToString() ?? "—",
            Fallos = a.Result?.Wrong.ToString() ?? "—", SinResponder = a.Result?.Blank.ToString() ?? "—",
            Puntos = a.Result == null ? "—" : $"{a.Result.Correct}/{a.Result.Total}",
            Nota = a.Result == null ? "—" : $"{a.Result.Percentage:0.0}/100", Tiempo = a.Result == null ? "—" : Ui.Duration(a.Result.Seconds), Banco = a.BankVersion
        }).ToList();
        grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && attempts[e.RowIndex].State != AttemptState.Abandoned) Ui.Guard(() => Open(attempts[e.RowIndex].Id)); };
        var buttons = Ui.Row(); buttons.Dock = DockStyle.Bottom;
        buttons.Controls.Add(Ui.Button("Abrir intento seleccionado", () =>
        {
            if (grid.CurrentRow == null) return;
            var a = attempts[grid.CurrentRow.Index];
            if (a.State == AttemptState.Abandoned) throw new InvalidOperationException("El intento se abandonó sin resultado final.");
            Open(a.Id);
        }, true));
        var title = Ui.Label("Historial · selecciona un intento para reanudarlo o revisar sus respuestas", 15); title.Dock = DockStyle.Top;
        body.Controls.Add(grid); body.Controls.Add(buttons); body.Controls.Add(title);
    }
    void Helps()
    {
        if (exams.History.Any(a => a.State == AttemptState.InProgress && a.Mode == ExamMode.Mock))
            throw new InvalidOperationException("Finaliza o abandona el simulacro activo antes de consultar las ayudas.");
        Clear(); var list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        var helps = exams.Bank.Questions.Select(q => q.Help).DistinctBy(h => h.Id).OrderBy(h => h.DomainId).ThenBy(h => h.Title).ToList();
        list.Items.AddRange(helps.Select(h => $"{h.DomainId} · {h.Title}").Cast<object>().ToArray());
        void Read() { if (list.SelectedIndex >= 0) { var h = helps[list.SelectedIndex]; Ui.ReadText(h.Title, h.Text, h.References, this); } }
        list.DoubleClick += (_, _) => Ui.Guard(Read);
        var button = Ui.Button("Leer ayuda seleccionada", Read, true); button.Dock = DockStyle.Bottom;
        body.Controls.Add(list); body.Controls.Add(button);
    }
    void About() => Ui.ReadText("Contenido y procedencia", $"ANTHROPICCERT 1.1\n\n{exams.Bank.Certification} · Banco {exams.Bank.Version}: {exams.Bank.Questions.Count} preguntas activas.\n" +
        "Aplicación independiente, no afiliada ni avalada por Anthropic. No contiene preguntas del examen real.\n\n" +
        "Incluye adaptaciones del texto MIT de Amey Thakur y preguntas originales del proyecto, con fuentes y revisión identificadas en cada ítem. " +
        "Las entradas repetidas o pendientes de contraste se conservan en el archivo de fuentes y no se ofrecen como nuevas preguntas.\n\n" +
        "La revisión técnica realizada por el agente no equivale a una revisión humana independiente ni a validación oficial. El objetivo de 350 Professional sigue siendo una ampliación pendiente.\n\n" +
        "El ZIP incluye el catálogo de fuentes, el informe de revisión y sus licencias en Content/Sources. Las webs sin permiso de reproducción se conservan como enlaces externos.\n\n" +
        "Tus intentos se guardan en Data junto al ejecutable. Cierra la aplicación antes de copiar la carpeta. Para actualizar, conserva Data.\n\n" +
        exams.Bank.ProfileStatus, owner: this);
    void Stats()
    {
        Clear(); var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        var filters = Ui.Row(); filters.Dock = DockStyle.Fill;
        var mode = Ui.Combo(["Simulacros", "Prácticas", "Ambos modos"]); mode.Width = 140; mode.Name = "StatisticsMode";
        var versions = new[] { exams.Bank.Version }.Concat(exams.SelectedHistory.Select(a => a.BankVersion)).Distinct().ToArray();
        var version = Ui.Combo(versions.Prepend("Todas las versiones")); version.Width = 170; version.SelectedIndex = 1;
        var profiles = exams.SelectedHistory.Select(a => a.Profile).Prepend(exams.Bank.ProfileLabel).Distinct().ToArray();
        var profile = Ui.Combo(profiles); profile.Width = 240;
        var from = new DateTimePicker { ShowCheckBox = true, Checked = false, Format = DateTimePickerFormat.Short, Width = 160 };
        var to = new DateTimePicker { ShowCheckBox = true, Checked = false, Format = DateTimePickerFormat.Short, Width = 160 };
        filters.Controls.AddRange([mode, version, profile, Ui.Label("Desde"), from, Ui.Label("Hasta"), to]);
        var summary = Ui.Label("", 11); summary.Dock = DockStyle.Fill;
        var grid = Ui.Grid(); var chart = new EvolutionChart { Dock = DockStyle.Fill };
        void RefreshStats()
        {
            var start = from.Checked ? new DateTimeOffset(from.Value.Date).ToUniversalTime() : (DateTimeOffset?)null;
            var end = to.Checked ? new DateTimeOffset(to.Value.Date.AddDays(1)).ToUniversalTime() : (DateTimeOffset?)null;
            if (start >= end) throw new InvalidOperationException("La fecha inicial debe ser anterior o igual a la final.");
            var filter = new StatisticsFilter(mode.SelectedIndex == 2 ? null : mode.SelectedIndex == 0 ? ExamMode.Mock : ExamMode.Practice,
                start, end, version.SelectedIndex == 0 ? null : versions[version.SelectedIndex - 1], (string)profile.SelectedItem!);
            var s = Statistics.Calculate(exams.History, exams.Bank, filter);
            summary.Text = $"Exámenes finalizados: {s.Attempts.Count} · Preguntas evaluadas: {s.Total} · Aciertos: {s.Correct} · Fallos: {s.Wrong} · Sin responder: {s.Blank}\n" +
                $"Global por preguntas: {Ui.Percent(s.Global)}   |   Media por examen: {Ui.Percent(s.Mean)}   |   Mejor: {Ui.Percent(s.Best)}   |   Última: {Ui.Percent(s.Last)}\n" +
                $"Tiempo total: {Ui.Duration(s.Seconds)} · Media/examen: {(s.Attempts.Count == 0 ? "Sin datos" : Ui.Duration(s.Seconds / s.Attempts.Count))} · Media/pregunta: {(s.Total == 0 ? "Sin datos" : $"{s.Seconds / s.Total:0.0} s")}\n" +
                (version.SelectedIndex == 1 ? $"Cobertura del banco actual: {s.Unique}/{s.Eligible} preguntas distintas." : "Cobertura única: selecciona la versión actual del banco.");
            grid.DataSource = s.Domains.OrderBy(d => d.Score.Total == 0 ? double.MaxValue : d.Score.Percentage)
                .ThenByDescending(d => d.Score.Wrong).ThenBy(d => d.Id).Select(d => new
                {
                    Dominio = exams.Bank.Domains.Single(x => x.Id == d.Id).Title, Preguntas = d.Score.Total,
                    Aciertos = d.Score.Correct, Fallos = d.Score.Wrong, SinResponder = d.Score.Blank,
                    Porcentaje = d.Score.Total == 0 ? "Sin datos" : Ui.Percent(d.Score.Percentage),
                    Repaso = d.Score.Total == 0 ? "Sin datos" : d.Score.Total < 10 ? "Muestra reducida" : "Menor porcentaje = mayor prioridad"
                }).ToList();
            chart.Attempts = s.Attempts; chart.Invalidate();
        }
        filters.Controls.Add(Ui.Button("Aplicar filtros", RefreshStats, true));
        layout.Controls.Add(filters, 0, 0); layout.Controls.Add(summary, 0, 1); layout.Controls.Add(grid, 0, 2); layout.Controls.Add(chart, 0, 3);
        layout.SizeChanged += (_, _) =>
        {
            filters.MaximumSize = new Size(Math.Max(600, layout.ClientSize.Width - 12), 0);
            summary.MaximumSize = new Size(Math.Max(600, layout.ClientSize.Width - 12), 0);
        };
        body.Controls.Add(layout); RefreshStats();
    }
}
