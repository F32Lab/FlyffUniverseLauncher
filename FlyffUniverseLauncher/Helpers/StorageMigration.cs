namespace FlyffUniverseLauncher.Helpers;

/// <summary>
/// Moves the launcher data of older versions (which was stored at the root of the drive,
/// e.g. <c>C:\Flyff Universe Launcher</c>) to the standard application data folder.
/// </summary>
public static class StorageMigration
{
    /// <summary>
    /// Gets the list of old folders whose content was completely moved to the new location.
    /// The launcher uses this list to inform the user about the move on start up.
    /// </summary>
    public static List<string> MigratedFolders { get; } = [];

    /// <summary>
    /// Looks for the old launcher folder at the root of every fixed drive and moves its content
    /// to <see cref="FlyffUniverseConstants.Directory.ProgramStorage"/>. The old folder is deleted afterwards.
    /// </summary>
    /// <remarks>
    /// Only older Windows versions of the launcher stored the data at the root of the drive,
    /// so there is nothing to migrate on macOS and Linux.
    /// Files can be locked while the old launcher still has the game open. Those files are simply
    /// skipped: the old folder stays where it is and the migration finishes the job the next time
    /// the launcher is opened. Only a fully migrated folder is deleted and reported to the user.
    /// </remarks>
    public static void MigrateOldData()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        foreach (var drive in DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed))
        {
            var oldDirectory = Path.Combine(drive.RootDirectory.FullName, "Flyff Universe Launcher");

            if (!Directory.Exists(oldDirectory))
            {
                continue;
            }

            // Just to be safe, never migrate the new location onto itself.
            if (string.Equals(Path.GetFullPath(oldDirectory), Path.GetFullPath(FlyffUniverseConstants.Directory.ProgramStorage), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var skippedFiles = new List<string>();

            try
            {
                MoveDirectoryContent(oldDirectory, FlyffUniverseConstants.Directory.ProgramStorage, skippedFiles);

                if (skippedFiles.Count == 0)
                {
                    Directory.Delete(oldDirectory, true);
                    MigratedFolders.Add(oldDirectory);
                }
                else
                {
                    WriteMigrationLog(oldDirectory, skippedFiles);
                }
            }
            catch (Exception exception)
            {
                WriteMigrationLog(oldDirectory, [exception.ToString()]);
            }
        }
    }

    /// <summary>
    /// Recursively moves the content of <paramref name="sourceDirectory"/> into <paramref name="targetDirectory"/>.
    /// </summary>
    /// <remarks>
    /// When a file exists on both sides (for example because a previous migration was interrupted),
    /// the more recently modified one wins. Files that cannot be moved (usually because the old
    /// launcher is still running and keeps them locked) are collected in <paramref name="skippedFiles"/>.
    /// </remarks>
    private static void MoveDirectoryContent(string sourceDirectory, string targetDirectory, List<string> skippedFiles)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var file in Directory.GetFiles(sourceDirectory))
        {
            var targetFile = Path.Combine(targetDirectory, Path.GetFileName(file));

            try
            {
                if (!File.Exists(targetFile))
                {
                    File.Move(file, targetFile);
                }
                else if (File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(targetFile))
                {
                    File.Move(file, targetFile, true);
                }
                else
                {
                    // The target already has the newer version of the file, the old copy is dropped.
                    File.Delete(file);
                }
            }
            catch (Exception)
            {
                skippedFiles.Add(file);
            }
        }

        foreach (var directory in Directory.GetDirectories(sourceDirectory))
        {
            MoveDirectoryContent(directory, Path.Combine(targetDirectory, Path.GetFileName(directory)), skippedFiles);
        }
    }

    /// <summary>
    /// Writes a small log so an interrupted migration can be diagnosed.
    /// </summary>
    private static void WriteMigrationLog(string oldDirectory, List<string> skippedFiles)
    {
        try
        {
            Directory.CreateDirectory(FlyffUniverseConstants.Directory.LogStorage);
            var fileName = $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_migration.log";
            var content = $"Could not fully migrate '{oldDirectory}', the migration will be retried on the next start up. Skipped files:"
                          + Environment.NewLine + string.Join(Environment.NewLine, skippedFiles);
            File.WriteAllText(Path.Combine(FlyffUniverseConstants.Directory.LogStorage, fileName), content);
        }
        catch (Exception)
        {
            // Logging must never take the launcher down.
        }
    }
}
