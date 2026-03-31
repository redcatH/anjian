using System;
using System.Drawing;
using System.Windows.Forms;

namespace Anjian;

internal static class RegionSelectionService
{
    public static Rectangle? PickRegion(IWin32Window? owner, Rectangle? initialRegion = null)
    {
        using var overlay = new ScreenRegionSelectionOverlay(initialRegion);
        return overlay.ShowDialog(owner) == DialogResult.OK ? overlay.SelectedRegion : null;
    }
}

internal sealed class ScreenRegionSelectionOverlay : Form
{
    private Point? _startPoint;
    private Point _currentPoint;
    private Rectangle? _initialRegion;

    public ScreenRegionSelectionOverlay(Rectangle? initialRegion = null)
    {
        var bounds = ScreenCaptureService.GetVirtualScreenBounds();

        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = bounds;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.Black;
        Opacity = 0.25;
        Cursor = Cursors.Cross;
        KeyPreview = true;
        _initialRegion = initialRegion;
    }

    public Rectangle? SelectedRegion { get; private set; }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Capture = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _startPoint = PointToScreen(e.Location);
        _currentPoint = _startPoint.Value;
        _initialRegion = null;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_startPoint is null)
        {
            return;
        }

        _currentPoint = PointToScreen(e.Location);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left || _startPoint is null)
        {
            return;
        }

        _currentPoint = PointToScreen(e.Location);
        var region = NormalizeRectangle(_startPoint.Value, _currentPoint);
        _startPoint = null;

        if (region.Width <= 0 || region.Height <= 0)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        SelectedRegion = region;
        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Rectangle? drawRegion = null;
        if (_startPoint is not null)
        {
            drawRegion = NormalizeRectangle(_startPoint.Value, _currentPoint);
        }
        else if (_initialRegion is not null)
        {
            drawRegion = _initialRegion;
        }

        if (drawRegion is null)
        {
            return;
        }

        var region = drawRegion.Value;
        var localRegion = new Rectangle(region.X - Left, region.Y - Top, region.Width, region.Height);

        using var pen = new Pen(Color.LimeGreen, 2);
        using var brush = new SolidBrush(Color.FromArgb(80, Color.DeepSkyBlue));
        using var labelBrush = new SolidBrush(Color.White);
        using var labelBackBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
        using var font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

        e.Graphics.FillRectangle(brush, localRegion);
        e.Graphics.DrawRectangle(pen, localRegion);

        var label = $"X={region.X}, Y={region.Y}, W={region.Width}, H={region.Height}";
        var labelSize = e.Graphics.MeasureString(label, font);
        var labelRect = new RectangleF(
            localRegion.X,
            Math.Max(0, localRegion.Y - labelSize.Height - 6),
            labelSize.Width + 10,
            labelSize.Height + 4);

        e.Graphics.FillRectangle(labelBackBrush, labelRect);
        e.Graphics.DrawString(label, font, labelBrush, labelRect.X + 5, labelRect.Y + 2);
    }

    private static Rectangle NormalizeRectangle(Point p1, Point p2)
    {
        var left = Math.Min(p1.X, p2.X);
        var top = Math.Min(p1.Y, p2.Y);
        var right = Math.Max(p1.X, p2.X);
        var bottom = Math.Max(p1.Y, p2.Y);
        return Rectangle.FromLTRB(left, top, right, bottom);
    }
}
