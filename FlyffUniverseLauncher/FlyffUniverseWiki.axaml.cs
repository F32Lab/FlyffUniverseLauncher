using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using FlyffUniverseLauncher.Helpers;

namespace FlyffUniverseLauncher
{
    public sealed partial class FlyffUniverseWiki : Window
    {

        public static string currentPage = string.Empty;
        private string networkDataDirectory = string.Empty;

        public FlyffUniverseWiki(string link)
        {
            currentPage = link;
            InitializeComponent();
            SetWindowProperties();
            SetUpUri();
        }

        /// <summary>
        /// Sets the window's properties, such as the window name, size and location.
        /// </summary>
        private void SetWindowProperties()
        {
            // The helper window opens at 1280x720, or smaller when the screen cannot fit it.
            var screen = Screens.ScreenFromWindow(Program.launcher);
            var screenWidth = screen?.Bounds.Width ?? 1280;
            var screenHeight = screen?.Bounds.Height ?? 720;
            Width = Math.Min(1280, screenWidth);
            Height = Math.Min(720, screenHeight);
            Title += $"{Program.CurrentVersion} - Helper";
            Position = Program.launcher.Position;
        }

        /// <summary>
        /// Navigates the webview to the requested page. All the helper pages share
        /// the same network data folder, exactly like the previous versions of the launcher.
        /// </summary>
        private void SetUpUri()
        {
            var name = "FlyffWiki".ToLower();
            networkDataDirectory = Path.Combine(FlyffUniverseConstants.Directory.ProgramNetworkStorage, name);
            webView2.Source = new Uri(currentPage);
        }

        /// <summary>
        /// Called before the underlying browser is created, to point it to the helper's own data folder.
        /// The platform handling mirrors the one of <see cref="FlyffUniverseWindow"/>.
        /// </summary>
        private void webView2_EnvironmentRequested(object? sender, WebViewEnvironmentRequestedEventArgs e)
        {
            switch (e)
            {
                case WindowsWebView2EnvironmentRequestedEventArgs webView2Environment:
                    webView2Environment.UserDataFolder = networkDataDirectory;
                    break;
                case GtkWebViewEnvironmentRequestedEventArgs webKitGtk:
                    webKitGtk.BaseDataDirectory = networkDataDirectory;
                    webKitGtk.BaseCacheDirectory = Path.Combine(networkDataDirectory, "Cache");
                    break;
                case LinuxWpeWebViewEnvironmentRequestedEventArgs webKitWpe:
                    webKitWpe.DataDirectory = networkDataDirectory;
                    webKitWpe.CacheDirectory = Path.Combine(networkDataDirectory, "Cache");
                    break;
            }
        }

        /// <summary>
        /// Called when a helper page has finished loading. The webview swallows the keyboard,
        /// so a small script forwards the ESCAPE key from the web page back to this window.
        /// </summary>
        private async void webView2_NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
        {
            await webView2.InvokeScript(
                """
                window.addEventListener('keydown', function (event) {
                    if (event.key === 'Escape') {
                        invokeCSharpAction(event.key);
                    }
                });
                """);
        }

        /// <summary>
        /// Receives the key forwarded by the script injected in <see cref="webView2_NavigationCompleted"/>.
        /// </summary>
        private void webView2_WebMessageReceived(object? sender, WebMessageReceivedEventArgs e)
        {
            if (e.Body == "Escape")
            {
                Close();
            }
        }

        private void FlyffUniverseWiki_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        /// <summary>
        /// Called when the window is closed. Clears the reference the game window keeps of this window,
        /// like the old Dispose() of the WinForms version did.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            FlyffUniverseWindow.currentWikiWidow = null;
            base.OnClosed(e);
        }

        public void SetPage(string link)
        {
            currentPage = link;
            webView2.Source = new Uri(link);
        }

        public string GetCurrentPage()
        {
            return webView2.Source?.AbsoluteUri ?? currentPage;
        }
    }
}
