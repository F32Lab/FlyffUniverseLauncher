namespace FlyffUniverseLauncher.Helpers;

/// <summary>
/// A class containing constant values and resources for the Flyff Universe Launcher application.
/// This class provides directory paths and URLs used throughout the application.
/// </summary>
public abstract class FlyffUniverseConstants
{
    /// <summary>
    /// Represents a collection of static directory path constants used by the Flyff Universe Launcher application.
    /// This class defines key directories for application storage, network data, profiles, and configuration files.
    /// It helps manage file and directory paths required for various launcher operations.
    /// </summary>
    public abstract class Directory
    {
        // The launcher data lives in the standard application data folder of the user:
        // - Windows: C:\Users\<name>\AppData\Local\Flyff Universe Launcher
        // - macOS / Linux: ~/.local/share/Flyff Universe Launcher
        // Older versions of the launcher stored the data at the root of the drive,
        // that data is moved over automatically by StorageMigration on start up.
        public static readonly string ProgramStorage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flyff Universe Launcher");
        public static readonly string LogStorage = Path.Combine(ProgramStorage, "Logs");
        public static readonly string ProgramNetworkStorage = Path.Combine(ProgramStorage, "Network Data");
        public static readonly string ProfilesDirectory = Path.Combine(ProgramStorage, "Profile");

        // The profile files of the older versions of the launcher. When one of them is found,
        // it is converted once to the new profiles.json and then deleted.
        public static readonly string OldProfilesFile = Path.Combine(ProfilesDirectory, "profiles.txt");
        public static readonly string OldCsvProfilesFile = Path.Combine(ProfilesDirectory, "profiles.csv");
        public static readonly string ProfilesFile = Path.Combine(ProfilesDirectory, "profiles.json");
        public static readonly string LauncherFile = Path.Combine(ProgramStorage, "Launcher.json");
    }

    /// <summary>
    /// A static class containing constant URLs used in the Flyff Universe Launcher application.
    /// This class provides predefined links for resourceful tools, maps, and reference sites related to the game.
    /// </summary>
    public abstract class Url
    {
        public const string Play = "https://universe.flyff.com/play";
        public const string Flyffipedia = "https://flyffipedia.com/";
        public const string Madrigalinside = "https://madrigalinside.com/";
        public const string FlyffMap = "https://www.flyff.me/map";
        public const string FlyffTrainer = "https://www.flyff.me/trainer";
        public const string Flyffmodelviewer = "https://flyffmodelviewer.com/";
        public const string Skillulator = "https://skillulator.com/";
    }

    /// <summary>
    /// Provides language constants representing supported localization codes for the Flyff Universe Launcher application.
    /// Use these constants to set or identify the application's language preferences.
    /// </summary>
    public abstract class Language
    {
        public const string English = "en-us";
        public const string Italian = "it-ch";
        public const string German = "de-de";
    }
}