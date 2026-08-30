using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;
using SystemProgramm.Services;
using SystemProgramm.ViewModels.Pages;

namespace SystemProgramm.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    private PageViewModel _currentPage;

    public MainWindowViewModel(AppStore store)
    {
        var overview = new OverviewViewModel(store.Overview);

        Pages =
        [
            overview,
            new PageViewModel(new PageInfo(Localization.PageCpu, IconKind.Cpu)),
            new PageViewModel(new PageInfo(Localization.PageGpu, IconKind.Gpu)),
            new PageViewModel(new PageInfo(Localization.PageMemory, IconKind.Memory)),
            new PageViewModel(new PageInfo(Localization.PageStorage, IconKind.Storage)),
            new PageViewModel(new PageInfo(Localization.PageNetwork, IconKind.Network)),
            new SettingsViewModel(store)
        ];

        _currentPage = Pages[0];
        
        foreach (var card in overview.Cards)
            if (Pages.FirstOrDefault(page => page.Info.Icon == card.Icon) is { } target)
                card.Open = () => CurrentPage = target;
    }

    public ObservableCollection<PageViewModel> Pages { get; }
    
    public void Dispose()
    {
        foreach (var page in Pages.OfType<IDisposable>())
            page.Dispose();
    }
}
