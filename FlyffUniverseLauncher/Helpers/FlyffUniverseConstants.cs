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
        // On Windows the data is kept at the root of the drive (like the previous versions of the launcher did),
        // so already existing profiles are not lost. On macOS and Linux the root is not writable,
        // so the data goes to the user's local application data folder instead.
        public static readonly string CurrentDirectory = OperatingSystem.IsWindows()
            ? Path.GetPathRoot(Environment.CurrentDirectory) ?? System.IO.Directory.GetCurrentDirectory()
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string ProgramStorage = Path.Combine(CurrentDirectory, "Flyff Universe Launcher");
        public static readonly string LogStorage = Path.Combine(ProgramStorage, "Logs");
        public static readonly string ProgramNetworkStorage = Path.Combine(ProgramStorage, "Network Data");
        public static readonly string ProfilesDirectory = Path.Combine(ProgramStorage, "Profile");
        public static readonly string OldProfilesFile = Path.Combine(ProfilesDirectory, "profiles.txt");
        public static readonly string ProfilesFile = Path.Combine(ProfilesDirectory, "profiles.csv");
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