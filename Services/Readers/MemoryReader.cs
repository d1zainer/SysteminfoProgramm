using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class MemoryReader(HardwareMonitor monitor) : SectionReader(monitor)
{
    private const string PhysicalMemory = "/ram";

    private const string UsedData = "Memory Used";

    private const string AvailableData = "Memory Available";

    private const string LoadSensor = "Memory";

    public override SectionKind Section => SectionKind.Memory;

    public override string Title => Localization.CardMemory;

    public override IReadOnlyList<MetricInfo> Metrics { get; } = [new MetricInfo(MetricKind.Load, 100)];

    private IHardware? Ram => Monitor.Read(hardware => hardware.Identifier.ToString() == PhysicalMemory);

    public override HardwareReading Read()
    {
        if (Ram is not { } memory)
        {
            return new HardwareReading(Localization.ValueUnknown);
        }

        var used = memory.Value(SensorType.Data, UsedData);
        var available = memory.Value(SensorType.Data, AvailableData);

        if (used is null || available is null)
        {
            return new HardwareReading(Localization.ValueUnknown);
        }

        var load = memory.Value(SensorType.Load, LoadSensor);

        return new HardwareReading(
            Format.Gigabytes(used.Value + available.Value),
            string.Format(Localization.MemoryDetail, Format.Gigabytes(used.Value), Format.Gigabytes(available.Value)),
            load,
            load is null ? null : string.Format(Localization.UsedPercent, load));
    }

    protected override IReadOnlyList<DetailRow> DescribeRows()
    {
        var used = Ram?.Value(SensorType.Data, UsedData);
        var available = Ram?.Value(SensorType.Data, AvailableData);

        if (used is null || available is null)
        {
            return [];
        }

        // Пустые слоты SMBIOS тоже перечисляет - с нулевым размером и без типа.
        var modules = (Monitor.Smbios?.MemoryDevices ?? [])
            .Where(module => module.Size > 0)
            .ToArray();

        var speed = modules is [{ ConfiguredSpeed: > 0 } module, ..] ? (double?)module.ConfiguredSpeed : null;

        string? Modules(IReadOnlyList<MemoryDevice> devices)
        {
            if (devices.Count == 0)
            {
                return null;
            }

            return $"{devices.Count} × {Format.Gigabytes(((double)devices[0].Size).MegabytesToGigabytes())}";
        }

        return
        [
            new(Localization.DetailTotal, Format.Gigabytes(used.Value + available.Value)),
            new(Localization.DetailType, modules.Length == 0 ? null : modules[0].Type.ToString()),
            new(Localization.DetailModules, Modules(modules)),
            new(Localization.DetailClock, Format.Megahertz(speed))
        ];
    }

    public override SectionReading ReadSection()
    {
        if (Ram is not { } memory)
        {
            return Blank();
        }

        var used = memory.Value(SensorType.Data, UsedData);
        var available = memory.Value(SensorType.Data, AvailableData);
        var load = memory.Value(SensorType.Load, LoadSensor);

        DetailRow[] rows =
        [
            new(Localization.DetailLoad, Format.Percent(load)),
            new(Localization.DetailUsed, used is null ? null : Format.Gigabytes(used.Value)),
            new(Localization.DetailFree, available is null ? null : Format.Gigabytes(available.Value))
        ];

        return new SectionReading([load], Rows(rows));
    }
}
