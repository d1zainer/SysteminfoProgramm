using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class GpuReader(HardwareMonitor monitor) : IHardwareReader
{
    private const string CoreLoad = "GPU Core";

    private const string CoreTemperature = "GPU Core";

    private const string MemoryUsed = "GPU Memory Used";

    private const string MemoryTotal = "GPU Memory Total";

    public SectionKind Section => SectionKind.Gpu;

    public string Title => Localization.CardGpu;

    public HardwareReading Read()
    {
        // Дискретная карта важнее встроенной, поэтому сначала ищем её.
        var gpu = monitor.Read(hardware => hardware.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd)
                  ?? monitor.Read(HardwareType.GpuIntel);

        if (gpu is null)
        {
            return new HardwareReading(Localization.ValueUnknown);
        }

        var load = gpu.Value(SensorType.Load, CoreLoad);

        return new HardwareReading(
            gpu.Name,
            Detail(gpu),
            load,
            load is null ? null : string.Format(Localization.LoadPercent, load));
    }

    private static string? Detail(IHardware gpu)
    {
        var used = gpu.Value(SensorType.SmallData, MemoryUsed);
        var total = gpu.Value(SensorType.SmallData, MemoryTotal);

        // Сенсоры памяти у видеокарты в мегабайтах.
        var memory = used is null || total is null
            ? null
            : string.Format(
                Localization.StorageDetail,
                Format.Gigabytes(used.Value / 1024),
                Format.Gigabytes(total.Value / 1024));

        var heat = Format.Celsius(gpu.Value(SensorType.Temperature, CoreTemperature));

        var parts = new[] { memory, heat }
            .Where(part => part is not null)
            .ToArray();

        return parts.Length == 0 ? null : string.Join(", ", parts);
    }
}