using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SystemProgramm.Views.Controls;

public sealed class MetricChart : Control
{
    private const double LabelGap = 8;

    public static readonly StyledProperty<IReadOnlyList<double>?> ValuesProperty =
        AvaloniaProperty.Register<MetricChart, IReadOnlyList<double>?>(nameof(Values));

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<MetricChart, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<MetricChart, double>(nameof(Maximum), 100);

    public static readonly StyledProperty<int> CapacityProperty =
        AvaloniaProperty.Register<MetricChart, int>(nameof(Capacity), 120);

    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<MetricChart, string?>(nameof(Unit));

    public static readonly StyledProperty<string?> WindowLabelProperty =
        AvaloniaProperty.Register<MetricChart, string?>(nameof(WindowLabel));

    public static readonly StyledProperty<string?> NowLabelProperty =
        AvaloniaProperty.Register<MetricChart, string?>(nameof(NowLabel));

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<MetricChart, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<MetricChart, IBrush?>(nameof(Fill));

    public static readonly StyledProperty<IBrush?> GridStrokeProperty =
        AvaloniaProperty.Register<MetricChart, IBrush?>(nameof(GridStroke));

    public static readonly StyledProperty<IBrush?> LabelBrushProperty =
        AvaloniaProperty.Register<MetricChart, IBrush?>(nameof(LabelBrush));

    public static readonly StyledProperty<FontFamily?> LabelFontFamilyProperty =
        AvaloniaProperty.Register<MetricChart, FontFamily?>(nameof(LabelFontFamily));

    public static readonly StyledProperty<double> LabelFontSizeProperty =
        AvaloniaProperty.Register<MetricChart, double>(nameof(LabelFontSize), 11);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<MetricChart, double>(nameof(StrokeThickness), 1.5);

    public static readonly StyledProperty<int> DivisionsProperty =
        AvaloniaProperty.Register<MetricChart, int>(nameof(Divisions), 4);

    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<MetricChart, int>(nameof(Columns), 4);

    static MetricChart() =>
        AffectsRender<MetricChart>(
            ValuesProperty, MinimumProperty, MaximumProperty, CapacityProperty,
            UnitProperty, WindowLabelProperty, NowLabelProperty,
            StrokeProperty, FillProperty, GridStrokeProperty, LabelBrushProperty,
            LabelFontFamilyProperty, LabelFontSizeProperty, StrokeThicknessProperty, DivisionsProperty, ColumnsProperty);

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

    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string? WindowLabel
    {
        get => GetValue(WindowLabelProperty);
        set => SetValue(WindowLabelProperty, value);
    }

    public string? NowLabel
    {
        get => GetValue(NowLabelProperty);
        set => SetValue(NowLabelProperty, value);
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

    public IBrush? LabelBrush
    {
        get => GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    public FontFamily? LabelFontFamily
    {
        get => GetValue(LabelFontFamilyProperty);
        set => SetValue(LabelFontFamilyProperty, value);
    }

    public double LabelFontSize
    {
        get => GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
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

    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var labels = ScaleLabels();

        // Подписи шкалы стоят справа от области графика: справа резервируется их ширина,
        // а сверху - половина строки, иначе верхняя подпись срезается краем контрола.
        var right = labels.Count == 0 ? 0 : labels.Max(label => label.Width) + LabelGap;
        var inset = labels.Count == 0 ? 0 : labels[0].Height / 2;
        var bottom = HasTimeAxis() ? LabelFontSize + 8 : inset;

        var plot = new Rect(0, inset, Bounds.Width - right, Bounds.Height - inset - bottom);

        if (plot.Width <= 0 || plot.Height <= 0)
        {
            return;
        }

        DrawScale(context, plot, labels);
        DrawSeries(context, plot);
        DrawTimeAxis(context, plot);
    }

    private bool HasTimeAxis() => !string.IsNullOrEmpty(WindowLabel) || !string.IsNullOrEmpty(NowLabel);

    private FormattedText Text(string value) =>
        new(value, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(LabelFontFamily ?? FontFamily.Default), LabelFontSize,
            LabelBrush ?? Brushes.Gray);

    // Подписи по Y: от максимума сверху до минимума снизу, по числу делений сетки.
    private List<FormattedText> ScaleLabels()
    {
        if (LabelBrush is null || Divisions <= 0)
        {
            return [];
        }

        var span = Maximum - Minimum;
        var labels = new List<FormattedText>(Divisions + 1);

        for (var i = 0; i <= Divisions; i++)
        {
            var value = Maximum - span / Divisions * i;
            labels.Add(Text(Unit is null ? $"{value:0.#}" : $"{value:0.#} {Unit}"));
        }

        return labels;
    }

    private void DrawScale(DrawingContext context, Rect plot, List<FormattedText> labels)
    {
        var pen = GridStroke is null ? null : new Pen(GridStroke);

        for (var i = 0; i <= Divisions; i++)
        {
            var y = plot.Top + plot.Height / Divisions * i;

            if (pen is not null)
            {
                context.DrawLine(pen, new Point(plot.Left, y), new Point(plot.Right, y));
            }

            if (i < labels.Count)
            {
                context.DrawText(labels[i], new Point(plot.Right + LabelGap, y - labels[i].Height / 2));
            }
        }

        if (pen is null || Columns <= 0)
        {
            return;
        }

        // Вертикальные деления по времени: сетка замыкается и по краям области.
        for (var i = 0; i <= Columns; i++)
        {
            var x = plot.Left + plot.Width / Columns * i;
            context.DrawLine(pen, new Point(x, plot.Top), new Point(x, plot.Bottom));
        }
    }

    private void DrawSeries(DrawingContext context, Rect plot)
    {
        if (Values is not { Count: > 1 } values)
        {
            return;
        }

        // Свежая точка всегда у правого края, история уходит влево:
        // пока окно не заполнено, пустое место остаётся слева, а не справа.
        var step = plot.Width / Math.Max(Capacity - 1, 1);
        var points = new Point[values.Count];

        for (var i = 0; i < values.Count; i++)
        {
            points[i] = new Point(plot.Right - (values.Count - 1 - i) * step, Offset(values[i], plot));
        }

        if (Fill is { } fill)
        {
            context.DrawGeometry(fill, null, Area(points, plot.Bottom));
        }

        if (Stroke is { } stroke)
        {
            context.DrawGeometry(null, new Pen(stroke, StrokeThickness), Line(points));
        }
    }

    private void DrawTimeAxis(DrawingContext context, Rect plot)
    {
        if (LabelBrush is null)
        {
            return;
        }

        if (WindowLabel is { Length: > 0 } past)
        {
            context.DrawText(Text(past), new Point(plot.Left, plot.Bottom + 4));
        }

        if (NowLabel is { Length: > 0 } now)
        {
            var text = Text(now);
            context.DrawText(text, new Point(plot.Right - text.Width, plot.Bottom + 4));
        }
    }

    private double Offset(double value, Rect plot)
    {
        var span = Maximum - Minimum;
        var share = span <= 0 ? 0 : (value - Minimum) / span;

        return plot.Bottom - Math.Clamp(share, 0, 1) * plot.Height;
    }

    private static StreamGeometry Line(IReadOnlyList<Point> points)
    {
        var geometry = new StreamGeometry();

        using var line = geometry.Open();
        line.BeginFigure(points[0], false);

        for (var i = 1; i < points.Count; i++)
        {
            line.LineTo(points[i]);
        }

        line.EndFigure(false);

        return geometry;
    }

    private static StreamGeometry Area(IReadOnlyList<Point> points, double bottom)
    {
        var geometry = new StreamGeometry();

        using var area = geometry.Open();
        area.BeginFigure(new Point(points[0].X, bottom), true);

        foreach (var point in points)
        {
            area.LineTo(point);
        }

        area.LineTo(new Point(points[^1].X, bottom));
        area.EndFigure(true);

        return geometry;
    }
}