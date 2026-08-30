using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;

namespace SystemProgramm.ViewModels.Charts;

public sealed partial class MetricViewModel : ObservableObject
{
    private readonly Queue<double> _points = new();

    private readonly double? _fixedMaximum;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasData))]
    private IReadOnlyList<double> _values = [];

    [ObservableProperty]
    private string _current = Localization.ValueUnknown;

    [ObservableProperty]
    private double _maximum;

    public MetricViewModel(MetricInfo info, int capacity = 60)
    {
        Kind = info.Kind;
        Title = info.Title;
        Capacity = capacity;

        _fixedMaximum = info.Maximum;
        _maximum = info.Maximum ?? 1;

        Unit = info.Kind switch
        {
            MetricKind.Temperature => Localization.UnitCelsius,
            _ => Localization.UnitPercent
        };

        // Тик сэмплера - секунда, поэтому окно графика это ёмкость в секундах.
        Window = string.Format(Localization.ChartWindow, TimeSpan.FromSeconds(capacity).TotalMinutes);
    }

    public MetricKind Kind { get; }

    public bool HasData => Values.Count > 0;

    public string Title { get; }

    public int Capacity { get; }

    public double Minimum => 0;

    public string Unit { get; }

    public string Window { get; }

    public void Push(double? value)
    {
        if (value is null)
            return;

        _points.Enqueue(value.Value);

        while (_points.Count > Capacity)
            _points.Dequeue();

        Values = _points.ToArray();
        Current = Format(value.Value);
        Maximum = _fixedMaximum ?? Scale();
    }

    // Своя шкала нужна там, где потолка не существует: скорость сети, например.
    private double Scale()
    {
        var peak = _points.Max();
        var step = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(peak, 1))));

        return Math.Max(Math.Ceiling(peak * 1.2 / step) * step, 1);
    }

    // Число без пояснения не читается: в шапке карточки это единственная подпись графика.
    private string Format(double value) => $"{Title} {Number(value)}";

    private string Number(double value) => Kind switch
    {
        MetricKind.Temperature => string.Format(Localization.TemperatureCelsius, value),
        _ => string.Format(Localization.PercentValue, value)
    };
}