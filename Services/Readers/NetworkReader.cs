using System.Net.NetworkInformation;
using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class NetworkReader(HardwareMonitor monitor) : IHardwareReader
{
    public SectionKind Section => SectionKind.Network;

    public string Title => Localization.CardNetwork;

    public HardwareReading Read()
    {
        var adapter = Active();

        if (adapter is null)
            return new HardwareReading(Localization.ValueUnknown);

        var nic = Nic(adapter);

        var download = nic?.Value(SensorType.Throughput, "Download Speed");
        var upload = nic?.Value(SensorType.Throughput, "Upload Speed");

        return new HardwareReading(
            adapter.Name,
            adapter.Speed > 0 ? $"{adapter.Description}, {Speed(adapter.Speed)}" : adapter.Description,
            nic?.Value(SensorType.Load, "Network Utilization"),
            download is null || upload is null
                ? null
                : string.Format(Localization.NetworkTraffic, Speed(download.Value * 8), Speed(upload.Value * 8)));
    }

    // LibreHardwareMonitor опознаёт адаптер как /nic/%7BGUID%7D - тот же GUID,
    // что и NetworkInterface.Id, только фигурные скобки в url-кодировке.
    private IHardware? Nic(NetworkInterface adapter) =>
        monitor.Read(hardware => hardware.HardwareType == HardwareType.Network
                                 && Uri.UnescapeDataString(hardware.Identifier.ToString())
                                     .Equals($"/nic/{adapter.Id}", StringComparison.OrdinalIgnoreCase));

    private static NetworkInterface? Active() =>
        NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up
                              && adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback
                              && adapter.GetIPProperties().GatewayAddresses.Count > 0)
            .OrderByDescending(Received)
            .FirstOrDefault();

    private static long Received(NetworkInterface adapter)
    {
        try
        {
            return adapter.GetIPStatistics().BytesReceived;
        }
        catch (NetworkInformationException)
        {
            return 0;
        }
    }

    private static string Speed(double bitsPerSecond)
    {
        var megabits = bitsPerSecond / 1_000_000;

        return megabits switch
        {
            >= 1000 => $"{megabits / 1000:0.#} {Localization.UnitGigabitPerSecond}",
            >= 1 => $"{megabits:0.#} {Localization.UnitMegabitPerSecond}",
            _ => $"{bitsPerSecond / 1000:0} {Localization.UnitKilobitPerSecond}"
        };
    }
}