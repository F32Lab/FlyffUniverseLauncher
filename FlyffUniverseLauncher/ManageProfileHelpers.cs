using FlyffUniverseLauncher.Classes.Csv;

namespace FlyffUniverseLauncher;

/// <summary>
/// Provides helper methods for managing profile operations.
/// </summary>
public static class ManageProfileHelpers
{
    private const string ProfileColumn = "Profile";
    private static CsvTable? _profilesTable;

    public static void Setup(CsvTable profilesTable)
    {
        _profilesTable = profilesTable;
    }

    /// <summary>
    /// Determines whether a profile with the specified new profile name exists in the system.
    /// </summary>
    /// <param name="newProfileName">The new profile name to check for existence.</param>
    /// <returns>
    /// <c>true</c> if a profile with the specified new profile name exists; otherwise, <c>false</c>.
    /// </returns>
    public static bool DoesProfileToOverrideExist(string newProfileName)
    {
        int newProfileUserIndex = GetProfileIndex(newProfileName);

        // If newProfileUserIndex == 1, it means that the selected new username exists
        return newProfileUserIndex != -1;
    }

    /// <summary>
    /// Compares two profile names for equality using a case-insensitive comparison.
    /// </summary>
    /// <param name="oldProfileName">The name of the existing profile to compare.</param>
    /// <param name="newProfileName">The name of the new profile to compare.</param>
    /// <returns>
    /// <c>true</c> if the profile names are equal ignoring case; otherwise, <c>false</c>.
    /// </returns>
    public static bool AreProfileNamesEqual(string oldProfileName, string newProfileName) => oldProfileName.Equals(newProfileName, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Retrieves the index of the specified profile name from the profiles table.
    /// </summary>
    /// <param name="profile">The profile name to locate within the profiles table.</param>
    /// <returns>
    /// The index of the specified profile name in the profiles table if found; otherwise, <c>-1</c>.
    /// </returns>
    private static int GetProfileIndex(string profile)
    {
        ArgumentNullException.ThrowIfNull(_profilesTable);
        
        return _profilesTable[ProfileColumn].RowList.FindIndex(x => x != null && x.Equals(profile, StringComparison.OrdinalIgnoreCase));
    }
}