using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class CpuReader(HardwareMonitor monitor) : IHardwareReader
{
    public SectionKind Section => SectionKind.Cpu;

    public string Title => Localization.CardCpu;

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

    private string? Cores()
    {
        var processor = monitor.Smbios?.Processors?.FirstOrDefault();

        return processor is null || processor.CoreCount == 0
            ? null
            : string.Format(Localization.CpuCoresThreads, processor.CoreCount, processor.ThreadCount);
    }
}