using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FlyffUniverseLauncher.Helpers;

namespace FlyffUniverseLauncher;

/// <summary>
/// The Avalonia application. It is responsible for creating the launcher window on startup.
/// </summary>
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Older versions of the launcher stored their data at the root of the drive.
            // If such data is found, it is moved to the application data folder before
            // the launcher window loads the profiles.
            StorageMigration.MigrateOldData();

            // Keep a global reference to the launcher window, so the other
            // windows can hide/show it and read its position on the screen.
            Program.launcher = new FlyffUniverseLauncher();
            desktop.MainWindow = Program.launcher;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
