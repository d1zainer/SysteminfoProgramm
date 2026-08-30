using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class MemoryReader(HardwareMonitor monitor) : IHardwareReader
{
    public SectionKind Section => SectionKind.Memory;

    public string Title => Localization.CardMemory;

    public HardwareReading Read()
    {
        // У железок типа Memory две штуки: /ram - физическая, /vram - виртуальная с файлом подкачки.
        var memory = monitor.Read(hardware => hardware.Identifier.ToString() == "/ram");

        var used = memory?.Value(SensorType.Data, "Memory Used");
        var available = memory?.Value(SensorType.Data, "Memory Available");

        if (used is null || available is null)
            return new HardwareReading(Localization.ValueUnknown);

        var load = memory?.Value(SensorType.Load, "Memory");

        return new HardwareReading(
            Format.Gigabytes(used.Value + available.Value),
            string.Format(Localization.MemoryDetail, Format.Gigabytes(used.Value), Format.Gigabytes(available.Value)),
            load,
            load is null ? null : string.Format(Localization.UsedPercent, load));
    }
}