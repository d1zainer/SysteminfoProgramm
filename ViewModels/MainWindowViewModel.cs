using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;
using SystemProgramm.Services;
using SystemProgramm.ViewModels.Pages;

namespace SystemProgramm.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private PageViewModel _currentPage;

    public MainWindowViewModel(AppStore store)
    {
        Pages =
        [
            new OverviewViewModel(),
            new PageViewModel(new PageInfo(Localization.PageCpu, PageKind.Cpu)),
            new PageViewModel(new PageInfo(Localization.PageGpu, PageKind.Gpu)),
            new PageViewModel(new PageInfo(Localization.PageMemory, PageKind.Memory)),
            new PageViewModel(new PageInfo(Localization.PageStorage, PageKind.Storage)),
            new PageViewModel(new PageInfo(Localization.PageNetwork, PageKind.Network)),
            new SettingsViewModel(store)
        ];

        _currentPage = Pages[0];
    }

    public ObservableCollection<PageViewModel> Pages { get; }
}
