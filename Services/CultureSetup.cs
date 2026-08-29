using System.Globalization;
using SystemProgramm.Models;

namespace SystemProgramm.Services;

public static class CultureSetup
{
    public static void Apply(string language)
    {
        var culture = new CultureInfo(language);

        Localization.Culture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    // Сохранённый язык, а если его нет - язык системы.
    public static string Resolve(AppSettings settings) =>
        settings.Language is { Length: > 0 } saved
            ? saved
            : CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
}