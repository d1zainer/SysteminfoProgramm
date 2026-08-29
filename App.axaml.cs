using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using SystemProgramm.Models;
using SystemProgramm.Settings;
using SystemProgramm.ViewModels;
using SystemProgramm.Views;

namespace SystemProgramm;

public partial class App : Application
{
    private AppSettings _settings = new();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _settings = SettingsStore.Load();

            if (_settings.Language is { Length: > 0 } language)
                Localization.Culture = new CultureInfo(language);

            RequestedThemeVariant = _settings.Theme switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };

            desktop.MainWindow = CreateMainWindow(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private MainWindow CreateMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var viewModel = new MainWindowViewModel(_settings);
        
        viewModel.LanguageChanged += (_, _) =>
        {
            var previous = desktop.MainWindow;
            var replacement = CreateMainWindow(desktop);

            desktop.MainWindow = replacement;
            replacement.Show();
            previous?.Close();
        };

        return new MainWindow { DataContext = viewModel };
    }
}