using SystemProgramm.Models;
using SystemProgramm.ViewModels.Charts;

namespace SystemProgramm.ViewModels.Pages;

public sealed class CpuViewModel : PageViewModel
{
    public CpuViewModel() : base(new PageInfo(Localization.PageCpu, SectionKind.Cpu))
    {
        Load = new MetricViewModel(MetricKind.Load, Localization.MetricLoad);
        Temperature = new MetricViewModel(MetricKind.Temperature, Localization.MetricTemperature);

        // Мок: страница пока на выдуманных данных, смотрим только вёрстку.
        Fill(Load, 20, 14, seed: 7);
        Fill(Temperature, 52, 4, seed: 11);
    }

    // Мок.
    public string Headline => "13th Gen Intel Core i5-13400F";

    // Мок.
    public string Detail => string.Format(Localization.CpuCoresThreads, 10, 16);

    public MetricViewModel Load { get; }

    public MetricViewModel Temperature { get; }

    private static void Fill(MetricViewModel metric, double start, double spread, int seed)
    {
        var random = new Random(seed);
        var value = start;

        for (var i = 0; i < metric.Capacity; i++)
        {
            value = Math.Clamp(value + (random.NextDouble() - 0.5) * spread, 2, 98);
            metric.Push(value);
        }
    }
}