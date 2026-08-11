using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using FlyffUniverseLauncher.Classes;
using FlyffUniverseLauncher.Helpers;
using TsadriuUtilitiesOld;

namespace FlyffUniverseLauncher
{
    public sealed partial class FlyffUniverseWindow : Window
    {
        public static FlyffUniverseWiki? currentWikiWidow;

        // Keeps track of the game windows that are currently open, one per profile.
        private static readonly Dictionary<string, FlyffUniverseWindow> _openGameWindows = new(StringComparer.OrdinalIgnoreCase);

        private Profile _currentProfile;
        private bool _isFullScreen;
        private string _networkDataDirectory = string.Empty;

        public FlyffUniverseWindow(Profile profile)
        {
            _currentProfile = profile;
            InitializeComponent();
            flyffMenuStrip.IsVisible = false;
            SetWindowProperties();
            UpdateAllLabelsLanguage();
            _openGameWindows[profile.Name] = this;
        }

        /// <summary>
        /// Gets the game window that is currently running the given profile, or <c>null</c> if there is none.
        /// </summary>
        /// <param name="profileName">The name of the profile to look for.</param>
        public static FlyffUniverseWindow? GetOpenWindow(string profileName)
        {
            return _openGameWindows.GetValueOrDefault(profileName);
        }

        /// <summary>
        /// Called when the window is closed. Removes the profile from the list of running game windows.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _openGameWindows.Remove(_currentProfile.Name);
            base.OnClosed(e);
        }

        /// <summary>
        /// Called when the window receives input.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Current event.</param>
        private void webView_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Home:
                    ToggleToolbar();
                    break;
                case Key.F11:
                    ToggleFullScreen();
                    break;
            }
        }

        /// <summary>
        /// Shows/hides the utility toolbar.
        /// </summary>
        private void ToggleToolbar()
        {
            flyffMenuStrip.IsVisible = !flyffMenuStrip.IsVisible;
        }

        /// <summary>
        /// Toggles the window between full screen and its normal state.
        /// </summary>
        /// <remarks>The full screen state already removes the window borders by itself.</remarks>
        private void ToggleFullScreen()
        {
            _isFullScreen = !_isFullScreen;
            WindowState = _isFullScreen ? WindowState.FullScreen : WindowState.Normal;
        }

        /// <summary>
        /// Sets the window's properties, such as the window name, size and location.
        /// </summary>
        private void SetWindowProperties()
        {
            Width = _currentProfile.Width;
            Height = _currentProfile.Height;
            Title += $@"{Program.CurrentVersion} - {_currentProfile.Name.LetterUpperCase(0)}";
            Position = Program.launcher.Position;

            if (_currentProfile.IsFullScreen)
            {
                ToggleFullScreen();
            }
        }

        /// <summary>
        /// Launches the game by navigating the webview to the Flyff Universe play page.
        /// </summary>
        /// <remarks>
        /// Each profile keeps its own network data folder (cookies, cache, local storage),
        /// which is what allows playing with multiple accounts at the same time.
        /// The folder is handed over to the webview in <see cref="webView_EnvironmentRequested"/>.
        /// </remarks>
        public void LaunchGame()
        {
            _networkDataDirectory = Path.Combine(FlyffUniverseConstants.Directory.ProgramNetworkStorage, Regex.Replace(_currentProfile.Name, @"[^\w\d]", string.Empty));
            webView.Source = new Uri(FlyffUniverseConstants.Url.Play);
            Show();
        }

        /// <summary>
        /// Called before the underlying browser is created. Every platform has its own way of
        /// pointing the browser to a custom data folder, so each one is handled separately here:
        /// WebView2 on Windows, WebKitGTK/WPE on Linux and WKWebView on macOS.
        /// </summary>
        private void webView_EnvironmentRequested(object? sender, WebViewEnvironmentRequestedEventArgs e)
        {
            switch (e)
            {
                case WindowsWebView2EnvironmentRequestedEventArgs webView2:
                    webView2.UserDataFolder = _networkDataDirectory;
                    break;
                case GtkWebViewEnvironmentRequestedEventArgs webKitGtk:
                    webKitGtk.BaseDataDirectory = _networkDataDirectory;
                    webKitGtk.BaseCacheDirectory = Path.Combine(_networkDataDirectory, "Cache");
                    break;
                case LinuxWpeWebViewEnvironmentRequestedEventArgs webKitWpe:
                    webKitWpe.DataDirectory = _networkDataDirectory;
                    webKitWpe.CacheDirectory = Path.Combine(_networkDataDirectory, "Cache");
                    break;
                case AppleWKWebViewEnvironmentRequestedEventArgs wkWebView:
                    // WKWebView does not expose a folder directly, but a stable identifier
                    // derived from the profile folder gives every profile its own data store.
                    wkWebView.DataStoreIdentifier = CreateStableGuid(_networkDataDirectory);
                    break;
            }
        }

        /// <summary>
        /// Called when the game page has finished loading. The game canvas swallows the keyboard,
        /// so a small script forwards the two launcher hotkeys (HOME and F11) from the web page
        /// back to this window through <see cref="webView_WebMessageReceived"/>.
        /// </summary>
        private async void webView_NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
        {
            await webView.InvokeScript(
                """
                window.addEventListener('keydown', function (event) {
                    if (event.key === 'F11' || event.key === 'Home') {
                        event.preventDefault();
                        invokeCSharpAction(event.key);
                    }
                });
                """);
        }

        /// <summary>
        /// Receives the hotkeys forwarded by the script injected in <see cref="webView_NavigationCompleted"/>.
        /// </summary>
        private void webView_WebMessageReceived(object? sender, WebMessageReceivedEventArgs e)
        {
            switch (e.Body)
            {
                case "Home":
                    ToggleToolbar();
                    break;
                case "F11":
                    ToggleFullScreen();
                    break;
            }
        }

        /// <summary>
        /// Creates a stable <see cref="Guid"/> out of the given text,
        /// so the same profile always maps to the same browser data store.
        /// </summary>
        private static Guid CreateStableGuid(string text)
        {
            return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(text.ToLowerInvariant())));
        }

        private void flyffipediaMenuitem_Click(object? sender, RoutedEventArgs e)
        {
            OpenHelperWindow(FlyffUniverseConstants.Url.Flyffipedia);
        }

        private void madrigalinsideMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            OpenHelperWindow(FlyffUniverseConstants.Url.Madrigalinside);
        }

        private void flyffModelViewerMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            OpenHelperWindow(FlyffUniverseConstants.Url.Flyffmodelviewer);
        }

        private void skillulatorMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            OpenHelperWindow(FlyffUniverseConstants.Url.Skillulator);
        }

        private void hideToolbarMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            ToggleToolbar();
        }

        private void frozenGameClickHereToolStripMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            webView.Refresh();
        }

        private void flyffMeMadrigalMap_Click(object? sender, RoutedEventArgs e)
        {
            OpenHelperWindow(FlyffUniverseConstants.Url.FlyffMap);
        }

        private void flyffMeTrainer_Click(object? sender, RoutedEventArgs e)
        {
            OpenHelperWindow(FlyffUniverseConstants.Url.FlyffTrainer);
        }

        private void OpenHelperWindow(string link = "")
        {
            if (currentWikiWidow == null)
            {
                currentWikiWidow = new FlyffUniverseWiki(link);
                currentWikiWidow.Show();
            }
            else
            {
                if (currentWikiWidow.GetCurrentPage() != link)
                {
                    currentWikiWidow.SetPage(link);
                }
                currentWikiWidow.Activate();
                currentWikiWidow.Focus();
            }

            hideToolbarMenuItem_Click(this, new RoutedEventArgs());
        }

        private void UpdateAllLabelsLanguage()
        {
            flyffModelViewerMenuItem.Header = Properties.Resources.FULW_flyffModelViewerMenuItem;
            frozenGameClickHereToolStripMenuItem.Header = Properties.Resources.FULW_frozenGameMenuItem;
            hideToolbarMenuItem.Header = Properties.Resources.FULW_HideMenuMenuItem;
        }
    }
}
