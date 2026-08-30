using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class MemoryReader(HardwareMonitor monitor) : IHardwareReader
{
    private const string PhysicalMemory = "/ram";

    private const string UsedData = "Memory Used";

    private const string AvailableData = "Memory Available";

    private const string LoadSensor = "Memory";

    public SectionKind Section => SectionKind.Memory;

    public string Title => Localization.CardMemory;

    public HardwareReading Read()
    {
        // Железок типа Memory две: /ram - физическая, /vram - виртуальная с файлом подкачки.
        var memory = monitor.Read(hardware => hardware.Identifier.ToString() == PhysicalMemory);

        if (memory is null)
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
}