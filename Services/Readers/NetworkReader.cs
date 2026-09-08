using System.Net.NetworkInformation;
using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class NetworkReader(HardwareMonitor monitor) : IHardwareReader
{
    private const string DownloadSpeed = "Download Speed";

    private const string UploadSpeed = "Upload Speed";

    private const string Utilization = "Network Utilization";

    public SectionKind Section => SectionKind.Network;

    public string Title => Localization.CardNetwork;

    public HardwareReading Read()
    {
        var adapter = Active();

        if (adapter is null)
        {
            return new HardwareReading(Localization.ValueUnknown);
        }

        var nic = Nic(adapter);

        var download = nic?.Value(SensorType.Throughput, DownloadSpeed);
        var upload = nic?.Value(SensorType.Throughput, UploadSpeed);

        var detail = adapter.Speed > 0
            ? $"{adapter.Description}, {Format.BitsPerSecond(adapter.Speed)}"
            : adapter.Description;

        var traffic = download is null || upload is null
            ? null
            : string.Format(
                Localization.NetworkTraffic,
                Format.BitsPerSecond(download.Value.BytesToBits()),
                Format.BitsPerSecond(upload.Value.BytesToBits()));

        return new HardwareReading(adapter.Name, detail, nic?.Value(SensorType.Load, Utilization), traffic);
    }

    // LibreHardwareMonitor опознаёт адаптер как /nic/%7BGUID%7D - тот же GUID,
    // что и NetworkInterface.Id, только фигурные скобки в url-кодировке.
    private IHardware? Nic(NetworkInterface adapter)
    {
        return monitor.Read(hardware => hardware.HardwareType == HardwareType.Network
                                        && Uri.UnescapeDataString(hardware.Identifier.ToString())
                                            .Equals($"/nic/{adapter.Id}", StringComparison.OrdinalIgnoreCase));
    }

    // Активным считаем поднятый адаптер со шлюзом, а из таких - тот, через который
    // реально идёт трафик: виртуальных и туннельных в системе десятки.
    private static NetworkInterface? Active()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up
                              && adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback
                              && adapter.GetIPProperties().GatewayAddresses.Count > 0)
            .OrderByDescending(Received)
            .FirstOrDefault();
    }

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
}