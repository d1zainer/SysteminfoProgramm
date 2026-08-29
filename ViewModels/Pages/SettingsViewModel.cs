using System.Globalization;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;
using SystemProgramm.Settings;

namespace SystemProgramm.ViewModels.Pages;

public sealed partial class SettingsViewModel : PageViewModel
{
    private readonly AppSettings _settings;

    [ObservableProperty]
    private ThemeOption _selectedTheme;

    [ObservableProperty]
    private LanguageOption _selectedLanguage;

    public SettingsViewModel(AppSettings settings)
        : base(new PageInfo(Localization.PageSettings, PageKind.Settings))
    {
        _settings = settings;

        Themes =
        [
            new ThemeOption("System", Localization.ThemeSystem, ThemeVariant.Default),
            new ThemeOption("Light", Localization.ThemeLight, ThemeVariant.Light),
            new ThemeOption("Dark", Localization.ThemeDark, ThemeVariant.Dark)
        ];

        Languages =
        [
            new LanguageOption("ru", "Русский"),
            new LanguageOption("en", "English")
        ];

        _selectedTheme = Themes.FirstOrDefault(t => t.Id == settings.Theme) ?? Themes[0];

        _selectedLanguage =
            Languages.FirstOrDefault(l => l.Culture == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
            ?? Languages[0];
    }

    public event EventHandler? LanguageChanged;


    public IReadOnlyList<ThemeOption> Themes { get; }

    public IReadOnlyList<LanguageOption> Languages { get; }

    partial void OnSelectedThemeChanged(ThemeOption value)
    {
        if (Application.Current is { } app)
            app.RequestedThemeVariant = value.Variant;

        _settings.Theme = value.Id;
        SettingsStore.Save(_settings);
    }

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        _settings.Language = value.Culture;
        SettingsStore.Save(_settings);

        Localization.Culture = new CultureInfo(value.Culture);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}