using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class StorageReader : IHardwareReader
{
    private const double Gigabyte = 1024d * 1024 * 1024;

    public SectionKind Section => SectionKind.Storage;

    public string Title => Localization.CardStorage;
    
    public HardwareReading Read()
    {
        var drive = SystemDrive();

        if (drive is null)
            return new HardwareReading(Localization.ValueUnknown);

        var total = drive.TotalSize / Gigabyte;
        var used = total - drive.AvailableFreeSpace / Gigabyte;
        var load = used / total * 100;

        return new HardwareReading(
            drive.Name,
            string.Format(Localization.StorageDetail, SizeFormat.Gigabytes(used), SizeFormat.Gigabytes(total)),
            load,
            string.Format(Localization.UsedPercent, load));
    }

    private static DriveInfo? SystemDrive()
    {
        var root = Path.GetPathRoot(Environment.SystemDirectory);

        try
        {
            var drive = root is null ? null : new DriveInfo(root);
            return drive is { IsReady: true } ? drive : null;
        }
        catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}