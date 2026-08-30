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
            new OverviewViewModel(store.OverviewReaders),
            new PageViewModel(new PageInfo(Localization.PageCpu, IconKind.Cpu)),
            new PageViewModel(new PageInfo(Localization.PageGpu, IconKind.Gpu)),
            new PageViewModel(new PageInfo(Localization.PageMemory, IconKind.Memory)),
            new PageViewModel(new PageInfo(Localization.PageStorage, IconKind.Storage)),
            new PageViewModel(new PageInfo(Localization.PageNetwork, IconKind.Network)),
            new SettingsViewModel(store)
        ];

        _currentPage = Pages[0];
    }

    public ObservableCollection<PageViewModel> Pages { get; }
}
