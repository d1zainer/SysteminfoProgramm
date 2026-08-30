using System.Collections.ObjectModel;
using SystemProgramm.Models;
using SystemProgramm.Services.Readers;
using SystemProgramm.ViewModels.Cards;

namespace SystemProgramm.ViewModels.Pages;

public sealed class OverviewViewModel : PageViewModel
{
    private readonly IReadOnlyList<IHardwareReader> _readers;

    public OverviewViewModel(IReadOnlyList<IHardwareReader> readers) : base(new PageInfo(Localization.PageOverview, IconKind.Overview))
    {
        _readers = readers;
        Cards = new ObservableCollection<OverviewCardViewModel>(
            readers.Select(reader => new OverviewCardViewModel(reader.Title, reader.Icon)));

        Refresh();
    }

    public ObservableCollection<OverviewCardViewModel> Cards { get; }
    
    private void Refresh()
    {
        for (var i = 0; i < _readers.Count; i++)
            Cards[i].Apply(_readers[i].Read());
    }
}