using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class CpuReader(HardwareMonitor monitor) : IHardwareReader, ISectionReader
{
    public SectionKind Section => SectionKind.Cpu;

    public string Title => Localization.CardCpu;

    public IReadOnlyList<MetricInfo> Metrics { get; } =
    [
        new MetricInfo(MetricKind.Load, Localization.MetricLoad, 100),
        new MetricInfo(MetricKind.Temperature, Localization.MetricTemperature, 100)
    ];

    public HardwareReading Read()
    {
        var cpu = monitor.Read(HardwareType.Cpu);

        if (cpu is null)
            return new HardwareReading(Localization.ValueUnknown);

        var load = cpu.Value(SensorType.Load, "CPU Total");

        return new HardwareReading(
            cpu.Name,
            Cores(),
            load,
            load is null ? null : string.Format(Localization.LoadPercent, load));
    }

    public SectionReading ReadSection()
    {
        var cpu = monitor.Read(HardwareType.Cpu);

        if (cpu is null)
            return new SectionReading([null, null], []);

        var load = cpu.Value(SensorType.Load, "CPU Total");
        var temperature = cpu.Value(SensorType.Temperature, "Core Average")
                          ?? cpu.Value(SensorType.Temperature, "CPU Package");

        var clock = cpu.Sensors
            .Where(sensor => sensor.SensorType == SensorType.Clock && sensor.Value is > 0)
            .Max(sensor => (double?)sensor.Value);

        // Температуры, частоты и мощность CPU читаются через MSR. Если драйвер не поднялся,
        // мощность приходит нулём, а не null - показывать такой ноль было бы враньём.
        var msr = temperature is not null || clock is not null;
        var processor = monitor.Smbios?.Processors?.FirstOrDefault();

        DetailRow[] details =
        [
            new(Localization.DetailModel, cpu.Name),
            new(Localization.DetailVendor, processor?.ManufacturerName?.Trim()),
            new(Localization.DetailFamily, processor?.Family.ToString()),
            new(Localization.DetailSocket, Socket(processor)),
            new(Localization.DetailCores, processor is { CoreCount: > 0 } ? $"{processor.CoreCount} / {processor.ThreadCount}" : null),
            new(Localization.DetailLoad, Format.Percent(load)),
            new(Localization.DetailClock, Format.Megahertz(clock)),
            new(Localization.DetailMaxClock, Format.Megahertz(processor is { MaxSpeed: > 0 } ? processor.MaxSpeed : null)),
            new(Localization.DetailBusClock, Format.Megahertz(processor is { ExternalClock: > 0 } ? processor.ExternalClock : null)),
            new(Localization.DetailPower, Format.Watt(msr ? cpu.Value(SensorType.Power, "CPU Package") : null)),
            new(Localization.DetailPowerCores, Format.Watt(msr ? cpu.Value(SensorType.Power, "CPU Cores") : null)),
            new(Localization.DetailCache, Cache())
        ];

        // Строки без значения не показываем: пустой прочерк ничего не объясняет.
        return new SectionReading([load, temperature], [..details.Where(row => row.Value is not null)]);
    }

    // Перечисление сокетов у LHM неполное: для LGA1700 приходит безымянное число,
    // поэтому в таком случае показываем обозначение из SMBIOS.
    private static string? Socket(ProcessorInformation? processor) =>
        processor is null ? null
        : Enum.IsDefined(processor.Socket) ? processor.Socket.ToString()
        : processor.SocketDesignation?.Trim();

    private string? Cache()
    {
        var caches = monitor.Smbios?.ProcessorCaches;

        if (caches is null || caches.Length == 0)
            return null;

        // У гибридных процессоров SMBIOS отдаёт по записи на тип ядра: L1 и L2 у P- и E-ядер
        // разные и складываются, а L3 общий на кристалл и в записях повторяется - его берём один раз.
        var levels = caches
            .GroupBy(cache => cache.Designation)
            .OrderBy(group => group.Key.ToString())
            .Select(group => $"{group.Key} {Format.Kilobytes(
                group.Key == CacheDesignation.L3
                    ? group.Max(cache => (double)cache.Size)
                    : group.Sum(cache => (double)cache.Size))}");

        return string.Join(Environment.NewLine, levels);
    }

    private string? Cores()
    {
        var processor = monitor.Smbios?.Processors?.FirstOrDefault();

        return processor is { CoreCount: > 0 }
            ? string.Format(Localization.CpuCoresThreads, processor.CoreCount, processor.ThreadCount)
            : null;
    }
}