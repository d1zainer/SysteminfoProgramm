using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;
using SystemProgramm.Services;
using SystemProgramm.ViewModels.Charts;

namespace SystemProgramm.ViewModels.Pages;

public sealed partial class SectionViewModel : PageViewModel, IDisposable
{
    private readonly HardwareSampler _sampler;

    [ObservableProperty]
    private MetricViewModel? _selectedMetric;

    public SectionViewModel(PageInfo info, HardwareSampler sampler, IReadOnlyList<MetricInfo> metrics) : base(info)
    {
        _sampler = sampler;

        Metrics = [..metrics.Select(metric => new MetricViewModel(metric))];

        Charts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasSwitch));
        sampler.SectionUpdated += OnUpdated;
    }

    public IReadOnlyList<MetricViewModel> Metrics { get; }

    public ObservableCollection<MetricViewModel> Charts { get; } = [];

    // Переключатель из одной кнопки только сбивает с толку: выбирать нечего.
    public bool HasSwitch => Charts.Count > 1;

    public ObservableCollection<DetailRowViewModel> Details { get; } = [];

    public void Dispose() => _sampler.SectionUpdated -= OnUpdated;

    private void OnUpdated(object? sender, SectionSample sample)
    {
        if (sample.Section != Info.Section)
            return;

        Apply(sample.Reading);
    }

    private void Apply(SectionReading reading)
    {
        for (var i = 0; i < Metrics.Count && i < reading.Points.Count; i++)
        {
            var metric = Metrics[i];
            metric.Push(reading.Points[i]);

            if (metric.HasData && !Charts.Contains(metric))
                Charts.Add(metric);
        }

        SelectedMetric ??= Charts.FirstOrDefault();

        Rows(reading.Details);
    }

    // Набор строк меняется, когда данные появляются или пропадают, поэтому при
    // несовпадении имён таблицу пересобираем, а в обычном случае правим значения на месте.
    private void Rows(IReadOnlyList<DetailRow> rows)
    {
        if (Details.Count != rows.Count || Details.Where((row, i) => row.Name != rows[i].Name).Any())
        {
            Details.Clear();

            foreach (var row in rows)
                Details.Add(new DetailRowViewModel(row.Name) { Value = row.Value });

            return;
        }

        for (var i = 0; i < rows.Count; i++)
            Details[i].Value = rows[i].Value;
    }
}