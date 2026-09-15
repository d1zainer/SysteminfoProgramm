using HidSharp;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class DeviceReader : IHardwareReader
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(5);

    private string[]? _names;

    private DateTime _taken;

    public SectionKind Section => SectionKind.Devices;

    public string Title => Localization.CardDevices;

    // Перебор HID стоит около 90 мс, а устройства меняются редко: держим прошлый список
    // и обновляем его не каждый тик.
    public HardwareReading Read()
    {
        if (_names is null || DateTime.UtcNow - _taken >= Lifetime)
        {
            _names = [..Removable(), ..Hid()];
            _taken = DateTime.UtcNow;
        }

        return new HardwareReading(
            string.Format(Localization.DeviceCount, _names.Length),
            _names.Length == 0 ? null : string.Join(", ", _names));
    }

    private static IEnumerable<string> Removable()
    {
        return DriveInfo.GetDrives()
            .Where(drive => drive.DriveType is DriveType.Removable && drive.IsReady)
            .Select(drive => string.IsNullOrEmpty(drive.VolumeLabel)
                ? drive.Name
                : $"{drive.VolumeLabel} ({drive.Name})");
    }

    // Одно физическое устройство отдаёт несколько HID-интерфейсов,
    // поэтому схлопываем по паре идентификаторов.
    private static IEnumerable<string> Hid()
    {
        try
        {
            return DeviceList.Local.GetHidDevices()
                .GroupBy(device => (device.VendorID, device.ProductID))
                .Select(group => Name(group.First()))
                .Where(name => name.Length > 0)
                .ToArray();
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static string Name(HidDevice device)
    {
        try
        {
            return device.GetFriendlyName().Trim();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}