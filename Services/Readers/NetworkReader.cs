using System.Net.NetworkInformation;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class NetworkReader : IHardwareReader
{
    public IconKind Icon => IconKind.Network;

    public string Title => Localization.CardNetwork;

    public HardwareReading Read()
    {
        var adapter = Active();

        if (adapter is null)
            return new HardwareReading(Localization.ValueUnknown);

        return new HardwareReading(
            adapter.Name,
            adapter.Speed > 0 ? $"{adapter.Description}, {Speed(adapter.Speed)}" : adapter.Description);
    }
    
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

    private static string Speed(long bitsPerSecond)
    {
        var megabits = bitsPerSecond / 1_000_000d;

        return megabits >= 1000
            ? $"{megabits / 1000:0.#} {Localization.UnitGigabitPerSecond}"
            : $"{megabits:0} {Localization.UnitMegabitPerSecond}";
    }
}