using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class StorageReader(HardwareMonitor monitor) : IHardwareReader
{
    private const double Gigabyte = 1024d * 1024 * 1024;

    public SectionKind Section => SectionKind.Storage;

    public string Title => Localization.CardStorage;

    public HardwareReading Read()
    {
        var drive = SystemDrive();

        if (drive is null)
        {
            return new HardwareReading(Localization.ValueUnknown);
        }

        var total = drive.TotalSize / Gigabyte;
        var used = total - drive.AvailableFreeSpace / Gigabyte;
        var load = used / total * 100;

        // Модель диска и его температуру отдаёт только LibreHardwareMonitor,
        // и только под администратором. Без них показываем сам том.
        var disk = monitor.Read(HardwareType.Storage);
        var temperature = disk?.Sensors.FirstOrDefault(sensor => sensor.SensorType == SensorType.Temperature)?.Value;

        var space = string.Format(Localization.StorageDetail, Format.Gigabytes(used), Format.Gigabytes(total));
        var heat = Format.Celsius(temperature);

        return new HardwareReading(
            disk?.Name ?? drive.Name,
            heat is null ? space : $"{space}, {heat}",
            load,
            string.Format(Localization.UsedPercent, load));
    }

    private static DriveInfo? SystemDrive()
    {
        var root = Path.GetPathRoot(Environment.SystemDirectory);

        if (root is null)
        {
            return null;
        }

        try
        {
            var drive = new DriveInfo(root);

            return drive.IsReady ? drive : null;
        }
        catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}