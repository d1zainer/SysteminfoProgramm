using Avalonia;

namespace SystemProgramm;

internal static class Program
{
    // До вызова BuildAvaloniaApp нельзя трогать Avalonia и всё, что зависит
    // от SynchronizationContext: среда ещё не инициализирована.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Используется и визуальным конструктором — не удалять.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}