using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;

namespace FlyffUniverseLauncher;

/// <summary>
/// The buttons that can be displayed on a <see cref="MessageBox"/>.
/// </summary>
public enum MessageBoxButtons
{
    OK,
    YesNo,
}

/// <summary>
/// The icon displayed next to the message of a <see cref="MessageBox"/>.
/// </summary>
public enum MessageBoxIcon
{
    Information,
    Warning,
    Error,
    Question,
}

/// <summary>
/// The result returned by a <see cref="MessageBox"/> once the user closes it.
/// </summary>
public enum DialogResult
{
    OK,
    Yes,
    No,
}

/// <summary>
/// A small cross-platform replacement of the classic WinForms MessageBox,
/// built with plain Avalonia controls so it looks the same on Windows, macOS and Linux.
/// </summary>
public static class MessageBox
{
    /// <summary>
    /// Shows a modal message box on top of the currently active window.
    /// </summary>
    /// <param name="text">The message to display.</param>
    /// <param name="caption">The title of the message box window.</param>
    /// <param name="buttons">The buttons to display (<b>OK</b> or <b>Yes/No</b>).</param>
    /// <param name="icon">The icon to display next to the message.</param>
    /// <returns>A <see cref="Task"/> containing the <see cref="DialogResult"/> the user picked.</returns>
    /// <remarks>
    /// Unlike WinForms, dialogs in Avalonia are asynchronous, so callers have to <c>await</c> this method.
    /// </remarks>
    public static async Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        // The dialog result defaults to 'No' (or 'OK'), so closing the window with 'X' never confirms anything by accident.
        var dialogResult = buttons == MessageBoxButtons.OK ? DialogResult.OK : DialogResult.No;

        var dialog = new Window
        {
            Title = caption,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            MinWidth = 320,
            MaxWidth = 520,
        };

        // The icon is drawn as a simple unicode glyph, which keeps the message box free of any image assets.
        var iconGlyph = icon switch
        {
            MessageBoxIcon.Information => "ℹ️",
            MessageBoxIcon.Warning => "⚠️",
            MessageBoxIcon.Error => "❌",
            MessageBoxIcon.Question => "❓",
            _ => string.Empty,
        };

        var messageRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Avalonia.Thickness(16, 16, 16, 8),
        };
        messageRow.Children.Add(new TextBlock { Text = iconGlyph, FontSize = 24, VerticalAlignment = VerticalAlignment.Center });
        messageRow.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, MaxWidth = 420, VerticalAlignment = VerticalAlignment.Center });

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Avalonia.Thickness(16, 8, 16, 16),
        };

        if (buttons == MessageBoxButtons.OK)
        {
            var okButton = new Button { Content = "OK", MinWidth = 80, IsDefault = true };
            okButton.Click += (_, _) => { dialogResult = DialogResult.OK; dialog.Close(); };
            buttonRow.Children.Add(okButton);
        }
        else
        {
            var yesButton = new Button { Content = "Yes", MinWidth = 80, IsDefault = true };
            yesButton.Click += (_, _) => { dialogResult = DialogResult.Yes; dialog.Close(); };
            var noButton = new Button { Content = "No", MinWidth = 80, IsCancel = true };
            noButton.Click += (_, _) => { dialogResult = DialogResult.No; dialog.Close(); };
            buttonRow.Children.Add(yesButton);
            buttonRow.Children.Add(noButton);
        }

        var layout = new StackPanel();
        layout.Children.Add(messageRow);
        layout.Children.Add(buttonRow);
        dialog.Content = layout;

        // Attach the dialog to the window the user is currently interacting with.
        // When no window is available (should not happen), the dialog is shown standalone.
        var owner = GetActiveWindow();

        if (owner != null)
        {
            await dialog.ShowDialog(owner);
        }
        else
        {
            var closed = new TaskCompletionSource();
            dialog.Closed += (_, _) => closed.SetResult();
            dialog.Show();
            await closed.Task;
        }

        return dialogResult;
    }

    /// <summary>
    /// Gets the window that currently has the focus, falling back to the launcher window.
    /// </summary>
    private static Window? GetActiveWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.Windows.FirstOrDefault(window => window.IsActive && window.IsVisible)
                   ?? desktop.Windows.FirstOrDefault(window => window.IsVisible);
        }

        return null;
    }
}
