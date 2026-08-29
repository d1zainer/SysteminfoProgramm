using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;
using SystemProgramm.ViewModels.Pages;

namespace SystemProgramm.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private PageViewModel _currentPage;

    public MainWindowViewModel(AppSettings settings)
    {
        var settingsPage = new SettingsViewModel(settings);
        settingsPage.LanguageChanged += (_, e) => LanguageChanged?.Invoke(this, e);

        Pages =
        [
            new OverviewViewModel(),
            new PageViewModel(new PageInfo(Localization.PageCpu, PageKind.Cpu)),
            new PageViewModel(new PageInfo(Localization.PageGpu, PageKind.Gpu)),
            new PageViewModel(new PageInfo(Localization.PageMemory, PageKind.Memory)),
            new PageViewModel(new PageInfo(Localization.PageStorage, PageKind.Storage)),
            new PageViewModel(new PageInfo(Localization.PageNetwork, PageKind.Network)),
            settingsPage
        ];

        _currentPage = Pages[0];
    }

    public event EventHandler? LanguageChanged;

    public ObservableCollection<PageViewModel> Pages { get; }
}