using System.Collections.ObjectModel;
using SystemProgramm.Models;
using SystemProgramm.Services;
using SystemProgramm.ViewModels.Cards;

namespace SystemProgramm.ViewModels.Pages;

public sealed class OverviewViewModel : PageViewModel, IDisposable
{
    private readonly HardwareSampler _sampler;

    public OverviewViewModel(HardwareSampler sampler)
        : base(new PageInfo(Localization.PageOverview, SectionKind.Overview))
    {
        _sampler = sampler;

        Cards = new ObservableCollection<OverviewCardViewModel>(
            sampler.Readers.Select(reader => new OverviewCardViewModel(reader.Title, reader.Section)));

        // Первый снимок мог прийти до того, как окно построилось.
        if (sampler.Latest is { } readings)
        {
            Apply(readings);
        }

        sampler.Updated += OnUpdated;
    }

    public ObservableCollection<OverviewCardViewModel> Cards { get; }

    public void Dispose()
    {
        _sampler.Updated -= OnUpdated;
    }

    private void OnUpdated(object? sender, IReadOnlyList<HardwareReading> readings)
    {
        Apply(readings);
    }

    // Карточки и ридеры лежат в одном порядке: коллекция строится один раз в конструкторе.
    private void Apply(IReadOnlyList<HardwareReading> readings)
    {
        for (var i = 0; i < Cards.Count && i < readings.Count; i++)
        {
            Cards[i].Apply(readings[i]);
        }
    }
}