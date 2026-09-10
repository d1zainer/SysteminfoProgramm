using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class GpuReader(HardwareMonitor monitor) : SectionReader(monitor)
{
    private const string CoreLoad = "GPU Core";

    private const string CoreTemperature = "GPU Core";

    private const string CoreClock = "GPU Core";

    private const string MemoryClock = "GPU Memory";

    private const string MemoryUsed = "GPU Memory Used";

    private const string MemoryTotal = "GPU Memory Total";

    private const string PackagePower = "GPU Package";

    private const string BoardPower = "GPU Power";

    public override SectionKind Section => SectionKind.Gpu;

    public override string Title => Localization.CardGpu;

    public override IReadOnlyList<MetricInfo> Metrics { get; } =
    [
        new MetricInfo(MetricKind.Load, 100),
        new MetricInfo(MetricKind.Temperature, 100)
    ];

    // Дискретная карта важнее встроенной, поэтому сначала ищем её.
    private IHardware? Gpu =>
        Monitor.Read(hardware => hardware.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd)
        ?? Monitor.Read(HardwareType.GpuIntel);

    public override HardwareReading Read()
    {
        if (Gpu is not { } gpu)
        {
            return new HardwareReading(Localization.ValueUnknown);
        }

        var load = gpu.Value(SensorType.Load, CoreLoad);

        var parts = new[] { Memory(gpu), Format.Celsius(gpu.Value(SensorType.Temperature, CoreTemperature)) }
            .Where(part => part is not null)
            .ToArray();

        return new HardwareReading(
            gpu.Name,
            parts.Length == 0 ? null : string.Join(", ", parts),
            load,
            load is null ? null : string.Format(Localization.LoadPercent, load));
    }

    protected override IReadOnlyList<DetailRow> DescribeRows()
    {
        if (Gpu is not { } gpu)
        {
            return [];
        }

        var vendor = gpu.HardwareType switch
        {
            HardwareType.GpuNvidia => "NVIDIA",
            HardwareType.GpuAmd => "AMD",
            HardwareType.GpuIntel => "Intel",
            _ => null
        };

        return
        [
            new(Localization.DetailModel, gpu.Name),
            new(Localization.DetailVendor, vendor)
        ];
    }

    public override SectionReading ReadSection()
    {
        if (Gpu is not { } gpu)
        {
            return Blank();
        }

        var load = gpu.Value(SensorType.Load, CoreLoad);
        var temperature = gpu.Value(SensorType.Temperature, CoreTemperature);

        // У встроенной графики нет ни своей памяти, ни датчика мощности,
        // поэтому набор строк у разных карт разный.
        var power = gpu.Value(SensorType.Power, PackagePower) ?? gpu.Value(SensorType.Power, BoardPower);

        DetailRow[] rows =
        [
            new(Localization.DetailLoad, Format.Percent(load)),
            new(Localization.DetailTemperature, Format.Celsius(temperature)),
            new(Localization.DetailClock, Format.Megahertz(gpu.Value(SensorType.Clock, CoreClock))),
            new(Localization.DetailMemoryClock, Format.Megahertz(gpu.Value(SensorType.Clock, MemoryClock))),
            new(Localization.DetailVideoMemory, Memory(gpu)),
            new(Localization.DetailPower, Format.Watt(power))
        ];

        return new SectionReading([load, temperature], Rows(rows));
    }

    // Занятая и общая видеопамять нужны и карточке обзора, и таблице подробностей.
    private static string? Memory(IHardware gpu)
    {
        var used = gpu.Value(SensorType.SmallData, MemoryUsed);
        var total = gpu.Value(SensorType.SmallData, MemoryTotal);

        if (used is null || total is null)
        {
            return null;
        }

        // Сенсоры памяти у видеокарты в мегабайтах.
        return string.Format(
            Localization.StorageDetail,
            Format.Gigabytes(used.Value.MegabytesToGigabytes()),
            Format.Gigabytes(total.Value.MegabytesToGigabytes()));
    }
}
