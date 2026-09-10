using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;
using SystemProgramm.Services;
using SystemProgramm.Services.Readers;
using SystemProgramm.ViewModels.Charts;

namespace SystemProgramm.ViewModels.Pages;

public sealed partial class SectionViewModel : PageViewModel, IDisposable
{
    private readonly HardwareSampler _sampler;

    private readonly ISectionReader _reader;

    [ObservableProperty]
    private MetricViewModel? _selectedMetric;
    
    [ObservableProperty]
    private IReadOnlyList<DetailRowViewModel> _details = [];

    public SectionViewModel(PageInfo info, HardwareSampler sampler, ISectionReader reader) : base(info)
    {
        _sampler = sampler;
        _reader = reader;

        Metrics = [..reader.Metrics.Select(metric => new MetricViewModel(metric))];

        Charts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasSwitch));
        sampler.SectionUpdated += OnUpdated;
    }

    public IReadOnlyList<MetricViewModel> Metrics { get; }

    // В переключатель попадают только те графики, по которым данные реально пришли:
    // без доступа к MSR у процессора нет температуры, и вкладки для неё не будет.
    public ObservableCollection<MetricViewModel> Charts { get; } = [];

    // Переключатель из одной кнопки только сбивает с толку: выбирать нечего.
    public bool HasSwitch => Charts.Count > 1;

    public void Dispose()
    {
        _sampler.SectionUpdated -= OnUpdated;
    }

    private void OnUpdated(object? sender, SectionSample sample)
    {
        if (sample.Section != Info.Section)
        {
            return;
        }

        var reading = sample.Reading;

        for (var i = 0; i < Metrics.Count && i < reading.Points.Count; i++)
        {
            var metric = Metrics[i];
            var secondary = reading.Secondary is { } values && i < values.Count ? values[i] : null;
            metric.Push(reading.Points[i], secondary);

            if (metric.HasData && !Charts.Contains(metric))
            {
                Charts.Add(metric);
            }
        }

        SelectedMetric ??= Charts.FirstOrDefault();

        // Describe() сам кэширует неизменные строки - здесь их просто дописывают
        // впереди живых показаний каждый тик.
        DetailRow[] rows = [.._reader.Describe(), ..reading.Details];

        if (Details.Count != rows.Length || Details.Where((row, i) => row.Name != rows[i].Name).Any())
        {
            Details = [..rows.Select(row => new DetailRowViewModel(row.Name) { Value = row.Value })];

            return;
        }

        for (var i = 0; i < rows.Length; i++)
        {
            Details[i].Value = rows[i].Value;
        }
    }
}