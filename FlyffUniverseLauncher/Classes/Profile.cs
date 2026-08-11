using System.Text.Json.Serialization;

namespace FlyffUniverseLauncher.Classes;

/// <summary>
/// Represents a single profile of the profiles.json file.
/// </summary>
public class Profile
{
    /// <summary>
    /// Gets or sets the name of the profile.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last time the user pressed 'Play'.
    /// </summary>
    [JsonPropertyName("lastLogin")]
    public DateTime LastLogin { get; set; }

    /// <summary>
    /// Gets or sets the width of the window.
    /// </summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the window.
    /// </summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the profile will launch in full-screen mode.
    /// </summary>
    [JsonPropertyName("isFullScreen")]
    public bool IsFullScreen { get; set; }
}