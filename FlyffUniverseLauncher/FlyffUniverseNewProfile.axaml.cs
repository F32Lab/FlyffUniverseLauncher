using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FlyffUniverseLauncher.Classes;

namespace FlyffUniverseLauncher;

public sealed partial class FlyffUniverseNewProfile : Window
{
    public FlyffUniverseNewProfile()
    {
        InitializeComponent();
        Title = "Flyff Universe Launcher - " + Program.CurrentVersion + " - New profile";
        Position = Program.launcher.Position;
        Program.launcher.Hide();
        UpdateAllLabelsLanguage();
        Show();
        Focus();
    }

    /// <summary>
    /// Called when the window is closed. Brings the launcher window back,
    /// like the old Dispose() of the WinForms version did.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        Program.launcher.Show();
        base.OnClosed(e);
    }

    private async void newProfileSaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(newProfileNameTextBox.Text))
        {
            await MessageBox.Show(Properties.Resources.FULNP_saveButton_invalidUsername, Properties.Resources.FULNP_saveButton_invalidUsername_caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrEmpty(newProfilePrefWidthTextBox.Text) || string.IsNullOrEmpty(newProfilePrefHeightTextBox.Text))
        {
            await MessageBox.Show(Properties.Resources.FULNP_saveButton_invalidWidthAndHeight, Properties.Resources.FULNP_saveButton_invalidWidthAndHeight_caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!newProfilePrefWidthTextBox.Text.All(char.IsDigit))
        {
            await MessageBox.Show(Properties.Resources.FULNP_saveButton_invalidWidth, Properties.Resources.FULNP_saveButton_invalidWidth_caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!newProfilePrefHeightTextBox.Text.All(char.IsDigit))
        {
            await MessageBox.Show(Properties.Resources.FULNP_saveButton_invalidHeight, Properties.Resources.FULNP_saveButton_invalidHeight_caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // The name is stored in lower case, so the profile folders stay consistent
        // also on file systems that are case sensitive (like on Linux).
        Profile profile = new Profile()
        {
            Name = Regex.Replace(newProfileNameTextBox.Text.ToLower(), @"[^\w\d]", string.Empty),
            Width = int.Parse(newProfilePrefWidthTextBox.Text),
            Height = int.Parse(newProfilePrefHeightTextBox.Text),
            IsFullScreen = newProfileFullscreenCheckBox.IsChecked == true,
        };

        FlyffUniverseLauncher.SaveProfile(profile);
        Program.launcher.SetCurrentProfile(profile);
        Program.launcher.ReloadComboBoxes();
        await MessageBox.Show(Properties.Resources.FULNP_saveButton_success, Properties.Resources.FULNP_saveButton_success_caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }

    private void newProfileAdaptScreenSizeButton_Click(object? sender, RoutedEventArgs e)
    {
        // The screen can be null when the window is not visible on any screen yet.
        var screen = Screens.ScreenFromWindow(this);

        if (screen == null)
        {
            return;
        }

        newProfilePrefWidthTextBox.Text = screen.Bounds.Width.ToString();
        newProfilePrefHeightTextBox.Text = screen.Bounds.Height.ToString();
    }

    private void UpdateAllLabelsLanguage()
    {
        newProfileNameLabel.Text = Properties.Resources.FUL_manageProfiles_profileNameLabel;
        newProfilePrefWidthLabel.Text = Properties.Resources.FUL_manageProfiles_preferredWidthLabel;
        ToolTip.SetTip(newProfilePrefWidthLabel, Properties.Resources.FULNP_preferredWidthLabel_tooltip);
        newProfilePrefHeightLabel.Text = Properties.Resources.FUL_manageProfiles_preferredHeightLabel;
        ToolTip.SetTip(newProfilePrefHeightLabel, Properties.Resources.FULNP_preferredHeightLabel_tooltip);
        newProfileFullscreenCheckBox.Content = Properties.Resources.FUL_manageProfiles_fullscreenLabel;
        ToolTip.SetTip(newProfileFullscreenCheckBox, Properties.Resources.FULNP_fullscreenLabel_tooltip);
        newProfileAdaptScreenSizeButton.Content = Properties.Resources.FUL_manageProfiles_adaptToScreenSizeButton;
        ToolTip.SetTip(newProfileAdaptScreenSizeButton, Properties.Resources.FULNP_adaptScreenSize_tooltip);
        newProfileSaveButton.Content = Properties.Resources.FULNP_saveButton;
    }
}
