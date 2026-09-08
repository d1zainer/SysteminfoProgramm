using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class GpuReader(HardwareMonitor monitor) : IHardwareReader, ISectionReader
{
    private const string CoreLoad = "GPU Core";

    private const string CoreTemperature = "GPU Core";

    private const string CoreClock = "GPU Core";

    private const string MemoryClock = "GPU Memory";

    private const string MemoryUsed = "GPU Memory Used";

    private const string MemoryTotal = "GPU Memory Total";

    private const string PackagePower = "GPU Package";

    private const string BoardPower = "GPU Power";

    private IReadOnlyList<DetailRow>? _describe;

    public SectionKind Section => SectionKind.Gpu;

    public string Title => Localization.CardGpu;

    public IReadOnlyList<MetricInfo> Metrics { get; } =
    [
        new MetricInfo(MetricKind.Load, 100),
        new MetricInfo(MetricKind.Temperature, 100)
    ];

    // Дискретная карта важнее встроенной, поэтому сначала ищем её.
    private IHardware? Gpu =>
        monitor.Read(hardware => hardware.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd)
        ?? monitor.Read(HardwareType.GpuIntel);

    public HardwareReading Read()
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
    
    public IReadOnlyList<DetailRow> Describe()
    {
        if (_describe is not null)
        {
            return _describe;
        }

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

        DetailRow[] rows =
        [
            new(Localization.DetailModel, gpu.Name),
            new(Localization.DetailVendor, vendor)
        ];

        return _describe = [..rows.Where(row => row.Value is not null)];
    }

    public SectionReading ReadSection()
    {
        if (Gpu is not { } gpu)
        {
            return new SectionReading([..Metrics.Select(_ => (double?)null)], []);
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

        // Строки без значения не показываем: пустой прочерк ничего не объясняет.
        return new SectionReading([load, temperature], [..rows.Where(row => row.Value is not null)]);
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