using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class 
    GpuReader(HardwareMonitor monitor) : IHardwareReader
{
    public IconKind Icon => IconKind.Gpu;

    public string Title => Localization.CardGpu;

    public HardwareReading Read()
    {
        var gpu = monitor.Read(hardware => hardware.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd)
                  ?? monitor.Read(HardwareType.GpuIntel);

        if (gpu is null)
            return new HardwareReading(Localization.ValueUnknown);

        var load = gpu.Value(SensorType.Load, "GPU Core");

        return new HardwareReading(
            gpu.Name,
            Detail(gpu),
            load,
            load is null ? null : string.Format(Localization.LoadPercent, load));
    }

    private static string? Detail(IHardware gpu)
    {
        var used = gpu.Value(SensorType.SmallData, "GPU Memory Used");
        var total = gpu.Value(SensorType.SmallData, "GPU Memory Total");
        var temperature = gpu.Value(SensorType.Temperature, "GPU Core");

        var memory = used is null || total is null
            ? null
            : string.Format(Localization.StorageDetail,
                SizeFormat.Gigabytes(used.Value / 1024),
                SizeFormat.Gigabytes(total.Value / 1024));

        var heat = temperature is null
            ? null
            : string.Format(Localization.TemperatureCelsius, temperature);

        return string.Join(", ", new[] { memory, heat }.Where(part => part is not null));
    }
}