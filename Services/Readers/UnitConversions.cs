namespace SystemProgramm.Services.Readers;

internal static class UnitConversions
{
    private const double Kilo = 1024;

    public static double BytesToGigabytes(this double bytes) => bytes / Kilo / Kilo / Kilo;

    public static double MegabytesToGigabytes(this double megabytes) => megabytes / Kilo;

    public static double BytesToBits(this double bytes) => bytes * 8;
}