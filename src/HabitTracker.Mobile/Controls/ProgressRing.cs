namespace HabitTracker.Mobile.Controls;

// Circular progress indicator. Progress is a percentage from 0 to 100.
public class ProgressRing : GraphicsView
{
    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
        nameof(Progress), typeof(double), typeof(ProgressRing), 0.0,
        propertyChanged: (b, _, _) => ((ProgressRing)b).Invalidate());

    public static readonly BindableProperty RingColorProperty = BindableProperty.Create(
        nameof(RingColor), typeof(Color), typeof(ProgressRing), Color.FromArgb("#4F46E5"),
        propertyChanged: (b, _, _) => ((ProgressRing)b).Invalidate());

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public Color RingColor
    {
        get => (Color)GetValue(RingColorProperty);
        set => SetValue(RingColorProperty, value);
    }

    public ProgressRing()
    {
        Drawable = new RingDrawable(this);
        Loaded += (_, _) =>
        {
            if (Application.Current is { } app)
                app.RequestedThemeChanged += OnThemeChanged;
        };
        Unloaded += (_, _) =>
        {
            if (Application.Current is { } app)
                app.RequestedThemeChanged -= OnThemeChanged;
        };
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();

    private class RingDrawable(ProgressRing ring) : IDrawable
    {
        private const float StrokeWidth = 10;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
            var radius = (size - StrokeWidth) / 2;
            if (radius <= 0)
                return;

            var cx = dirtyRect.Center.X;
            var cy = dirtyRect.Center.Y;
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

            canvas.StrokeSize = StrokeWidth;
            canvas.StrokeLineCap = LineCap.Round;

            canvas.StrokeColor = Color.FromArgb(isDark ? "#334155" : "#E2E8F0");
            canvas.DrawCircle(cx, cy, radius);

            var fraction = Math.Clamp(ring.Progress / 100.0, 0, 1);
            if (fraction <= 0)
                return;

            canvas.StrokeColor = ring.RingColor;
            if (fraction >= 1)
            {
                canvas.DrawCircle(cx, cy, radius);
                return;
            }

            // Arc starting at 12 o'clock, drawn clockwise as a polyline (1 degree steps).
            var path = new PathF();
            var degrees = (int)Math.Ceiling(fraction * 360);
            for (var i = 0; i <= degrees; i++)
            {
                var angle = Math.Min(i, fraction * 360) * Math.PI / 180;
                var x = cx + (float)(radius * Math.Sin(angle));
                var y = cy - (float)(radius * Math.Cos(angle));
                if (i == 0)
                    path.MoveTo(x, y);
                else
                    path.LineTo(x, y);
            }

            canvas.DrawPath(path);
        }
    }
}
