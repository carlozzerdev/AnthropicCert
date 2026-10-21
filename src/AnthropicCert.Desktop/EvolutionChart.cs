using AnthropicCert.Domain;
namespace AnthropicCert.Desktop;
internal sealed class EvolutionChart : Control
{
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<Attempt> Attempts { get; set; } = [];
    public EvolutionChart() { DoubleBuffered = true; BackColor = Color.White; AccessibleName = "Evolución de notas por intento"; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var g = e.Graphics; g.Clear(BackColor);
        g.DrawString("Evolución · nota sobre 100 (últimos 20 intentos)", Font, Brushes.Black, 14, 8);
        var points = Attempts.TakeLast(20).ToList();
        if (points.Count == 0) { g.DrawString("Sin datos. Finaliza un examen con estos filtros.", Font, Brushes.Gray, 30, 50); return; }
        var area = new Rectangle(45, 42, Math.Max(10, Width - 85), Math.Max(10, Height - 108));
        using var gridPen = new Pen(Color.FromArgb(225, 230, 230));
        foreach (var n in new[] { 0, 50, 100 }) { var y = area.Bottom - area.Height * n / 100; g.DrawLine(gridPen, area.Left, y, area.Right, y); g.DrawString(n.ToString(), Font, Brushes.Gray, 5, y - 8); }
        PointF Point(int index) => new(area.Left + (points.Count == 1 ? area.Width / 2f : index * area.Width / (float)(points.Count - 1)), area.Bottom - (float)points[index].Result!.Percentage * area.Height / 100);
        for (var i = 0; i < points.Count; i++)
        {
            var p = Point(i); var a = points[i];
            using var brush = new SolidBrush(a.Mode == ExamMode.Mock ? Ui.Accent : Color.DarkOrange);
            if (i > 0 && points[i - 1].Mode == a.Mode && points[i - 1].BankVersion == a.BankVersion && points[i - 1].Profile == a.Profile)
            { using var pen = new Pen(brush, 2); g.DrawLine(pen, Point(i - 1), p); }
            g.FillEllipse(brush, p.X - 4, p.Y - 4, 8, 8);
            if (points.Count <= 8) g.DrawString($"{a.Result!.Percentage:0}%", Font, brush, p.X - 14, p.Y - 20);
        }
        using var small = new Font("Segoe UI", 9);
        g.DrawString($"{points[0].Finished!.Value.LocalDateTime:g} → {points[^1].Finished!.Value.LocalDateTime:g}   ·   Verde: simulacro / Naranja: práctica\nConsulta fechas, tamaño, modo y versión en Historial.", small, Brushes.DimGray, 15, Math.Max(45, Height - 52));
    }
}
