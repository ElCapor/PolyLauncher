using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace PolyLauncher.ViewModels
{
    public partial class ConfigurationViewModel : ObservableObject
    {
        private readonly Services.SettingsService _settingsService;
        private readonly Services.InstallationService _installationService;

        [ObservableProperty]
        private bool enableHwidSpoofer;

        [ObservableProperty]
        private bool autoStartExecutor;

        [ObservableProperty]
        private bool allowMulticlient;

        [ObservableProperty]
        private int customLaunchDuration;

        [ObservableProperty]
        private string customLoadingIcon = string.Empty;

        [ObservableProperty]
        private string backgroundColor = "#1E1E1E";

        [ObservableProperty]
        private string loadingBarColor = "#007ACC";

        [ObservableProperty]
        private string loadingText = "Loading Polytoria";

        [ObservableProperty]
        private string textColor = "#FFFFFF";

        [ObservableProperty]
        private Brush backgroundBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));

        [ObservableProperty]
        private Brush loadingBarBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204));

        [ObservableProperty]
        private Brush textBrush = new SolidColorBrush(Colors.White);

        [ObservableProperty]
        private string installPath = string.Empty;

        [ObservableProperty]
        private bool isInstalled;

        public ConfigurationViewModel(Services.SettingsService settingsService)
        {
            _settingsService = settingsService;
            _installationService = new Services.InstallationService(settingsService);
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.LoadSettings();
            EnableHwidSpoofer = settings.EnableHwidSpoofer;
            AutoStartExecutor = settings.AutoStartExecutor;
            AllowMulticlient = settings.AllowMulticlient;
            CustomLaunchDuration = settings.CustomLaunchDuration;
            CustomLoadingIcon = settings.CustomLoadingIcon ?? string.Empty;
            BackgroundColor = settings.BackgroundColor;
            LoadingBarColor = settings.LoadingBarColor;
            LoadingText = settings.LoadingText;
            TextColor = settings.TextColor;
            InstallPath = settings.InstallPath ?? _installationService.GetDefaultInstallPath();
            IsInstalled = _installationService.IsRunningFromInstallPath();

            UpdateBrushes();
        }

        partial void OnBackgroundColorChanged(string value)
        {
            UpdateBrushes();
        }

        partial void OnLoadingBarColorChanged(string value)
        {
            UpdateBrushes();
        }

        partial void OnTextColorChanged(string value)
        {
            UpdateBrushes();
        }

        private void UpdateBrushes()
        {
            try
            {
                BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColor));
            }
            catch { }

            try
            {
                LoadingBarBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(LoadingBarColor));
            }
            catch { }

            try
            {
                TextBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(TextColor));
            }
            catch { }
        }

        [RelayCommand]
        private void BrowseIcon()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.ico;*.bmp|All Files|*.*",
                Title = "Select Loading Icon"
            };

            if (dialog.ShowDialog() == true)
            {
                CustomLoadingIcon = dialog.FileName;
            }
        }

        [RelayCommand]
        private void SaveSettings()
        {
            ApplySettings();
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void SaveAndRestart()
        {
            ApplySettings();
            if (Application.Current is App app)
            {
                app.Restart();
            }
        }

        private void ApplySettings()
        {
            _settingsService.UpdateSettings(settings =>
            {
                settings.EnableHwidSpoofer = EnableHwidSpoofer;
                settings.AutoStartExecutor = AutoStartExecutor;
                settings.AllowMulticlient = AllowMulticlient;
                settings.CustomLaunchDuration = Math.Max(1, Math.Min(60, CustomLaunchDuration));
                settings.CustomLoadingIcon = string.IsNullOrWhiteSpace(CustomLoadingIcon) ? null : CustomLoadingIcon;
                settings.BackgroundColor = BackgroundColor;
                settings.LoadingBarColor = LoadingBarColor;
                settings.LoadingText = LoadingText;
                settings.TextColor = TextColor;
                settings.InstallPath = InstallPath;
                settings.FirstRun = false;
            });
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to defaults?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _settingsService.ResetSettings();
                LoadSettings();
                MessageBox.Show("Settings reset to defaults!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void RegisterProtocol()
        {
            if (Services.ProtocolService.IsRegisteredAsProtocolHandler())
            {
                MessageBox.Show("Already registered as protocol handler!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Services.ProtocolService.RegisterAsProtocolHandler();
            MessageBox.Show("Protocol handler registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void UnregisterProtocol()
        {
            if (!Services.ProtocolService.IsRegisteredAsProtocolHandler())
            {
                MessageBox.Show("No protocol handler is currently registered.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to unregister PolyLauncher as the protocol handler for polytoria:// links?\n\nThis will prevent the browser from launching the game through this launcher.",
                "Confirm Unregistration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Services.ProtocolService.UnregisterProtocolHandler();
                MessageBox.Show("Protocol handler unregistered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void BrowseInstallPath()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Installation Directory",
                InitialDirectory = InstallPath
            };

            if (dialog.ShowDialog() == true)
            {
                InstallPath = dialog.FolderName;
            }
        }

        [RelayCommand]
        private async Task Install()
        {
            if (string.IsNullOrEmpty(InstallPath))
            {
                MessageBox.Show("Please select a valid installation path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_installationService.IsRunningFromInstallPath())
            {
                MessageBox.Show("PolyLauncher is already running from the installation path.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"PolyLauncher will be installed to:\n{InstallPath}\n\nDo you want to continue?",
                "Confirm Installation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _installationService.InstallAsync(InstallPath);
                if (success)
                {
                    var relaunch = MessageBox.Show(
                        "Installation successful! Do you want to relaunch PolyLauncher from the new location now?",
                        "Installation Complete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (relaunch == MessageBoxResult.Yes)
                    {
                        _installationService.RelaunchFrom(InstallPath);
                    }
                    else
                    {
                        IsInstalled = _installationService.IsRunningFromInstallPath();
                    }
                }
            }
        }

        [RelayCommand]
        private void Uninstall()
        {
            var result = MessageBox.Show(
                "Warning: This will unregister PolyLauncher as the protocol handler and delete all installation files.\n\nAre you sure you want to uninstall?",
                "Confirm Uninstall",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _installationService.Uninstall();
            }
        }
    }
}
