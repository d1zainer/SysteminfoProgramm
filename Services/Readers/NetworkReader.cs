using System.Net.NetworkInformation;
using LibreHardwareMonitor.Hardware;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class NetworkReader(HardwareMonitor monitor) : IHardwareReader, ISectionReader
{
    private const string DownloadSpeed = "Download Speed";

    private const string UploadSpeed = "Upload Speed";

    private const string Utilization = "Network Utilization";

    private IReadOnlyList<DetailRow>? _describe;

    public SectionKind Section => SectionKind.Network;

    public string Title => Localization.CardNetwork;

    public IReadOnlyList<MetricInfo> Metrics { get; } = [new MetricInfo(MetricKind.Network, null)];

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

    // Адаптер и его максимальная скорость соединения не меняются, пока он не переключился.
    public IReadOnlyList<DetailRow> Describe()
    {
        if (_describe is not null)
        {
            return _describe;
        }

        if (Active() is not { } adapter)
        {
            return [];
        }

        DetailRow[] rows =
        [
            new(Localization.DetailAdapter, adapter.Description),
            new(Localization.DetailLinkSpeed, adapter.Speed > 0 ? Format.BitsPerSecond(adapter.Speed) : null)
        ];

        return _describe = [..rows.Where(row => row.Value is not null)];
    }

    public SectionReading ReadSection()
    {
        var adapter = Active();

        if (adapter is null)
        {
            return new SectionReading([..Metrics.Select(_ => (double?)null)], []);
        }

        var nic = Nic(adapter);

        var download = nic?.Value(SensorType.Throughput, DownloadSpeed);
        var upload = nic?.Value(SensorType.Throughput, UploadSpeed);

        DetailRow[] rows =
        [
            new(Localization.DetailDownload, download is null ? null : Format.BitsPerSecond(download.Value.BytesToBits())),
            new(Localization.DetailUpload, upload is null ? null : Format.BitsPerSecond(upload.Value.BytesToBits()))
        ];

        return new SectionReading(
            [download?.BytesToMegabitsPerSecond()],
            [..rows.Where(row => row.Value is not null)],
            [upload?.BytesToMegabitsPerSecond()]);
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