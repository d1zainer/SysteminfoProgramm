using LibreHardwareMonitor.Hardware;

namespace SystemProgramm.Services;

public sealed class HardwareMonitor : IDisposable
{
    private readonly Computer _computer = new()
    {
        IsCpuEnabled = true,
        IsMemoryEnabled = true,
        IsGpuEnabled = true,
        IsNetworkEnabled = true
    };

    private bool _opened;

    public SMBios? Smbios => _opened ? _computer.SMBios : null;

    public void Open()
    {
        if (_opened)
            return;

        try
        {
            _computer.Open();
            _opened = true;
        }
        catch (Exception)
        {
            _opened = false;
        }
    }

    public IHardware? Read(HardwareType type) => Read(hardware => hardware.HardwareType == type);

    public IHardware? Read(Func<IHardware, bool> match)
    {
        var hardware = _opened ? _computer.Hardware.FirstOrDefault(match) : null;
        hardware?.Update();

        return hardware;
    }

    public void Dispose()
    {
        if (!_opened)
            return;

        _computer.Close();
        _opened = false;
    }
}