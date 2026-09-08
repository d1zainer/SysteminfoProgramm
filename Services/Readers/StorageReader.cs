using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class StorageReader(HardwareMonitor monitor) : IHardwareReader, ISectionReader
{
    private IReadOnlyList<DetailRow>? _describe;

    public SectionKind Section => SectionKind.Storage;

    public string Title => Localization.CardStorage;

    public IReadOnlyList<MetricInfo> Metrics { get; } =
    [
        new MetricInfo(MetricKind.Load, 100),
        new MetricInfo(MetricKind.Temperature, 100)
    ];

    public HardwareReading Read()
    {
        if (SystemDrive() is not { } drive)
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

    // Модель, файловая система и объём тома не меняются, пока приложение открыто.
    public IReadOnlyList<DetailRow> Describe()
    {
        if (_describe is not null)
        {
            return _describe;
        }

        if (SystemDrive() is not { } drive)
        {
            return [];
        }

        var disk = monitor.Read(HardwareType.Storage);
        var total = ((double)drive.TotalSize).BytesToGigabytes();

        DetailRow[] rows =
        [
            new(Localization.DetailModel, disk?.Name ?? drive.Name),
            new(Localization.DetailFileSystem, drive.DriveFormat),
            new(Localization.DetailTotal, Format.Gigabytes(total))
        ];

        return _describe = [..rows.Where(row => row.Value is not null)];
    }

    public SectionReading ReadSection()
    {
        if (SystemDrive() is not { } drive)
        {
            return new SectionReading([..Metrics.Select(_ => (double?)null)], []);
        }

        var total = ((double)drive.TotalSize).BytesToGigabytes();
        var free = ((double)drive.AvailableFreeSpace).BytesToGigabytes();
        var used = total - free;
        var load = used / total * 100;

        var disk = monitor.Read(HardwareType.Storage);
        var temperature = disk?.Sensors.FirstOrDefault(sensor => sensor.SensorType == SensorType.Temperature)?.Value;

        DetailRow[] rows =
        [
            new(Localization.DetailLoad, Format.Percent(load)),
            new(Localization.DetailUsed, Format.Gigabytes(used)),
            new(Localization.DetailFree, Format.Gigabytes(free)),
            new(Localization.DetailTemperature, Format.Celsius(temperature))
        ];

        return new SectionReading([load, temperature], [..rows.Where(row => row.Value is not null)]);
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