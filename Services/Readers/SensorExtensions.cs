using LibreHardwareMonitor.Hardware;

namespace SystemProgramm.Services.Readers;

internal static class SensorExtensions
{
    public static double? Value(this IHardware hardware, SensorType type, string name) =>
        hardware.Sensors.FirstOrDefault(sensor => sensor.SensorType == type && sensor.Name == name)?.Value;
}