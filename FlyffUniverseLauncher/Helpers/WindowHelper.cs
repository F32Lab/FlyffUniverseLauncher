using Avalonia;
using Avalonia.Controls;

namespace FlyffUniverseLauncher.Helpers;

/// <summary>
/// Small helpers shared by the windows of the launcher.
/// </summary>
public static class WindowHelper
{
    /// <summary>
    /// Places the given window at the center of the screen the launcher is currently on,
    /// like the old versions of the launcher did with every window they opened.
    /// </summary>
    /// <param name="window">The window to center. Its size has to be set before calling this.</param>
    public static void CenterOnLauncherScreen(Window window)
    {
        var screen = window.Screens.ScreenFromWindow(Program.launcher) ?? window.Screens.Primary;

        if (screen == null)
        {
            return;
        }

        // The window size is in device independent units, while the screen works in physical pixels.
        var windowWidth = (int)(window.Width * screen.Scaling);
        var windowHeight = (int)(window.Height * screen.Scaling);
        var workingArea = screen.WorkingArea;

        // A window that is bigger than the working area is pinned to the top left corner
        // of the screen instead, so its title bar always stays reachable.
        var x = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - windowWidth) / 2);
        var y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - windowHeight) / 2);

        window.Position = new PixelPoint(x, y);
    }
}
