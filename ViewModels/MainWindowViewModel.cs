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
            new PageViewModel(Localization.PageCpu),
            new PageViewModel(Localization.PageGpu),
            new PageViewModel(Localization.PageMemory),
            new PageViewModel(Localization.PageStorage),
            new PageViewModel(Localization.PageNetwork),
            settingsPage
        ];

        _currentPage = Pages[0];
    }

    public event EventHandler? LanguageChanged;

    public ObservableCollection<PageViewModel> Pages { get; }
}