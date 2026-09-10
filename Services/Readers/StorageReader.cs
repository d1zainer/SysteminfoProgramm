using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class StorageReader(HardwareMonitor monitor) : SectionReader(monitor)
{
    public override SectionKind Section => SectionKind.Storage;

    public override string Title => Localization.CardStorage;

    public override IReadOnlyList<MetricInfo> Metrics { get; } =
    [
        new MetricInfo(MetricKind.Load, 100),
        new MetricInfo(MetricKind.Temperature, 100)
    ];

    public override HardwareReading Read()
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
        var disk = Monitor.Read(HardwareType.Storage);
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
    protected override IReadOnlyList<DetailRow> DescribeRows()
    {
        if (SystemDrive() is not { } drive)
        {
            return [];
        }

        var disk = Monitor.Read(HardwareType.Storage);
        var total = ((double)drive.TotalSize).BytesToGigabytes();

        return
        [
            new(Localization.DetailModel, disk?.Name ?? drive.Name),
            new(Localization.DetailFileSystem, drive.DriveFormat),
            new(Localization.DetailTotal, Format.Gigabytes(total))
        ];
    }

    public override SectionReading ReadSection()
    {
        if (SystemDrive() is not { } drive)
        {
            return Blank();
        }

        var total = ((double)drive.TotalSize).BytesToGigabytes();
        var free = ((double)drive.AvailableFreeSpace).BytesToGigabytes();
        var used = total - free;
        var load = used / total * 100;

        var disk = Monitor.Read(HardwareType.Storage);
        var temperature = disk?.Sensors.FirstOrDefault(sensor => sensor.SensorType == SensorType.Temperature)?.Value;

        DetailRow[] rows =
        [
            new(Localization.DetailLoad, Format.Percent(load)),
            new(Localization.DetailUsed, Format.Gigabytes(used)),
            new(Localization.DetailFree, Format.Gigabytes(free)),
            new(Localization.DetailTemperature, Format.Celsius(temperature))
        ];

        return new SectionReading([load, temperature], Rows(rows));
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
