using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class CpuReader(HardwareMonitor monitor) : IHardwareReader, ISectionReader
{
    private const string TotalLoad = "CPU Total";

    private const string AverageTemperature = "Core Average";

    private const string PackageTemperature = "CPU Package";

    private const string PackagePower = "CPU Package";

    private const string CoresPower = "CPU Cores";

    public SectionKind Section => SectionKind.Cpu;

    public string Title => Localization.CardCpu;

    public IReadOnlyList<MetricInfo> Metrics { get; } =
    [
        new MetricInfo(MetricKind.Load, 100),
        new MetricInfo(MetricKind.Temperature, 100)
    ];

    // SMBIOS - слепок, снятый при старте машины, и за время работы не меняется.
    private ProcessorInformation? Processor => monitor.Smbios?.Processors?.FirstOrDefault();

    public HardwareReading Read()
    {
        var cpu = monitor.Read(HardwareType.Cpu);

        if (cpu is null)
        {
            return new HardwareReading(Localization.ValueUnknown);
        }

        var load = cpu.Value(SensorType.Load, TotalLoad);
        var processor = Processor;

        var cores = processor is { CoreCount: > 0 }
            ? string.Format(Localization.CpuCoresThreads, processor.CoreCount, processor.ThreadCount)
            : null;

        return new HardwareReading(
            cpu.Name,
            cores,
            load,
            load is null ? null : string.Format(Localization.LoadPercent, load));
    }

    public SectionReading ReadSection()
    {
        var cpu = monitor.Read(HardwareType.Cpu);

        if (cpu is null)
        {
            return new SectionReading([..Metrics.Select(_ => (double?)null)], []);
        }

        var load = cpu.Value(SensorType.Load, TotalLoad);

        var temperature = cpu.Value(SensorType.Temperature, AverageTemperature)
                          ?? cpu.Value(SensorType.Temperature, PackageTemperature);

        var clock = cpu.Sensors
            .Where(sensor => sensor.SensorType == SensorType.Clock && sensor.Value is > 0)
            .Max(sensor => (double?)sensor.Value);

        // Температуры, частоты и мощность CPU читаются через MSR. Если драйвер не поднялся,
        // мощность приходит нулём, а не null - показывать такой ноль было бы враньём.
        var msr = temperature is not null || clock is not null;

        var power = msr ? cpu.Value(SensorType.Power, PackagePower) : null;
        var coresPower = msr ? cpu.Value(SensorType.Power, CoresPower) : null;

        return new SectionReading([load, temperature], Details(cpu, load, clock, power, coresPower));
    }

    private IReadOnlyList<DetailRow> Details(
        IHardware cpu,
        double? load,
        double? clock,
        double? power,
        double? coresPower)
    {
        var processor = Processor;

        var cores = processor is { CoreCount: > 0 }
            ? $"{processor.CoreCount} / {processor.ThreadCount}"
            : null;

        var maxClock = processor is { MaxSpeed: > 0 } ? processor.MaxSpeed : (double?)null;
        var busClock = processor is { ExternalClock: > 0 } ? processor.ExternalClock : (double?)null;

        DetailRow[] rows =
        [
            new(Localization.DetailModel, cpu.Name),
            new(Localization.DetailVendor, processor?.ManufacturerName?.Trim()),
            new(Localization.DetailFamily, processor?.Family.ToString()),
            new(Localization.DetailSocket, Socket(processor)),
            new(Localization.DetailCores, cores),
            new(Localization.DetailLoad, Format.Percent(load)),
            new(Localization.DetailClock, Format.Megahertz(clock)),
            new(Localization.DetailMaxClock, Format.Megahertz(maxClock)),
            new(Localization.DetailBusClock, Format.Megahertz(busClock)),
            new(Localization.DetailPower, Format.Watt(power)),
            new(Localization.DetailPowerCores, Format.Watt(coresPower)),
            new(Localization.DetailCache, Cache())
        ];

        // Строки без значения не показываем: пустой прочерк ничего не объясняет.
        return [..rows.Where(row => row.Value is not null)];
    }

    // Перечисление сокетов у LHM неполное: для LGA1700 приходит безымянное число,
    // поэтому в таком случае показываем обозначение из SMBIOS.
    private static string? Socket(ProcessorInformation? processor)
    {
        if (processor is null)
        {
            return null;
        }

        if (Enum.IsDefined(processor.Socket))
        {
            return processor.Socket.ToString();
        }

        return processor.SocketDesignation?.Trim();
    }

    private string? Cache()
    {
        var caches = monitor.Smbios?.ProcessorCaches;

        if (caches is null || caches.Length == 0)
        {
            return null;
        }

        var levels = caches
            .GroupBy(cache => cache.Designation)
            .OrderBy(group => group.Key.ToString())
            .Select(group => $"{group.Key} {Format.Kilobytes(Size(group))}");

        return string.Join(Environment.NewLine, levels);
    }

    // У гибридных процессоров SMBIOS отдаёт по записи на тип ядра: L1 и L2 у P- и E-ядер
    // разные и складываются, а L3 общий на кристалл и в записях повторяется - его берём один раз.
    private static double Size(IGrouping<CacheDesignation, CacheInformation> caches)
    {
        if (caches.Key == CacheDesignation.L3)
        {
            return caches.Max(cache => (double)cache.Size);
        }

        return caches.Sum(cache => (double)cache.Size);
    }
}