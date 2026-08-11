using FlyffUniverseLauncher.Classes;

namespace FlyffUniverseLauncher;

/// <summary>
/// Provides helper methods for managing profile operations.
/// </summary>
public static class ManageProfileHelpers
{
    private static List<Profile>? _profiles;

    public static void Setup(List<Profile> profiles)
    {
        _profiles = profiles;
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
    /// Retrieves the index of the specified profile name from the profiles list.
    /// </summary>
    /// <param name="profile">The profile name to locate within the profiles list.</param>
    /// <returns>
    /// The index of the specified profile name in the profiles list if found; otherwise, <c>-1</c>.
    /// </returns>
    private static int GetProfileIndex(string profile)
    {
        ArgumentNullException.ThrowIfNull(_profiles);

        return _profiles.FindIndex(x => x.Name.Equals(profile, StringComparison.OrdinalIgnoreCase));
    }
}