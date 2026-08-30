using LibreHardwareMonitor.Hardware;

namespace SystemProgramm.Services;

public sealed class HardwareMonitor : IDisposable
{
    private readonly Computer _computer = new()
    {
        IsCpuEnabled = true,
        IsMemoryEnabled = true,
        IsGpuEnabled = true,
        IsNetworkEnabled = true,
        IsStorageEnabled = Elevation.IsAdministrator
    };

    private readonly Dictionary<IHardware, int> _updated = [];

    private bool _opened;

    private int _tick;

    public SMBios? Smbios => _opened ? _computer.SMBios : null;

    // За один тик одну и ту же железку просят и карточка обзора, и страница раздела.
    // Update() у видеокарты стоит 78 мс, поэтому обновляем её в тике один раз.
    public void NextTick() => _tick++;

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

        if (hardware is null)
            return null;

        if (_updated.TryGetValue(hardware, out var tick) && tick == _tick)
            return hardware;

        hardware.Update();
        _updated[hardware] = _tick;

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