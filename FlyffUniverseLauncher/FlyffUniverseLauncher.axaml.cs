using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FlyffUniverseLauncher.Helpers;
using TsadriuUtilities;
using TsadriuUtilities.Csv;
using TsadriuUtilities.Enums.StringHelper;
using TsadriuUtilitiesOld;
using FlyffUniverseLauncher.Classes;
using FlyffUniverseLauncher.Classes.Json;
using FlyffUniverseLauncher.Properties;

namespace FlyffUniverseLauncher
{
    public sealed partial class FlyffUniverseLauncher : Window
    {
        private static ICsvTable _profilesTable = new CsvTable();
        private const string ProfileColumn = "Profile";
        private const string LastLoginColumn = "Last Login";
        private const string PreferredWidthColumn = "Preferred Width";
        private const string PreferredHeightColumn = "Preferred Height";
        private const string IsFullScreenColumn = "Is Full Screen";

        private static Profile _selectedProfile = null!;
        private static List<Profile> _profiles = [];

        public FlyffUniverseLauncher()
        {
            InitializeComponent();
            PickRandomImage();
            AssignUsersToComboBox();
            Title += Program.CurrentVersion;
            ManageProfileHelpers.Setup(_profilesTable);
            LoadLauncherProperties();

            // The editable ComboBox has no TextChanged event, so the Text property is observed instead.
            selectUserInput.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ComboBox.TextProperty)
                {
                    selectUserInput_TextChanged();
                }
            };

            // The migration popup can only be shown once the window is open.
            Opened += ShowMigrationInfo;
        }

        /// <summary>
        /// Informs the user that the launcher files of an older version were moved
        /// to the new data folder, and where both locations are.
        /// </summary>
        private async void ShowMigrationInfo(object? sender, EventArgs e)
        {
            if (StorageMigration.MigratedFolders.Count == 0)
            {
                return;
            }

            var message = Properties.Resources.FUL_migration_message
                .Replace("$OLD$", string.Join(Environment.NewLine, StorageMigration.MigratedFolders))
                .Replace("$NEW$", FlyffUniverseConstants.Directory.ProgramStorage);

            await MessageBox.Show(message, Properties.Resources.FUL_migration_message_caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void SaveProfile(Profile profile)
        {
            _profilesTable[ProfileColumn].AddRow(profile.Name);
            _profilesTable[LastLoginColumn].AddRow($"{DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            _profilesTable[PreferredWidthColumn].AddRow(profile.Width.ToString());
            _profilesTable[PreferredHeightColumn].AddRow(profile.Height.ToString());
            _profilesTable[IsFullScreenColumn].AddRow(profile.IsFullScreen ? "1" : "0");
            File.WriteAllLines(FlyffUniverseConstants.Directory.ProfilesFile, _profilesTable.ToList());

            _profiles.Add(profile);
        }

        public void SetCurrentProfile(Profile profile)
        {
            _selectedProfile = profile;
            selectUserInput.Text = profile.Name;
        }

        private async void playButton_Click(object? sender, RoutedEventArgs e)
        {
            string currentUser = (selectUserInput.Text ?? string.Empty).ToLower();

            if (string.IsNullOrEmpty(currentUser))
            {
                await MessageBox.Show(Properties.Resources.FUL_playButton_no_profile_selected, Properties.Resources.FUL_playButton_no_profile_selected_caption, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // The profile is resolved from the typed text itself, so pressing 'Play' right after
            // typing a valid name works even when no selection event has fired yet.
            Profile? profileToLaunch = _profiles.Find(x => x.Name.Equals(currentUser, StringComparison.CurrentCultureIgnoreCase));

            if (profileToLaunch == null)
            {
                await MessageBox.Show(Properties.Resources.FUL_playButton_selected_profile_does_not_exist, Properties.Resources.FUL_playButton_selected_profile_does_not_exist_caption,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _selectedProfile = profileToLaunch;

            // If the profile is already running, bring its window to the front instead of
            // launching it a second time (two windows cannot share the same network data).
            var openWindow = FlyffUniverseWindow.GetOpenWindow(profileToLaunch.Name);

            if (openWindow != null)
            {
                openWindow.Activate();
                return;
            }

            var flyff = new FlyffUniverseWindow(profileToLaunch);
            flyff.LaunchGame();
        }

        /// <summary>
        /// Pressing ENTER in the profile box launches the selected profile right away.
        /// </summary>
        private void selectUserInput_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                playButton_Click(sender, new RoutedEventArgs());
            }
        }

        private async void selectUserInput_SelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
        {
            // During the selection event the Text property may not be updated yet, so the selected item is preferred.
            var selectedUser = selectUserInput.SelectedItem?.ToString() ?? selectUserInput.Text ?? string.Empty;

            // The selection can be cleared by the launcher itself (for example after deleting a profile),
            // that is not a wrong input of the user so no error is shown.
            if (string.IsNullOrEmpty(selectedUser))
            {
                return;
            }

            Profile? profileToSearch = _profiles.Find(x => x.Name.Equals(selectedUser, StringComparison.CurrentCultureIgnoreCase));

            if (profileToSearch == null)
            {
                await MessageBox.Show(Properties.Resources.FUL_selectUserInput_profileDoesNotExist, Properties.Resources.FUL_selectUserInput_profileDoesNotExist_caption, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _selectedProfile = null!;
                selectUserInput.Text = string.Empty;
                return;
            }

            _selectedProfile = profileToSearch;
        }

        private async void selectUserInput_TextChanged()
        {
            if (GetProfileIndex(selectUserInput.Text ?? string.Empty) == -1)
            {
                return;
            }

            Profile? profileToSearch = _profiles.Find(x => x.Name.Equals(selectUserInput.Text, StringComparison.CurrentCultureIgnoreCase));

            if (profileToSearch == null)
            {
                await MessageBox.Show(Properties.Resources.FUL_selectUserInput_profileDoesNotExist, Properties.Resources.FUL_profileSettingsLabel, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _selectedProfile = null!;
                selectUserInput.Text = string.Empty;
                return;
            }

            _selectedProfile = profileToSearch;
        }

        private void PickRandomImage()
        {
            var listOfImages = new List<string>
            {
                "img0",
                "img1",
                "img2",
                "img3",
                "img4",
                "img5",
                "img6",
                "img7",
                "img8",
                "img9",
                "img10",
                "img11",
            };

            // The upper bound of Random.Next is exclusive, so every image can be picked.
            var random = new Random();
            var randomNumber = random.Next(0, listOfImages.Count);

            // The images are shipped as Avalonia assets, so they can be loaded the same way on every platform.
            var imageUri = new Uri($"avares://FlyffUniverseLauncher/Assets/Images/{listOfImages[randomNumber]}.jpg");
            Background = new ImageBrush(new Bitmap(AssetLoader.Open(imageUri))) { Stretch = Stretch.UniformToFill };
        }

        private async void manageProfileSaveButton_Click(object? sender, RoutedEventArgs e)
        {
            int userIndex = GetProfileIndex(manageProfileComboBox.Text ?? string.Empty);

            if (userIndex == -1)
            {
                await MessageBox.Show(Properties.Resources.FUL_selectUserInput_profileDoesNotExist, Properties.Resources.FUL_selectUserInput_profileDoesNotExist_caption, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var doesNewUsernameExist = ManageProfileHelpers.DoesProfileToOverrideExist(manageProfileNameTextBox.Text ?? string.Empty);
            var areProfileNamesEqual = ManageProfileHelpers.AreProfileNamesEqual(manageProfileComboBox.Text ?? string.Empty, manageProfileNameTextBox.Text ?? string.Empty);

            if (doesNewUsernameExist && !areProfileNamesEqual)
            {
                await MessageBox.Show(Properties.Resources.FUL_manageProfileSaveButton_profileAlreadyExists, Properties.Resources.FUL_manageProfileSaveButton_profileAlreadyExists_caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // The width and height have to be numbers, exactly like when a new profile is created.
            if (string.IsNullOrEmpty(manageProfileWidthTextBox.Text) || !manageProfileWidthTextBox.Text.All(char.IsDigit))
            {
                await MessageBox.Show(Properties.Resources.FULNP_saveButton_invalidWidth, Properties.Resources.FULNP_saveButton_invalidWidth_caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(manageProfileHeightTextBox.Text) || !manageProfileHeightTextBox.Text.All(char.IsDigit))
            {
                await MessageBox.Show(Properties.Resources.FULNP_saveButton_invalidHeight, Properties.Resources.FULNP_saveButton_invalidHeight_caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var oldProfileName = Regex.Replace(manageProfileComboBox.Text ?? string.Empty, @"[^\w\d]", string.Empty);
            var newProfileName = Regex.Replace((manageProfileNameTextBox.Text ?? string.Empty).ToLower(), @"[^\w\d]", string.Empty);

            var oldDirectory = Path.Combine(FlyffUniverseConstants.Directory.ProgramNetworkStorage, oldProfileName);
            var newDirectory = Path.Combine(FlyffUniverseConstants.Directory.ProgramNetworkStorage, newProfileName);

            if (!Directory.Exists(oldDirectory))
            {
                await MessageBox.Show(Properties.Resources.FUL_manageProfileSaveButton_oldProfile_doesNotExist, Properties.Resources.FUL_manageProfileSaveButton_oldProfile_doesNotExist_caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                Directory.CreateDirectory(newDirectory);
            }
            else
            {
                if (!doesNewUsernameExist || !areProfileNamesEqual)
                {
                    Directory.Move(oldDirectory, newDirectory);
                }
            }

            _profilesTable[ProfileColumn].RowList[userIndex] = newProfileName;
            _profilesTable[PreferredWidthColumn].RowList[userIndex] = manageProfileWidthTextBox.Text;
            _profilesTable[PreferredHeightColumn].RowList[userIndex] = manageProfileHeightTextBox.Text;
            _profilesTable[IsFullScreenColumn].RowList[userIndex] = manageProfileFullscreenCheckBox.IsChecked == true ? "1" : "0";

            File.WriteAllLines(FlyffUniverseConstants.Directory.ProfilesFile, _profilesTable.ToList());

            var newProfile = new Profile()
            {
                Name = newProfileName,
                Width = (manageProfileWidthTextBox.Text ?? string.Empty).ToInt(),
                Height = (manageProfileHeightTextBox.Text ?? string.Empty).ToInt(),
                IsFullScreen = manageProfileFullscreenCheckBox.IsChecked == true,
            };

            AssignUsersToComboBox();
            _selectedProfile = newProfile;
            selectUserInput.Text = newProfile.Name;
            ResetManageProfileFields();

            await MessageBox.Show(Properties.Resources.FUL_manageProfileSaveButton_success, Properties.Resources.FUL_manageProfileSaveButton_success_caption, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            // Go back to the launcher tab
            launcherTabControl.SelectedIndex = 0;
        }

        private void manageProfileComboBox_SelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
        {
            // During the selection event the Text property may not be updated yet, so the selected item is preferred.
            var selectedUser = manageProfileComboBox.SelectedItem?.ToString() ?? manageProfileComboBox.Text ?? string.Empty;
            int userIndex = GetProfileIndex(selectedUser);

            if (userIndex == -1)
            {
                return;
            }

            manageProfileNameTextBox.Text = _profilesTable[ProfileColumn].RowList[userIndex]?.ToLower();
            manageProfileWidthTextBox.Text = _profilesTable[PreferredWidthColumn].RowList[userIndex];
            manageProfileHeightTextBox.Text = _profilesTable[PreferredHeightColumn].RowList[userIndex];
            manageProfileFullscreenCheckBox.IsChecked = _profilesTable[IsFullScreenColumn].RowList[userIndex] == "1";
        }

        private async void manageProfileDeleteButton_Click(object? sender, RoutedEventArgs e)
        {
            string manageProfileSelectedUser = manageProfileComboBox.Text ?? string.Empty;

            if (string.IsNullOrEmpty(manageProfileSelectedUser))
            {
                return;
            }

            int userIndex = GetProfileIndex(manageProfileSelectedUser);

            if (userIndex == -1)
            {
                return;
            }

            var message = Properties.Resources.FUL_manageProfile_deleteProfileButton_confirmation.Replace("$USERNAME$", manageProfileSelectedUser);
            var caption = Properties.Resources.FUL_manageProfile_deleteProfileButton_confirmation_caption.Replace("$USERNAME$", manageProfileSelectedUser);

            DialogResult result = await MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            for (int i = 0; i < _profilesTable.ColumnList.Count; i++)
            {
                _profilesTable[i].RowList.RemoveAt(userIndex);
            }

            File.WriteAllLines(FlyffUniverseConstants.Directory.ProfilesFile, _profilesTable.ToList());

            DeleteNetworkData(manageProfileSelectedUser);
            ResetManageProfileFields();
            AssignUsersToComboBox();
        }

        private async void manageProfileDeleteAllButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_profilesTable[ProfileColumn].ContainsEmptyRows)
            {
                return;
            }

            DialogResult result = await MessageBox.Show(Properties.Resources.FUL_manageProfile_deleteAllProfilesButton_confirmation,
                Properties.Resources.FUL_manageProfile_deleteAllProfilesButton_confirmation_caption, MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                return;
            }

            // Delete all profiles from the local profile file
            for (int i = 0; i < _profilesTable.ColumnList.Count; i++)
            {
                _profilesTable[i].RowList.Clear();
            }

            // Delete all folders (if there is any)
            if (Directory.Exists(FlyffUniverseConstants.Directory.ProgramNetworkStorage))
            {
                var foldersToDelete = Directory.GetDirectories(FlyffUniverseConstants.Directory.ProgramNetworkStorage)
                    .Where(x => !x.Contains("flyffwiki") && !x.Contains("flyffnews")).ToList();

                foreach (var folder in foldersToDelete)
                {
                    Directory.Delete(folder, true);
                }
            }

            File.WriteAllLines(FlyffUniverseConstants.Directory.ProfilesFile, _profilesTable.ToList());
            ResetManageProfileFields();
            AssignUsersToComboBox();
            launcherTabControl.SelectedIndex = 0;
        }

        private void manageProfileAdaptToScreenSize_Click(object? sender, RoutedEventArgs e)
        {
            string manageProfileSelectedUser = manageProfileComboBox.Text ?? string.Empty;

            if (string.IsNullOrEmpty(manageProfileSelectedUser))
            {
                return;
            }

            // The screen can be null when the window is not visible on any screen yet.
            var screen = Screens.ScreenFromWindow(this);

            if (screen == null)
            {
                return;
            }

            manageProfileWidthTextBox.Text = screen.Bounds.Width.ToString();
            manageProfileHeightTextBox.Text = screen.Bounds.Height.ToString();
        }

        /// <summary>
        /// Resets the fields related to profile management to their default state.
        /// </summary>
        /// <remarks>
        /// This method clears the text boxes for profile name, profile width, profile height,
        /// and unchecks the fullscreen checkbox in the profile management section of the Flyff Universe Launcher.
        /// </remarks>
        private void ResetManageProfileFields()
        {
            manageProfileComboBox.Text = string.Empty;
            manageProfileNameTextBox.Text = string.Empty;
            manageProfileWidthTextBox.Text = string.Empty;
            manageProfileHeightTextBox.Text = string.Empty;
            manageProfileFullscreenCheckBox.IsChecked = false;
        }

        /// <summary>
        /// Deletes the network data associated with the specified username from the program's storage.
        /// </summary>
        /// <param name="username">The username whose network data should be deleted.</param>
        private void DeleteNetworkData(string username)
        {
            // The folder name is sanitized the same way it was when the profile was launched.
            var networkDataToDelete = Path.Combine(FlyffUniverseConstants.Directory.ProgramNetworkStorage, Regex.Replace(username, @"[^\w\d]", string.Empty));

            if (Directory.Exists(networkDataToDelete))
            {
                Directory.Delete(networkDataToDelete, true);
            }
        }

        private int GetProfileIndex(string profile)
        {
            return _profilesTable[ProfileColumn].RowList.FindIndex(x => x != null && x.Equals(profile, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Initializes and assigns user profiles to the ComboBox controls.
        /// If the old profiles file exists, it converts the data to a new format, deletes the old file, and creates a new profiles file.
        /// If no profile data is found, it creates a new profiles table with default columns.
        /// </summary>
        /// <remarks>
        /// The method ensures that the profiles directory exists and checks if there is any setup data available in the old or new profiles files.
        /// If old data is present in the old profiles file, it is converted and saved in the new profiles file format.
        /// After setting up the profiles table, the <see cref="ReloadComboBoxes"/> method is called to reload the ComboBoxes with the updated profiles.
        /// </remarks>
        private void AssignUsersToComboBox()
        {
            if (!Directory.Exists(FlyffUniverseConstants.Directory.ProfilesDirectory))
            {
                Directory.CreateDirectory(FlyffUniverseConstants.Directory.ProfilesDirectory);
            }

            _profiles.Clear();
            bool hasSetupData = false;

            // If the old profiles file exists, replace it with the new format
            if (File.Exists(FlyffUniverseConstants.Directory.OldProfilesFile))
            {
                _profilesTable = new CsvTable(File.ReadAllText(FlyffUniverseConstants.Directory.OldProfilesFile).SplitBy(SplitType.EnvironmentNewLine), ";");
                _profilesTable["Width"].Name = PreferredWidthColumn;
                _profilesTable["Height"].Name = PreferredHeightColumn;
                _profilesTable.AddColumn(IsFullScreenColumn);

                // Every existing profile gets its own row in the new column, otherwise the table would be misaligned.
                for (int i = 0; i < _profilesTable[ProfileColumn].RowCount; i++)
                {
                    _profilesTable[IsFullScreenColumn].AddRow("0");
                }
                File.Delete(FlyffUniverseConstants.Directory.OldProfilesFile);
                File.WriteAllLines(FlyffUniverseConstants.Directory.ProfilesFile, _profilesTable.ToList());
                hasSetupData = true;
            }

            if (File.Exists(FlyffUniverseConstants.Directory.ProfilesFile))
            {
                _profilesTable = new CsvTable(File.ReadAllText(FlyffUniverseConstants.Directory.ProfilesFile).SplitBy(SplitType.EnvironmentNewLine), ";");
                hasSetupData = true;
            }

            if (!hasSetupData)
            {
                _profilesTable = new CsvTable(new CsvColumn(ProfileColumn), new CsvColumn(LastLoginColumn),
                    new CsvColumn(PreferredWidthColumn), new CsvColumn(PreferredHeightColumn),
                    new CsvColumn(IsFullScreenColumn));

                return;
            }

            for (int i = 0; i < _profilesTable[ProfileColumn].RowCount; i++)
            {
                var profile = new Profile
                {
                    Name = _profilesTable[ProfileColumn].RowList[i]!,
                    LastLogin = _profilesTable[LastLoginColumn].RowList[i].ToDateTime("dd/MM/yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss"),
                    Width = _profilesTable[PreferredWidthColumn].RowList[i].ToInt(),
                    Height = _profilesTable[PreferredHeightColumn].RowList[i].ToInt(),
                    IsFullScreen = _profilesTable[IsFullScreenColumn].RowList[i] == "1",
                };

                _profiles.Add(profile);
            }

            ReloadComboBoxes();
        }

        /// <summary>
        /// Clears both <b><see cref="selectUserInput"/></b> and <b><see cref="manageProfileComboBox"/></b> and reloads the profiles into them.
        /// </summary>
        public void ReloadComboBoxes()
        {
            var profileNames = new List<string>();

            foreach (string? profile in _profilesTable[ProfileColumn].RowList.Where(profile => !string.IsNullOrEmpty(profile)))
            {
                if (profile == null)
                {
                    continue;
                }

                profileNames.Add(profile);
            }

            selectUserInput.ItemsSource = profileNames;
            manageProfileComboBox.ItemsSource = profileNames;
        }

        /// <summary>
        /// Handles the click event of the createNewProfileButton.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void createNewProfileButton_Click(object? sender, RoutedEventArgs e)
        {
            _ = new FlyffUniverseNewProfile();
        }

        /// <summary>
        /// Opens the folder where the launcher saves all of its files in the file explorer
        /// of the operating system, so the user can inspect (or delete) the data themselves.
        /// </summary>
        private async void openDataFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(FlyffUniverseConstants.Directory.ProgramStorage);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = FlyffUniverseConstants.Directory.ProgramStorage,
                    UseShellExecute = true,
                });
            }
            catch (Exception)
            {
                // If no file explorer could be opened, at least show the user where the folder is.
                await MessageBox.Show(Properties.Resources.FUL_dataFolderButton_tooltip.Replace("$PATH$", FlyffUniverseConstants.Directory.ProgramStorage),
                    Properties.Resources.FUL_dataFolderButton, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ful__Language_ComboBox_SelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectedLanguage = (ful_language_comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var selectedCulture = selectedLanguage switch
            {
                "English" => FlyffUniverseConstants.Language.English,
                "Italiano" => FlyffUniverseConstants.Language.Italian,
                "Deutsch" => FlyffUniverseConstants.Language.German,
                _ => FlyffUniverseConstants.Language.English,
            };

            Properties.Resources.Culture = CultureInfo.GetCultureInfoByIetfLanguageTag(selectedCulture);
            UpdateAllLabelsLanguage();
            UpdateLauncherLanguageProperties(selectedCulture);
        }

        private void UpdateAllLabelsLanguage()
        {
            // Profile Settings tab
            profileSettingsTab.Header = Properties.Resources.FUL_profileSettingsLabel;
            manageProfilesTab.Header = Properties.Resources.FUL_manageProfilesLabel;
            selectUserLabel.Text = Properties.Resources.FUL_selectUserLabel;
            selectUserInput.PlaceholderText = Properties.Resources.FUL_selectUserInput_watermark;
            playButton.Content = Properties.Resources.FUL_playButton;
            createNewProfileButton.Content = Properties.Resources.FUL_createNewProfileButton;
            ful_language_label.Text = Properties.Resources.FUL_language_label;
            ful_credit_label.Text = Properties.Resources.FUL_credit_label;
            openDataFolderButton.Content = Properties.Resources.FUL_dataFolderButton;
            ToolTip.SetTip(openDataFolderButton, Properties.Resources.FUL_dataFolderButton_tooltip.Replace("$PATH$", FlyffUniverseConstants.Directory.ProgramStorage));

            // Manage Profiles tab
            selectProfileToModifyLabel.Text = Properties.Resources.FUL_manageProfiles_selectProfileToModifyLabel;
            manageProfileComboBox.PlaceholderText = Properties.Resources.FUL_selectUserInput_watermark;
            manageProfiles_profileNameLabel.Text = Properties.Resources.FUL_manageProfiles_profileNameLabel;
            manageProfiles_preferredWidthLabel.Text = Properties.Resources.FUL_manageProfiles_preferredWidthLabel;
            manageProfiles_preferredHeightLabel.Text = Properties.Resources.FUL_manageProfiles_preferredHeightLabel;
            manageProfileFullscreenCheckBox.Content = Properties.Resources.FUL_manageProfiles_fullscreenLabel;
            manageProfileAdaptToScreenSize.Content = Properties.Resources.FUL_manageProfiles_adaptToScreenSizeButton;
            manageProfileSaveButton.Content = Properties.Resources.FUL_manageProfiles_saveChangesButton;
            manageProfileDeleteButton.Content = Properties.Resources.FUL_manageProfiles_deleteProfileButton;
            manageProfileDeleteAllButton.Content = Properties.Resources.FUL_manageProfiles_deleteAllProfilesButton;
        }

        private void LoadLauncherProperties()
        {
            if (!Directory.Exists(FlyffUniverseConstants.Directory.ProgramStorage))
            {
                Directory.CreateDirectory(FlyffUniverseConstants.Directory.ProgramStorage);
            }

            LauncherPropertiesJson launcherProperties = new LauncherPropertiesJson();

            if (File.Exists(FlyffUniverseConstants.Directory.LauncherFile))
            {
                var fileContent = File.ReadAllText(FlyffUniverseConstants.Directory.LauncherFile);
                try
                {
                    launcherProperties = JsonSerializer.Deserialize<LauncherPropertiesJson>(fileContent)!;
                }
                catch (Exception exception)
                {
                    // The log folder has to exist before the log file can be written into it.
                    Directory.CreateDirectory(FlyffUniverseConstants.Directory.LogStorage);
                    string fileName = $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_launcher_error.log";
                    string description = Properties.Resources.FUL_launcherPropertiesJson_error.Replace("$LOCATION$",
                        Path.Combine(FlyffUniverseConstants.Directory.LogStorage, fileName));
                    _ = MessageBox.Show(description, Properties.Resources.FUL_launcherPropertiesJson_error_caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    string errorMessage = "Could not deserialize the JSON launcher properties! File content: " + fileContent + " - Exception: " + exception;
                    File.WriteAllText(Path.Combine(FlyffUniverseConstants.Directory.LogStorage, fileName), errorMessage);
                }
            }

            ful_language_comboBox.SelectedIndex = launcherProperties.Language switch
            {
                FlyffUniverseConstants.Language.English => 0,
                FlyffUniverseConstants.Language.Italian => 1,
                FlyffUniverseConstants.Language.German => 2,
                _ => 0,
            };

            UpdateLauncherLanguageProperties(launcherProperties.Language);
            UpdateAllLabelsLanguage();
        }

        /// <summary>
        /// Updates the launcher language properties by saving the specified culture code
        /// into the launcher configuration file.
        /// </summary>
        /// <param name="selectedCultureCode">The culture code representing the chosen language (e.g., "en-us").</param>
        /// <remarks>
        /// This method reads the current launcher configuration file, updates the language property
        /// with the provided culture code, and writes the changes back to the file. If the configuration
        /// file does not exist, it creates a new configuration file in the designated program storage directory.
        /// </remarks>
        private void UpdateLauncherLanguageProperties(string selectedCultureCode)
        {
            if (!Directory.Exists(FlyffUniverseConstants.Directory.ProgramStorage))
            {
                Directory.CreateDirectory(FlyffUniverseConstants.Directory.ProgramStorage);
            }

            LauncherPropertiesJson launcherProperties = new LauncherPropertiesJson();

            if (File.Exists(FlyffUniverseConstants.Directory.LauncherFile))
            {
                var fileContent = File.ReadAllText(FlyffUniverseConstants.Directory.LauncherFile);
                try
                {
                    launcherProperties = JsonSerializer.Deserialize<LauncherPropertiesJson>(fileContent)!;
                }
                catch (Exception exception)
                {
                    // The log folder has to exist before the log file can be written into it.
                    Directory.CreateDirectory(FlyffUniverseConstants.Directory.LogStorage);
                    string fileName = $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_update_launcher_error.log";
                    string description = Properties.Resources.FUL_launcherPropertiesJson_error.Replace("$LOCATION$",
                        Path.Combine(FlyffUniverseConstants.Directory.LogStorage, fileName));
                    _ = MessageBox.Show(description, Properties.Resources.FUL_launcherPropertiesJson_error_caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    string errorMessage = "Could not deserialize the JSON launcher properties during the update! File content: " + fileContent + " - Exception: " +
                                          exception;
                    File.WriteAllText(Path.Combine(FlyffUniverseConstants.Directory.LogStorage, fileName), errorMessage);
                }
            }

            if (launcherProperties.Language.Equals(selectedCultureCode, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            launcherProperties.Language = selectedCultureCode;
            File.WriteAllText(FlyffUniverseConstants.Directory.LauncherFile, JsonSerializer.Serialize(launcherProperties));
        }
    }
}
