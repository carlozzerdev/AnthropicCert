using System.Diagnostics;
using AnthropicCert.Domain;

namespace AnthropicCert.Desktop;
internal static class Ui
{
    public static readonly Color Ink = Color.FromArgb(34, 48, 60);
    public static readonly Color Accent = Color.FromArgb(25, 102, 109);
    public static readonly Color Paper = Color.FromArgb(245, 246, 243);
    public static void Style(Form f, string title)
    {
        f.Text = title; f.Font = new Font("Segoe UI", 10); f.BackColor = Paper; f.ForeColor = Ink;
        f.StartPosition = FormStartPosition.CenterScreen; f.AutoScaleMode = AutoScaleMode.Dpi;
        f.Size = new Size(1240, 850); f.MinimumSize = new Size(900, 650);
    }
    public static Button Button(string text, Action action, bool primary = false)
    {
        var b = new Button { Text = text, AutoSize = true, MinimumSize = new Size(110, 38), Padding = new Padding(10, 4, 10, 4),
            FlatStyle = FlatStyle.Flat, Margin = new Padding(4), BackColor = primary ? Accent : Color.White,
            ForeColor = primary ? Color.White : Ink, AccessibleName = text };
        b.Click += (_, _) => Guard(action); return b;
    }
    public static void Guard(Action action)
    {
        try { action(); }
        catch (Exception e) { MessageBox.Show(e.Message, "No se pudo completar la acción", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    public static Label Label(string text, int size = 11) => new() { Text = text, AutoSize = true, UseMnemonic = false,
        MaximumSize = new Size(1080, 0), Font = new Font("Segoe UI", size), Margin = new Padding(4, 8, 4, 8) };
    public static FlowLayoutPanel Column() => new() { Dock = DockStyle.Fill, AutoScroll = true,
        FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20) };
    public static FlowLayoutPanel Row() => new() { AutoSize = true, WrapContents = true, Dock = DockStyle.Top };
    public static ComboBox Combo(IEnumerable<string> values)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 330, Margin = new Padding(4, 8, 4, 8),
            DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 26, DropDownWidth = 740 };
        c.DrawItem += (_, e) =>
        {
            e.DrawBackground();
            if (e.Index >= 0) TextRenderer.DrawText(e.Graphics, c.Items[e.Index]?.ToString() ?? "", c.Font, e.Bounds,
                e.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            e.DrawFocusRectangle();
        };
        c.Items.AddRange(values.Cast<object>().ToArray()); c.SelectedIndex = 0; return c;
    }
    public static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false,
        BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoGenerateColumns = true,
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } };
    public static string Percent(double? n) => n is null ? "Sin datos" : $"{n:0.0}%";
    public static string Duration(double seconds)
    {
        var span = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return $"{(long)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}";
    }
    public static string Mode(ExamMode mode) => mode == ExamMode.Mock ? "Simulacro" : "Práctica";
    public static string State(AttemptState state) => state switch { AttemptState.Completed => "Finalizado", AttemptState.Abandoned => "Abandonado", _ => "En curso" };
    public static void OpenReference(Reference reference)
    {
        if (!Uri.TryCreate(reference.Url, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
            throw new InvalidOperationException("Solo se permiten enlaces HTTP/HTTPS.");
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }
    public static void ReadText(string title, string text, IEnumerable<Reference>? references = null, IWin32Window? owner = null)
    {
        using var dialog = new Form(); Style(dialog, title); dialog.Size = new Size(960, 740);
        var box = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill, Text = text.Replace("\n", Environment.NewLine), BackColor = Color.White, Font = new Font("Segoe UI", 11) };
        var links = Row(); links.Dock = DockStyle.Bottom;
        foreach (var r in references ?? []) links.Controls.Add(Button(r.Title, () => OpenReference(r)));
        dialog.Controls.Add(box); dialog.Controls.Add(links); dialog.ShowDialog(owner);
    }
}
