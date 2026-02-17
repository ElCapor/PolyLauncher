# PolyLauncher - Developer Quick Start

## Project Overview

This is a fully-featured, customizable launcher for Polytoria built in C# WPF using the MVVM pattern. It provides 1:1 feature parity with the original Polytoria launcher while adding extensive customization and modding capabilities.

## Architecture

### MVVM Pattern
The project follows the Model-View-ViewModel pattern:
- **Models**: Data structures (`LauncherSettings`, `LaunchArguments`, `UpdateResponse`)
- **Views**: XAML UI files (`MainWindow.xaml`, `ConfigurationWindow.xaml`)
- **ViewModels**: Business logic (`MainViewModel`, `ConfigurationViewModel`)
- **Services**: Core functionality (Settings, Updates, Protocol, Modding)

### Project Structure

```
PolyLauncher/
??? Models/                    # Data models
?   ??? LauncherSettings.cs   # Persistent settings
?   ??? LaunchArguments.cs    # Protocol arguments
?   ??? UpdateResponse.cs     # API responses
?
??? Services/                  # Business logic services
?   ??? SettingsService.cs    # Settings management
?   ??? UpdateService.cs      # Update/download logic
?   ??? ProtocolService.cs    # Protocol handler registration
?   ??? ModdingService.cs     # Modding features (HWID, executor)
?   ??? InstallationService.cs # Install/Uninstall logic
?
??? ViewModels/               # MVVM ViewModels
?   ??? MainViewModel.cs      # Main window logic
?   ??? ConfigurationViewModel.cs  # Configuration logic
?
??? Views/                    # Additional windows
?   ??? ConfigurationWindow.xaml   # Settings UI
?
??? MainWindow.xaml           # Primary UI
??? App.xaml.cs              # Application entry point
??? PolyLauncher.csproj      # Project file
```

## Key Features

### 1. Protocol Handler (`polytoria://`)
- **ProtocolService.cs**: Handles Windows registry registration
- **App.xaml.cs**: Parses protocol arguments on startup
- Supports: `client`, `clientbeta`, `test`, `testbeta`, `creator`, `creatorbeta`

### 2. Auto-Update System
- **UpdateService.cs**: Downloads and extracts client updates
- Version manifest tracking per release (Stable/Beta)
- Progress reporting for downloads
- Automatic cleanup of old versions

### 3. Customization System
- **SettingsService.cs**: JSON-based settings persistence
- Colors: Background, loading bar, text
- Custom loading duration (1-60 seconds)
- Custom loading icon support
- Custom loading text

### 4. Installation System
- **InstallationService.cs**: Handles copying the launcher and assets to a target directory.
- **App.xaml.cs**: Supports `--install` and `--uninstall` arguments.
- **Uninstallation**: Uses a delayed batch script (`cmd.exe`) for self-deletion after process exit.

### 5. Modding Features (Placeholders)
- **ModdingService.cs**: Framework for modding features
- HWID Spoofer (to be implemented)
- Executor auto-start (to be implemented)
- Process monitoring and injection hooks

## Data Flow

### Normal Launch Flow
```
1. User clicks "Play" on Polytoria.com
2. Browser redirects to polytoria://client/[token]
3. ProtocolService.ParseProtocolArguments() extracts token
4. MainViewModel.InitializeAsync() loads settings
5. Countdown timer shows for configured duration
6. UpdateService.CheckForUpdatesAsync() contacts API
7. If update available: DownloadAndExtractClientAsync()
8. UpdateService.LaunchClient() starts the game
9. ModdingService.MonitorGameProcessAsync() handles mods
10. Launcher exits
```

### First Run Flow
```
1. App.xaml.cs detects FirstRun == true
2. Shows configuration UI
3. User customizes settings
4. Registers protocol handler
5. Saves settings (FirstRun = false)
6. Ready for normal launch flow
```

## Settings Storage

**Location**: `%AppData%\PolyLauncher\Settings.json`

**Structure**:
```json
{
  "enableHwidSpoofer": false,
  "autoStartExecutor": false,
  "customLaunchDuration": 5,
  "customLoadingIcon": null,
  "backgroundColor": "#1E1E1E",
  "loadingBarColor": "#007ACC",
  "loadingText": "Loading Polytoria",
  "textColor": "#FFFFFF",
  "clientManifest": [
    {
      "release": "Stable",
      "version": "1.0.0",
      "installedAt": "2024-01-01T00:00:00Z"
    }
  ],
  "skipUpdates": false,
  "firstRun": true
}
```

## Client Installation

**Location**: `%AppData%\PolyLauncher\Client\[version]\`

Each version is stored separately to allow:
- Rollback to previous versions
- Testing multiple versions
- Easy cleanup

## Command-Line Interface

#### Usage
```bash
# Open configuration
PolyLauncher.exe --configure

# Install to default directory
PolyLauncher.exe --install

# Install to custom directory
PolyLauncher.exe --install "C:\Your\Path"

# Uninstall launcher
PolyLauncher.exe --uninstall

# Register protocol handler
PolyLauncher.exe --register-protocol

# Unregister protocol handler
PolyLauncher.exe --unregister-protocol

# Reset all settings
PolyLauncher.exe --reset-settings

# Launch via protocol
PolyLauncher.exe polytoria://client/[token]
```

## Extending the Launcher

### Adding New Modding Features

1. **Add to ModdingService.cs**:
```csharp
public void ApplyCustomMod()
{
    var settings = _settingsService.LoadSettings();
    // Your mod logic here
}
```

2. **Add Settings Property**:
```csharp
// In LauncherSettings.cs
[JsonProperty("enableCustomMod")]
public bool EnableCustomMod { get; set; } = false;
```

3. **Add UI Control**:
```xaml
<!-- In ConfigurationWindow.xaml -->
<CheckBox Content="Enable Custom Mod" 
          IsChecked="{Binding EnableCustomMod}"/>
```

4. **Add ViewModel Property**:
```csharp
// In ConfigurationViewModel.cs
[ObservableProperty]
private bool enableCustomMod;
```

### Adding New API Endpoints

1. **Update Models** (if needed):
```csharp
// In UpdateResponse.cs or create new model
public class NewApiResponse { ... }
```

2. **Add Service Method**:
```csharp
// In UpdateService.cs or create new service
public async Task<NewApiResponse?> CallNewApiAsync()
{
    var response = await _httpClient.GetAsync("https://api.polytoria.com/...");
    // Parse and return
}
```

3. **Call from ViewModel**:
```csharp
// In MainViewModel.cs or appropriate ViewModel
var result = await _updateService.CallNewApiAsync();
```

## Dependencies

### NuGet Packages
- **Newtonsoft.Json** (13.0.3): JSON serialization
- **CommunityToolkit.Mvvm** (8.2.2): MVVM helpers

### Framework
- **.NET 8.0 Windows Desktop**: Modern .NET runtime
- **WPF**: Windows Presentation Foundation

## Testing

### Manual Testing Checklist
- [ ] First run configuration
- [ ] Protocol handler registration
- [ ] Settings save/load
- [ ] Settings reset
- [ ] Download and extract client
- [ ] Launch client (Stable)
- [ ] Launch client (Beta)
- [ ] Custom colors apply correctly
- [ ] Custom loading duration works
- [ ] Countdown can be interrupted
- [ ] Start immediately button works

### Testing Without Browser
Create a batch file to simulate protocol launch:
```batch
@echo off
PolyLauncher.exe polytoria://client/YOUR_TOKEN_HERE
```

## Future Enhancements

### Self-Update (Priority)
Currently the launcher doesn't update itself. To implement:

1. Add launcher version check in `UpdateResponse`
2. Download new launcher to temp directory
3. Launch updater process that:
   - Waits for launcher to exit
   - Replaces launcher executable
   - Relaunches launcher

Reference the deobfuscated code for similar pattern (without using Updater.exe)

### HWID Spoofing Implementation
**WARNING**: May violate Terms of Service

Potential approaches:
- Registry key spoofing
- WMI query interception
- MAC address randomization
- System GUID modification

### Executor Integration
**WARNING**: May violate Terms of Service

Potential approaches:
- DLL injection via `CreateRemoteThread`
- Manual mapping
- Process hollowing
- Code cave injection

## Debugging

### Common Issues

**Settings not saving**:
- Check %AppData%\PolyLauncher\ permissions
- Verify JSON serialization in SettingsService

**Protocol not registering**:
- Run as administrator
- Check HKCU\Software\Classes\polytoria in Registry

**Client won't download**:
- Verify API connectivity
- Check token validity
- Clear %Temp%\PolyLauncher\

### Logging
Add logging to any method:
```csharp
System.Diagnostics.Debug.WriteLine("[ServiceName] Your log message");
```

View in Visual Studio Output window when debugging.

## Contributing

When contributing:
1. Follow existing MVVM pattern
2. Use CommunityToolkit.Mvvm attributes
3. Keep services stateless where possible
4. Add XML documentation to public methods
5. Test on clean install

## License & Disclaimer

This project is for **educational purposes only**. 

- Polytoria and related trademarks are property of Polytoria Inc.
- Not officially affiliated with or endorsed by Polytoria
- Use at your own risk
- Modding features may violate Terms of Service

## Contact & Support

For issues or questions:
1. Check the README.md
2. Review this developer guide
3. Examine the deobfuscated launcher.js for reference behavior

---

**Happy Coding!** ??
