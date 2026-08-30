namespace SystemProgramm.Services.Readers;

internal static class SizeFormat
{
    public static string Gigabytes(double value) =>
        value >= 1024
            ? $"{value / 1024:0.##} {Localization.UnitTerabyte}"
            : $"{value:0.#} {Localization.UnitGigabyte}";
}