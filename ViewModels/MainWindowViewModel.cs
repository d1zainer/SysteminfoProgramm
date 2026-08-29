using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SystemProgramm.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private PageViewModel _currentPage;

    public MainWindowViewModel()
    {
        Pages =
        [
            new OverviewViewModel(),
            new PageViewModel("Процессор"),
            new PageViewModel("Видеокарта"),
            new PageViewModel("Память"),
            new PageViewModel("Накопители"),
            new PageViewModel("Сеть"),
            new PageViewModel("Настройки")
        ];

        _currentPage = Pages[0];
    }

    public ObservableCollection<PageViewModel> Pages { get; }
}