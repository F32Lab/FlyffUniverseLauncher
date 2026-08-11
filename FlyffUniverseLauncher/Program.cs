using Avalonia;

namespace FlyffUniverseLauncher;

internal static class Program
{
    public const string CurrentVersion = "Version 3.0";
    public static FlyffUniverseLauncher launcher = null!;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    /// <remarks>
    /// The launcher window itself is created inside <see cref="App.OnFrameworkInitializationCompleted"/>,
    /// because Avalonia requires the framework to be fully initialized before any window can be created.
    /// </remarks>
    [STAThread]
    static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Configures the Avalonia application.
    /// </summary>
    /// <remarks>Avalonia configuration, don't remove; also used by the visual designer.</remarks>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
