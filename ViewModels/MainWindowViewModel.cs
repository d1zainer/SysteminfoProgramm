using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            new CpuViewModel(),
            new PageViewModel(new PageInfo(Localization.PageGpu, SectionKind.Gpu)),
            new PageViewModel(new PageInfo(Localization.PageMemory, SectionKind.Memory)),
            new PageViewModel(new PageInfo(Localization.PageStorage, SectionKind.Storage)),
            new PageViewModel(new PageInfo(Localization.PageNetwork, SectionKind.Network)),
            new SettingsViewModel(store)
        ];

        _currentPage = Pages[0];

        IsAdministrator = store.IsAdministrator;
        RightsNotice = IsAdministrator ? Localization.RightsGranted : Localization.RightsMissing;
        Elevate = new RelayCommand(store.Elevate);
        
        foreach (var card in overview.Cards)
            if (Pages.FirstOrDefault(page => page.Info.Section == card.Section) is { } target)
                card.Open = new RelayCommand(() => CurrentPage = target);
    }

    public ObservableCollection<PageViewModel> Pages { get; }

    public bool IsAdministrator { get; }

    public string RightsNotice { get; }

    public IRelayCommand Elevate { get; }
    
    public void Dispose()
    {
        foreach (var page in Pages.OfType<IDisposable>())
            page.Dispose();
    }
}
