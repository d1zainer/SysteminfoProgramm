using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SystemProgramm.Views.Controls;

public sealed class MetricChart : Control
{
    public static readonly StyledProperty<IReadOnlyList<double>?> ValuesProperty =
        AvaloniaProperty.Register<MetricChart, IReadOnlyList<double>?>(nameof(Values));

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<MetricChart, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<MetricChart, double>(nameof(Maximum), 100);

    public static readonly StyledProperty<int> CapacityProperty =
        AvaloniaProperty.Register<MetricChart, int>(nameof(Capacity), 120);

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<MetricChart, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<MetricChart, IBrush?>(nameof(Fill));

    public static readonly StyledProperty<IBrush?> GridStrokeProperty =
        AvaloniaProperty.Register<MetricChart, IBrush?>(nameof(GridStroke));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<MetricChart, double>(nameof(StrokeThickness), 1.5);

    public static readonly StyledProperty<int> DivisionsProperty =
        AvaloniaProperty.Register<MetricChart, int>(nameof(Divisions), 4);

    static MetricChart() =>
        AffectsRender<MetricChart>(
            ValuesProperty, MinimumProperty, MaximumProperty, CapacityProperty,
            StrokeProperty, FillProperty, GridStrokeProperty, StrokeThicknessProperty, DivisionsProperty);

    public IReadOnlyList<double>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // Сколько точек укладывается по ширине. График растёт слева направо
    // и до заполнения окна занимает лишь часть ширины.
    public int Capacity
    {
        get => GetValue(CapacityProperty);
        set => SetValue(CapacityProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public IBrush? GridStroke
    {
        get => GetValue(GridStrokeProperty);
        set => SetValue(GridStrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public int Divisions
    {
        get => GetValue(DivisionsProperty);
        set => SetValue(DivisionsProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var size = Bounds.Size;

        if (size.Width <= 0 || size.Height <= 0)
            return;

        DrawGrid(context, size);

        var values = Values;

        if (values is not { Count: > 1 })
            return;

        var step = size.Width / Math.Max(Capacity - 1, 1);
        var points = new Point[values.Count];

        for (var i = 0; i < values.Count; i++)
            points[i] = new Point(i * step, Offset(values[i], size.Height));

        if (Fill is { } fill)
            context.DrawGeometry(fill, null, Area(points, size.Height));

        if (Stroke is { } stroke)
            context.DrawGeometry(null, new Pen(stroke, StrokeThickness), Line(points));
    }

    private double Offset(double value, double height)
    {
        var span = Maximum - Minimum;
        var share = span <= 0 ? 0 : (value - Minimum) / span;

        return height - Math.Clamp(share, 0, 1) * height;
    }

    private void DrawGrid(DrawingContext context, Size size)
    {
        if (GridStroke is not { } brush || Divisions <= 0)
            return;

        var pen = new Pen(brush);

        for (var i = 0; i <= Divisions; i++)
        {
            var y = size.Height / Divisions * i;
            context.DrawLine(pen, new Point(0, y), new Point(size.Width, y));
        }
    }

    private static StreamGeometry Line(IReadOnlyList<Point> points)
    {
        var geometry = new StreamGeometry();

        using var line = geometry.Open();
        line.BeginFigure(points[0], false);

        for (var i = 1; i < points.Count; i++)
            line.LineTo(points[i]);

        line.EndFigure(false);

        return geometry;
    }

    private static StreamGeometry Area(IReadOnlyList<Point> points, double height)
    {
        var geometry = new StreamGeometry();

        using var area = geometry.Open();
        area.BeginFigure(new Point(points[0].X, height), true);

        foreach (var point in points)
            area.LineTo(point);

        area.LineTo(new Point(points[^1].X, height));
        area.EndFigure(true);

        return geometry;
    }
}