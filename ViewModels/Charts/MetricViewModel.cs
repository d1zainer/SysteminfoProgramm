using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;
using SystemProgramm.Services;

namespace SystemProgramm.ViewModels.Charts;

public sealed partial class MetricViewModel : ObservableObject
{
    private readonly Queue<double> _points = new();

    private readonly double? _fixedMaximum;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasData))]
    private IReadOnlyList<double> _values = [];

    [ObservableProperty]
    private double _maximum;

    public MetricViewModel(MetricInfo info, int capacity = Sampling.Window)
    {
        Kind = info.Kind;
        Capacity = capacity;

        _fixedMaximum = info.Maximum;
        _maximum = info.Maximum ?? 1;

        // Заголовок и единица берутся здесь, а не в ридере: они локализованные,
        // а вью-модель пересоздаётся вместе с окном при смене языка.
        Title = info.Kind switch
        {
            MetricKind.Temperature => Localization.MetricTemperature,
            _ => Localization.MetricLoad
        };

        Unit = info.Kind switch
        {
            MetricKind.Temperature => Localization.UnitCelsius,
            _ => Localization.UnitPercent
        };

        Window = string.Format(Localization.ChartWindow, capacity * Sampling.Interval.TotalMinutes);
    }

    public MetricKind Kind { get; }

    public string Title { get; }

    public string Unit { get; }

    public string Window { get; }

    public int Capacity { get; }

    public double Minimum => 0;

    public bool HasData => Values.Count > 0;

    public void Push(double? value)
    {
        if (value is null)
        {
            return;
        }

        _points.Enqueue(value.Value);

        while (_points.Count > Capacity)
        {
            _points.Dequeue();
        }

        Values = _points.ToArray();
        Maximum = _fixedMaximum ?? Scale();
    }

    // Своя шкала нужна там, где потолка не существует: скорость сети, например.
    private double Scale()
    {
        var peak = _points.Max();
        var step = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(peak, 1))));

        return Math.Max(Math.Ceiling(peak * 1.2 / step) * step, 1);
    }
}