using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;

namespace SystemProgramm.ViewModels.Charts;

public sealed partial class MetricViewModel : ObservableObject
{
    private readonly Queue<double> _points = new();

    [ObservableProperty]
    private IReadOnlyList<double> _values = [];

    [ObservableProperty]
    private string _current = Localization.ValueUnknown;

    public MetricViewModel(MetricKind kind, string title, int capacity = 120)
    {
        Kind = kind;
        Title = title;
        Capacity = capacity;

        Maximum = 100;
    }

    public MetricKind Kind { get; }

    public bool IsTemperature => Kind == MetricKind.Temperature;

    public string Title { get; }

    public int Capacity { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    public void Push(double? value)
    {
        if (value is null)
            return;

        _points.Enqueue(value.Value);

        while (_points.Count > Capacity)
            _points.Dequeue();

        Values = _points.ToArray();
        Current = Format(value.Value);
    }

    private string Format(double value) => Kind switch
    {
        MetricKind.Temperature => string.Format(Localization.TemperatureCelsius, value),
        _ => string.Format(Localization.PercentValue, value)
    };
}