# Flyff Universe Launcher!
* Uses libraries:
  * [Avalonia UI](https://avaloniaui.net/) (with [Avalonia WebView](https://docs.avaloniaui.net/docs/app-development/embedding-web-content))

##  Features 
* Works on **Windows**, **macOS** and **Linux**!
* Ability to launch and play the game with multiple profiles!
  * Each profile can be customized.
* Pressing **F11** will enable full screen. The only way to remove the full screen is to press it again.
* Pressing **HOME** will show/hide a toolbar. Said toolbar provides shortcuts to the flyff's wiki, and other useful websites.

##  License
MIT

##  Where the launcher saves its data
The launcher stores everything (profiles, settings, browser data) in one folder, which can also be opened directly with the *Open data folder* button inside the launcher:
* **Windows**: `C:\Users\<name>\AppData\Local\Flyff Universe Launcher`
* **macOS / Linux**: `~/.local/share/Flyff Universe Launcher`

Deleting that folder removes every trace of the launcher from the computer.

##  Building from source
The launcher needs the [.NET 10 SDK](https://dotnet.microsoft.com/download). A ready-to-run build for a specific platform is created with:
```
dotnet publish FlyffUniverseLauncher/FlyffUniverseLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
(replace `win-x64` with `linux-x64`, `osx-x64` or `osx-arm64` for the other platforms)

Pushing a tag that starts with `v` (e.g. `v3.0.0`) automatically builds all four platforms with GitHub Actions and attaches the zipped builds to a GitHub release.

## Release 3.0

### New Features
* The launcher is now **multiplatform**! It runs on Windows, macOS and Linux.
  * The user interface was moved from Windows Forms to [Avalonia UI](https://avaloniaui.net/).
  * The game is displayed through the webview engine of each platform (WebView2 on Windows, WKWebView on macOS, WebKitGTK on Linux), so the launcher stays small.
  * Every profile still keeps its own separate network data (cookies, cache, login), also on macOS and Linux.
* Updated project `.NET` version from `.NET 8.0` to `.NET 10.0`.
* The launcher data now lives in the standard application data folder:
  * **Windows**: `C:\Users\<name>\AppData\Local\Flyff Universe Launcher`
  * **macOS / Linux**: `~/.local/share/Flyff Universe Launcher`
  * Data of older versions (stored at the root of the drive, e.g. `C:\Flyff Universe Launcher`) is moved over automatically on the first start, the old folder is deleted and a pop-up shows both locations.
  * A new *Open data folder* button in the launcher opens that folder directly, so it's always clear where the data is (deleting that folder removes every trace of the launcher).
* The profiles are now stored in a readable `profiles.json` file (like the launcher settings).
  * An existing `profiles.csv` (or the ancient `profiles.txt`) is converted automatically on start up and then deleted.
* Removed the `TsadriuUtilities` dependency, the launcher now reads and writes the profiles file on its own.

### Quality of Life
* Pressing **ENTER** in the profile box launches the selected profile right away, and pressing **ENTER** in the *New profile* window saves the profile.
* If a profile is already running, pressing *Play* again brings its window to the front instead of launching it twice.
* A profile whose game window is still open can no longer be deleted (its data is in use by the game), the launcher explains why instead.
* Only one launcher can run at a time, so two instances can no longer write over each other's files.
* The *Last login* of a profile is now updated every time it is launched.
* The width and height fields are now validated in the *Manage Profiles* tab too, and the input boxes show hints about what is expected.
* The *Yes*/*No* buttons of the dialog windows are now localized as well.
* The launcher always uses the light theme, so it stays readable when the operating system is set to dark mode.

### Bugfixes
* The last background image of the launcher can now actually appear (it was never picked before).
* Creating a profile with a name that already exists now shows an error instead of creating a duplicate.
* Creating a profile whose name only consists of special characters (e.g. `!!!`) no longer creates a profile with an empty name.
* `Delete all profiles` now updates the profile file even when no network data folders exist.
* Deleting a profile whose name contains special characters now also deletes its network data folder.
* Pressing *Play* right after typing a profile name could launch the previously selected profile instead of the typed one.
* Converting profiles of very old versions (`profiles.txt`) no longer misaligns the profile data when there is more than one profile.
* Error logs are now written correctly even when the log folder does not exist yet.

## Release 2.0
### New Features
* Implemented localization in the launcher for the following languages:
  * English (US)
  * Italian (Swiss)
  * German (Germany)
    * Changing the language will be saved in a `launcher.json` file so it will be loaded for the next time the launcher is opened.
### Quality of Life
* Implemented a fix in case there is a profile saved in the `Profile` Path, but no actual data existed about the profile in the `Network Data` path.
  * This will now create a new profile with the same username.
  * Pressing either `Delete this profile` or `Delete all profiles` will now show a new confirmation dialog window to the user, before proceeding with the deletion.
### Bugfixes
* Trying to press `Play` without selecting a profile will now show an error dialog.
* Fixed an issue when trying to press `Play` by writing an invalid username (e.g. `ashaiuwhui`) would crash the application.
* Fixed an issue that allowed the user to select a profile, go to the `Manage Profiles` tab, delete the profile and then launch the profile.
* `Manage Profiles` section: 
  * Fixed an issue that would crash the application when the user tried to save a profile without changing their username. Previously, this would not crash if the user changed the profile's name.


## Release 1.8.0
* Updated project `.NET` version from `.NET 6.0` to `.NET 8.0`
* Fixed flickering of the ``Profile Settings``, and ``Manage Profiles`` when hovering with the mouse.
* Removed news webpage.
  * Because of this, the size of the launcher is also smaller.
* Removed the `Flyffulator` shortcut since the website doesn't work anymore.
* Implemented a new way to make ``New profiles``, simplifying the process.
* Moved toolbar from the top to the left.
  * Toolbar now automatically hides after pressing a shortcut.
  * Implemented ``Flyff.Me`` shortcut, which has a madrigal map with all enemies info + where the player should farm to get the most experience.
* Button ``Delete this profile`` and ``Delete all profiles`` now properly delete the selected/all profiles, instead of launching an exception.
  * This also includes the "simulated" browser page, which also reduces the size occupied by the program.

## Release 1.7.1
* Fixed an issue where the *Manage profiles* tab could not save the new changes to a profile.

## Release 1.7.0
* Implemented *Manage profiles* tab to change stuff about a selected profile.
* Implemented *Full screen* in the *Window Settings* tab.

## Release 1.6.0
* Implemented button *Close news tab* to improve performance. Pressing this button will dispose the news section and can only be accessed again by closing and opening the program.
* Implemented button *Frozen game? Click here* in the utility toolbar. If your game got stuck loading into an area or just isn't responding, click this button.

## Release 1.5.0
* Changed utility toolbar key-binding from *F1* to *Home*.
* Fixed an issue where trying to top-up would result in a "Your browser is out of date!" error.

## Release 1.4.0
* Added utility toolbar to quickly access the wikis.

## Release 1.3.2
* Fixed an issue where the user's preferred resolution was not being correctly saved.

## Release 1.3.1
* Updates the location of the saved launcher data.

## Release 1.3.0
* Added icons to the program.
* Renamed the folder *Users* to *Profiles* to avoid users thinking that the program is storing the flyff's account information.
* Added random cycling background images to the launcher.
* Removed the resize window option from the launcher.
* Removed *Launch game* from the game's window and will now automatically launch after selecting a profile.
* Fixed bug where the game's window form would not update correctly the first time the game was launched.
* Both launcher and game window will now start at the center of the selected screen.

## Release 1.2.0
* Implemented full-screen by pressing *F11*.

## Release 1.1.0
* After pressing 'Launch game' the button will hide itself and the window will be maximized.

## Release 1.0.0
* Initial release of the client.
