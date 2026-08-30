using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;
using SystemProgramm.Services;

namespace SystemProgramm.ViewModels.Pages;

public sealed partial class SettingsViewModel : PageViewModel
{
    private readonly AppStore _store;

    [ObservableProperty]
    private ThemeOption _selectedTheme;

    [ObservableProperty]
    private LanguageOption _selectedLanguage;

    [ObservableProperty]
    private bool _runElevated;

    public SettingsViewModel(AppStore store)
        : base(new PageInfo(Localization.PageSettings, SectionKind.Settings))
    {
        _store = store;

        Themes =
        [
            new ThemeOption("System", Localization.ThemeSystem),
            new ThemeOption("Light", Localization.ThemeLight),
            new ThemeOption("Dark", Localization.ThemeDark)
        ];

        Languages =
        [
            new LanguageOption("ru", "Русский"),
            new LanguageOption("en", "English")
        ];

        _selectedTheme = Themes.FirstOrDefault(t => t.Id == store.Theme) ?? Themes[0];

        _selectedLanguage = Languages.FirstOrDefault(l => l.Culture == store.Language) ?? Languages[0];

        _runElevated = store.RunElevated;
    }

    public IReadOnlyList<ThemeOption> Themes { get; }

    public IReadOnlyList<LanguageOption> Languages { get; }

    partial void OnSelectedThemeChanged(ThemeOption value) => _store.SetTheme(value.Id);

    partial void OnSelectedLanguageChanged(LanguageOption value) => _store.SetLanguage(value.Culture);
    
    partial void OnRunElevatedChanged(bool value) => _store.SetRunElevated(value);
}
