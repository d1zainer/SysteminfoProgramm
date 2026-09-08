using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class StorageReader(HardwareMonitor monitor) : IHardwareReader
{
    public SectionKind Section => SectionKind.Storage;

    public string Title => Localization.CardStorage;

    public HardwareReading Read()
    {
        var root = Path.GetPathRoot(Environment.SystemDirectory);

        DriveInfo? drive = null;

        if (root is not null)
        {
            try
            {
                var candidate = new DriveInfo(root);

                if (candidate.IsReady)
                {
                    drive = candidate;
                }
            }
            catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException)
            {
            }
        }

        if (drive is null)
        {
            return new HardwareReading(Localization.ValueUnknown);
        }

        var total = ((double)drive.TotalSize).BytesToGigabytes();
        var used = total - ((double)drive.AvailableFreeSpace).BytesToGigabytes();
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
}