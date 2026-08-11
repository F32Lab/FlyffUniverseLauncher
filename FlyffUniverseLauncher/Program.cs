using Avalonia;

namespace FlyffUniverseLauncher;

internal static class Program
{
    /// <summary>
    /// The version shown in the window titles, taken from the &lt;Version&gt; of the project file
    /// so it only has to be bumped in one place.
    /// </summary>
    public static readonly string CurrentVersion = "Version " + (typeof(Program).Assembly.GetName().Version?.ToString(2) ?? "3.0");

    public static FlyffUniverseLauncher launcher = null!;

    // Held for the whole lifetime of the application, see Main.
    private static Mutex? _singleInstanceMutex;

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
        // Only one launcher can run at a time, otherwise two instances would
        // write over each other's profile and settings files.
        _singleInstanceMutex = new Mutex(true, "FlyffUniverseLauncher.SingleInstance", out bool isFirstInstance);

        if (!isFirstInstance)
        {
            return;
        }

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
