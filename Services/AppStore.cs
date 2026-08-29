using SystemProgramm.Models;

namespace SystemProgramm.Services;

public sealed class AppStore
{
    private AppSettings _settings = SettingsStore.Load();

    public string Theme => _settings.Theme;

    public string Language => CultureSetup.Resolve(_settings);

    public event EventHandler? ThemeChanged;

    public event EventHandler? LanguageChanged;

    public void SetTheme(string id)
    {
        if (_settings.Theme == id)
            return;

        _settings = _settings with { Theme = id };
        SettingsStore.Save(_settings);

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetLanguage(string culture)
    {
        if (_settings.Language == culture)
            return;

        _settings = _settings with { Language = culture };
        SettingsStore.Save(_settings);

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
