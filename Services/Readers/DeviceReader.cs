using HidSharp;
using SystemProgramm.Models;

namespace SystemProgramm.Services.Readers;

public sealed class DeviceReader : IHardwareReader
{
    public IconKind Icon => IconKind.Devices;

    public string Title => Localization.CardDevices;

    public HardwareReading Read()
    {
        string[] names = [..Removable(), ..Hid()];

        return new HardwareReading(
            string.Format(Localization.DeviceCount, names.Length),
            names.Length == 0 ? null : string.Join(", ", names));
    }

    private static IEnumerable<string> Removable() =>
        DriveInfo.GetDrives()
            .Where(drive => drive.DriveType is DriveType.Removable && drive.IsReady)
            .Select(drive => string.IsNullOrEmpty(drive.VolumeLabel)
                ? drive.Name
                : $"{drive.VolumeLabel} ({drive.Name})");
    
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