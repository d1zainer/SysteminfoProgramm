using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SystemProgramm.Models;
using SystemProgramm.Services;
using SystemProgramm.ViewModels.Pages;

namespace SystemProgramm.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly AppStore _store;

    [ObservableProperty]
    private PageViewModel _currentPage;

    public MainWindowViewModel(AppStore store)
    {
        _store = store;

        var overview = new OverviewViewModel(store.Overview);

        Pages = Build(store, overview);

        _currentPage = overview;
        store.Activate(overview.Info.Section);

        IsAdministrator = store.IsAdministrator;
        RightsNotice = IsAdministrator ? Localization.RightsGranted : Localization.RightsMissing;
        Elevate = new RelayCommand(store.Elevate);

        Link(overview);
    }

    public ObservableCollection<PageViewModel> Pages { get; }

    public bool IsAdministrator { get; }

    public string RightsNotice { get; }

    public IRelayCommand Elevate { get; }

    public void Dispose()
    {
        foreach (var page in Pages.OfType<IDisposable>())
        {
            page.Dispose();
        }
    }

    private static ObservableCollection<PageViewModel> Build(AppStore store, OverviewViewModel overview) =>
    [
        overview,
        Page(store, Localization.PageCpu, SectionKind.Cpu),
        Page(store, Localization.PageGpu, SectionKind.Gpu),
        Page(store, Localization.PageMemory, SectionKind.Memory),
        Page(store, Localization.PageStorage, SectionKind.Storage),
        Page(store, Localization.PageNetwork, SectionKind.Network),
        new SettingsViewModel(store)
    ];

    // Раздел без своего ридера остаётся заглушкой - страница появится вместе с ридером.
    private static PageViewModel Page(AppStore store, string title, SectionKind section)
    {
        var info = new PageInfo(title, section);

        if (store.Section(section) is not { } reader)
        {
            return new PageViewModel(info);
        }

        return new SectionViewModel(info, store.Overview, reader.Metrics);
    }

    // Ссылка «Подробности» на карточке ведёт на страницу того же раздела.
    private void Link(OverviewViewModel overview)
    {
        foreach (var card in overview.Cards)
        {
            if (Pages.FirstOrDefault(page => page.Info.Section == card.Section) is not { } target)
            {
                continue;
            }

            card.Open = new RelayCommand(() => CurrentPage = target);
        }
    }

    partial void OnCurrentPageChanged(PageViewModel value)
    {
        _store.Activate(value.Info.Section);
    }
}