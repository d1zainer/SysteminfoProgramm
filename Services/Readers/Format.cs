namespace SystemProgramm.Services.Readers;

// Единицы измерения в одном месте: ими пользуются все ридеры.
internal static class Format
{
    public static string Kilobytes(double value) =>
        value >= 1024
            ? $"{value / 1024:0.#} {Localization.UnitMegabyte}"
            : $"{value:0} {Localization.UnitKilobyte}";

    public static string Gigabytes(double value) =>
        value >= 1024
            ? $"{value / 1024:0.##} {Localization.UnitTerabyte}"
            : $"{value:0.#} {Localization.UnitGigabyte}";

    public static string BitsPerSecond(double value)
    {
        var megabits = value / 1_000_000;

        return megabits switch
        {
            >= 1000 => $"{megabits / 1000:0.#} {Localization.UnitGigabitPerSecond}",
            >= 1 => $"{megabits:0.#} {Localization.UnitMegabitPerSecond}",
            _ => $"{value / 1000:0} {Localization.UnitKilobitPerSecond}"
        };
    }

    public static string? Percent(double? value) =>
        value is null ? null : string.Format(Localization.PercentValue, value);

    public static string? Celsius(double? value) =>
        value is null ? null : string.Format(Localization.TemperatureCelsius, value);

    public static string? Megahertz(double? value) =>
        value is null ? null : string.Format(Localization.UnitMegahertzValue, value);

    public static string? Watt(double? value) =>
        value is null ? null : string.Format(Localization.UnitWattValue, value);
}
