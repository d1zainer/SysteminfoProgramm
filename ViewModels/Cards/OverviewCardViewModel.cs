using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;

namespace SystemProgramm.ViewModels.Cards;

public sealed partial class OverviewCardViewModel(string title, SectionKind section) : ObservableObject
{
    // Главная строка: модель процессора, объём памяти и так далее.
    [ObservableProperty]
    private string _headline = Localization.ValueUnknown;

    // Вторичная строка: ядра/потоки, занято из общего и прочие детали.
    [ObservableProperty]
    private string? _detail;

    // Заполненность в процентах. null - полосы нет, карточке нечего показывать.
    [ObservableProperty]
    private double? _load;

    [ObservableProperty]
    private string? _loadText;

    public string Title { get; } = title;

    public SectionKind Section { get; } = section;
    
    public ICommand? Open { get; set; }

    public void Apply(HardwareReading reading)
    {
        Headline = reading.Headline;
        Detail = reading.Detail;
        Load = reading.Load;
        LoadText = reading.LoadText;
    }
}
