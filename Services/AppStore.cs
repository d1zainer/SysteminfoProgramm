using SystemProgramm.Models;
using SystemProgramm.Services.Readers;

namespace SystemProgramm.Services;

public sealed class AppStore : IDisposable
{
    private readonly HardwareMonitor _monitor = new();

    private AppSettings _settings = SettingsStore.Load();

    public AppStore() =>
        Overview = new HardwareSampler(_monitor,
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

    public ISectionReader? Section(SectionKind section) =>
        Overview.Readers.OfType<ISectionReader>().FirstOrDefault(reader => reader.Section == section);

    public void Activate(SectionKind section) => Overview.Watch(Section(section));

    public string Theme => _settings.Theme;

    public string Language => CultureSetup.Resolve(_settings);

    // Открытие монитора и первый снимок стоят около полусекунды, поэтому уходят в фон:
    // окно показывается сразу и заполняется, когда придут данные.
    public void Open()
    {
        var context = SynchronizationContext.Current;

        _ = Task.Run(() =>
        {
            _monitor.Open();
            Overview.Start(TimeSpan.FromSeconds(1), context);
        });
    }

    public void Dispose()
    {
        Overview.Dispose();
        _monitor.Dispose();
    }

    public bool RunElevated => _settings.RunElevated;

    public void SetRunElevated(bool value)
    {
        if (_settings.RunElevated == value)
            return;

        _settings = _settings with { RunElevated = value };
        SettingsStore.Save(_settings);
    }
    
    
    public void Elevate()
    {
        if (!Elevation.Restart())
            return;

        SetRunElevated(true);
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public bool ElevateOnStart() => RunElevated && !Elevation.IsAdministrator && Elevation.Restart();

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
