using System.Globalization;
using SystemProgramm.Models;

namespace SystemProgramm.Services;

public static class CultureSetup
{
    public static void Apply(string language)
    {
        var culture = new CultureInfo(language);

        Localization.Culture = culture;

        // Язык интерфейса задаёт и формат чисел. Без этого на русской Windows английский
        // интерфейс показывал «12,5 GB», а строка ресурса «{0:0.#} Вт» на английской
        // системе - «45.3 Вт»: разделитель брался у системы, а не у выбранного языка.
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    // Сохранённый язык, а если его нет - язык системы.
    public static string Resolve(AppSettings settings) =>
        settings.Language is { Length: > 0 } saved
            ? saved
            : CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
}