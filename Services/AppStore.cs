using SystemProgramm.Models;
using SystemProgramm.Services.Readers;

namespace SystemProgramm.Services;

public sealed class AppStore : IDisposable
{
    private readonly HardwareMonitor _monitor = new();

    private AppSettings _settings = SettingsStore.Load();

    public AppStore() =>
        Overview = new HardwareSampler(
        [
            new CpuReader(_monitor),
            new GpuReader(_monitor),
            new MemoryReader(_monitor),
            new StorageReader(_monitor),
            new NetworkReader(_monitor),
            new DeviceReader()
        ]);

    public HardwareSampler Overview { get; }

    public bool IsAdministrator => Elevation.IsAdministrator;

    public string Theme => _settings.Theme;

    public string Language => CultureSetup.Resolve(_settings);

    public void Open()
    {
        _monitor.Open();
        Overview.Start(TimeSpan.FromSeconds(1));
    }

    public void Dispose()
    {
        Overview.Dispose();
        _monitor.Dispose();
    }

    // Повышение прав - это перезапуск процесса: подняли новый экземпляр, закрываем себя.
    public void Elevate()
    {
        if (Elevation.Restart())
            ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ExitRequested;

    public event EventHandler? ThemeChanged;

    public event EventHandler? LanguageChanged;

    public void SetTheme(string id)
    {
        if (_settings.Theme == id)
            return;

        _settings = _settings with { Theme = id };
        SettingsStore.Save(_settings);

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetLanguage(string culture)
    {
        if (_settings.Language == culture)
            return;

        _settings = _settings with { Language = culture };
        SettingsStore.Save(_settings);

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
