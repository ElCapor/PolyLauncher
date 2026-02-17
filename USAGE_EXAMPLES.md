# PolyLauncher - Usage Examples

## For End Users

### First Time Setup

1. **Download and Extract**
   - Extract PolyLauncher.exe to a folder (e.g., `C:\PolyLauncher\`)

2. **Initial Launch**
   - Double-click `PolyLauncher.exe`
   - You'll see a prompt to configure the launcher

3. **Configure Settings**
   - Click "Configure Launcher"
   - Set your preferences:
     ```
     ? Enable HWID Spoofer (experimental)
     ? Auto-Start Executor (experimental)
     Launch Duration: 5 seconds
     Background Color: #1E1E1E (dark gray)
     Loading Bar Color: #007ACC (blue)
     Text Color: #FFFFFF (white)
     Loading Text: "Loading Polytoria"
     ```

4. **Register Protocol**
   - Click "Register Protocol Handler (polytoria://)"
   - This allows the launcher to work with browser links

5. **Save**
   - Click "Save Settings"
   - Close the configuration window

### Normal Usage

1. **Play a Game**
   - Go to Polytoria.com
   - Find a game you want to play
   - Click "Play"
   - Browser will open `polytoria://` link
   - Launcher automatically:
     - Shows loading screen
     - Counts down (default 5 seconds)
     - Checks for updates
     - Downloads updates if needed
     - Launches the game
     - Closes itself

2. **During Countdown**
   - **Start Now**: Click to skip countdown
   - **Configure**: Opens settings to change preferences

### Customization Examples

#### Dark Theme (Default)
```
Background: #1E1E1E
Loading Bar: #007ACC
Text: #FFFFFF
```

#### Light Theme
```
Background: #F5F5F5
Loading Bar: #0078D4
Text: #000000
```

#### Purple Theme
```
Background: #2B1B3D
Loading Bar: #9B59B6
Text: #ECF0F1
```

#### Matrix Theme
```
Background: #000000
Loading Bar: #00FF00
Text: #00FF00
Loading Text: "Entering the Matrix..."
```

#### Custom Branding
```
Background: #Your_Brand_Color
Loading Bar: #Your_Accent_Color
Text: #FFFFFF
Loading Text: "Loading [Your Name]'s Game"
Custom Icon: C:\Path\To\Your\Logo.png
```

## For Advanced Users

### Command-Line Usage

#### Open Configuration Anytime
```batch
PolyLauncher.exe --configure
```

#### Re-register Protocol Handler
```batch
PolyLauncher.exe --register-protocol
```

#### Unregister Protocol Handler
```batch
PolyLauncher.exe --unregister-protocol
```

#### Reset All Settings
```batch
PolyLauncher.exe --reset-settings
```

#### Direct Protocol Launch (for testing)
```batch
PolyLauncher.exe polytoria://client/YOUR_AUTH_TOKEN_HERE
```

### Batch Scripts

#### Quick Configure Script
Create `Configure.bat`:
```batch
@echo off
echo Opening PolyLauncher Configuration...
start "" "PolyLauncher.exe" --configure
```

#### Reset and Configure Script
Create `ResetAndConfigure.bat`:
```batch
@echo off
echo Resetting PolyLauncher settings...
"PolyLauncher.exe" --reset-settings
timeout /t 2 /nobreak >nul
echo Opening configuration...
start "" "PolyLauncher.exe" --configure
```

#### Launch with Custom Duration Script
1. Open settings
2. Change launch duration
3. Create `QuickLaunch.bat`:
```batch
@echo off
:: This will use your saved custom duration
start "" "PolyLauncher.exe" polytoria://client/TOKEN
```

### Settings File Customization

**Location**: `%AppData%\PolyLauncher\Settings.json`

You can manually edit this file with any text editor:

```json
{
  "enableHwidSpoofer": false,
  "autoStartExecutor": false,
  "customLaunchDuration": 3,
  "customLoadingIcon": "C:\\Users\\YourName\\Pictures\\logo.png",
  "backgroundColor": "#1E1E1E",
  "loadingBarColor": "#00FF00",
  "loadingText": "Hacking the mainframe...",
  "textColor": "#00FF00",
  "clientManifest": [],
  "skipUpdates": false,
  "firstRun": false
}
```

**Note**: After editing, restart the launcher.

### Custom Loading Icons

Supported formats:
- PNG (recommended, supports transparency)
- JPG/JPEG
- ICO
- BMP

Recommended size: 256x256 pixels

#### Example Custom Icons
1. **Animated Loading Icon**: Use an animated GIF (may require code modification)
2. **Brand Logo**: Your organization's logo
3. **Game-Specific**: Different icons per game (manual switching)

### Modding Features (Experimental)

**WARNING**: These features are placeholders and may violate Polytoria Terms of Service.

#### Enable HWID Spoofer
1. Open Configuration
2. Check "Enable HWID Spoofer"
3. Save Settings
4. Current implementation is placeholder
5. To implement: Modify `Services/ModdingService.cs`

#### Enable Auto-Start Executor
1. Place your executor in: `%AppData%\PolyLauncher\Executor\`
2. Name it: `executor.exe` or `executor.dll`
3. Open Configuration
4. Check "Auto-Start Executor"
5. Save Settings
6. Current implementation is placeholder
7. To implement: Modify `Services/ModdingService.cs`

### Multiple Configurations

You can create multiple settings profiles:

1. **Create Backup**:
```batch
copy "%AppData%\PolyLauncher\Settings.json" "%AppData%\PolyLauncher\Settings_Backup.json"
```

2. **Create Profile**:
```batch
copy "%AppData%\PolyLauncher\Settings.json" "%AppData%\PolyLauncher\Settings_Gaming.json"
```

3. **Switch Profile**:
```batch
copy "%AppData%\PolyLauncher\Settings_Gaming.json" "%AppData%\PolyLauncher\Settings.json"
```

## For Power Users / Developers

### Testing Different Releases

The launcher supports both Stable and Beta:

#### Stable Client
```
polytoria://client/TOKEN
```

#### Beta Client
```
polytoria://clientbeta/TOKEN
```

#### Test Mode (Solo)
```
polytoria://test/TOKEN/map/path
```

### Version Management

Installed clients are stored at:
```
%AppData%\PolyLauncher\Client\[version]\
```

To manage versions:
1. Navigate to the folder
2. See all installed versions
3. Delete old versions to save space
4. Keep multiple versions for rollback

### API Integration

The launcher calls:
```
GET https://api.polytoria.com/v1/launcher/updates?os=windows&release=stable
```

Response structure:
```json
{
  "maintenance": false,
  "client": {
    "version": "1.0.0",
    "download": "https://...",
    "release": "Stable"
  },
  "launcher": {
    "version": "1.0.0",
    "download": "https://..."
  }
}
```

### Skipping Updates

To skip updates temporarily:
1. Edit `Settings.json`
2. Set `"skipUpdates": true`
3. Save and restart

**Note**: This launches the currently installed version without checking for updates.

### Debugging

Enable debug mode:
1. Run from Visual Studio with debugger attached
2. View Output window
3. See all Debug.WriteLine() messages

Or add logging to any method in the source code.

## Troubleshooting Examples

### Problem: Launcher won't start
**Solution**:
```batch
:: Check if .NET 8 is installed
dotnet --version

:: If not, install .NET 8 Desktop Runtime
:: Download from: https://dotnet.microsoft.com/download/dotnet/8.0
```

### Problem: Settings keep resetting
**Solution**:
```batch
:: Check permissions
icacls "%AppData%\PolyLauncher"

:: Grant full control
icacls "%AppData%\PolyLauncher" /grant %USERNAME%:F
```

### Problem: Protocol handler not working
**Solution**:
```batch
:: Re-register as administrator
:: Right-click Command Prompt -> Run as Administrator
cd C:\Path\To\PolyLauncher
PolyLauncher.exe --register-protocol
```

### Problem: Client won't download
**Solution**:
```batch
:: Clear temp folder
rd /s /q "%Temp%\PolyLauncher"

:: Check internet connectivity
ping api.polytoria.com

:: Try manual download
:: Visit https://polytoria.com in browser
```

### Problem: Custom colors not applying
**Solution**:
- Ensure hex colors start with `#`
- Valid format: `#RRGGBB` (e.g., `#FF0000` for red)
- Use a color picker tool
- Example valid colors:
  - `#000000` (black)
  - `#FFFFFF` (white)
  - `#FF0000` (red)
  - `#00FF00` (green)
  - `#0000FF` (blue)

## Advanced Customization

### Creating a Portable Installation

1. Copy these files/folders:
   ```
   PolyLauncher.exe
   PolyLauncher.dll
   Newtonsoft.Json.dll
   CommunityToolkit.Mvvm.dll
   ```

2. Modify code to use relative paths instead of AppData

3. Settings will be local to the launcher directory

### Integrating with Other Tools

#### Discord Rich Presence
Modify `ModdingService.cs` to:
1. Detect game launch
2. Connect to Discord API
3. Update presence

#### Game Time Tracking
Modify `ModdingService.cs` to:
1. Track process start time
2. Log to file when game exits
3. Calculate play time

#### Auto-Screenshot
Modify `ModdingService.cs` to:
1. Monitor game process
2. Take periodic screenshots
3. Save to custom folder

## Tips & Tricks

### Tip 1: Fastest Launch
Set launch duration to 1 second:
```json
"customLaunchDuration": 1
```

### Tip 2: Disable Countdown
Set to 0 to launch immediately:
```json
"customLaunchDuration": 0
```
(Note: May require code modification to support 0)

### Tip 3: Custom Fonts
Modify XAML in `MainWindow.xaml`:
```xaml
<TextBlock FontFamily="Arial Black" ... />
```

### Tip 4: Window Position
Launcher always centers. To change:
Modify `MainWindow.xaml`:
```xaml
WindowStartupLocation="Manual"
Left="100"
Top="100"
```

### Tip 5: Always on Top
Modify `MainWindow.xaml`:
```xaml
Topmost="True"
```

---

**Need More Help?**
- Check README.md for overview
- Check DEVELOPER_GUIDE.md for technical details
- Review the source code for implementation details
