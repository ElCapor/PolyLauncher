using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PolyLauncher.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly Services.SettingsService _settingsService;
        private readonly Services.UpdateService _updateService;
        private readonly Services.ModdingService _moddingService;
        private readonly DispatcherTimer _countdownTimer;
        private Models.LaunchArguments? _launchArguments;
        private CancellationTokenSource? _cancellationTokenSource;

        [ObservableProperty]
        private bool showConfiguration;

        [ObservableProperty]
        private bool showLoading;

        [ObservableProperty]
        private string statusMessage = "Initializing...";

        [ObservableProperty]
        private int progressValue;

        [ObservableProperty]
        private int countdownSeconds;

        [ObservableProperty]
        private string loadingText = "Loading Polytoria";

        [ObservableProperty]
        private Brush backgroundBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));

        [ObservableProperty]
        private Brush loadingBarBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204));

        [ObservableProperty]
        private Brush textBrush = new SolidColorBrush(Colors.White);

        [ObservableProperty]
        private string? customIconPath;

        public MainViewModel(Services.SettingsService settingsService, Services.UpdateService updateService)
        {
            _settingsService = settingsService;
            _updateService = updateService;
            _moddingService = new Services.ModdingService(settingsService);
            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += CountdownTimer_Tick;

            // Subscribe to settings changes to reload configuration immediately
            Services.SettingsService.SettingsChanged += (s, e) => 
            {
                Application.Current.Dispatcher.Invoke(() => RefreshVisualSettings());
            };
        }

        private void RefreshVisualSettings()
        {
            var settings = _settingsService.LoadSettings();
            LoadingText = settings.LoadingText;
            try { BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.BackgroundColor)); } catch { }
            try { LoadingBarBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.LoadingBarColor)); } catch { }
            try { TextBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.TextColor)); } catch { }
            CustomIconPath = settings.CustomLoadingIcon;
        }

        public async Task InitializeAsync(Models.LaunchArguments? launchArgs)
        {
            Services.Logger.Log("Initializing MainViewModel...");
            _launchArguments = launchArgs;
            var settings = _settingsService.LoadSettings();

            LoadingText = settings.LoadingText;
            BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.BackgroundColor));
            LoadingBarBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.LoadingBarColor));
            TextBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.TextColor));
            CustomIconPath = settings.CustomLoadingIcon;

            if (settings.FirstRun || _launchArguments == null || !_launchArguments.IsValid)
            {
                Services.Logger.Log($"First run or invalid arguments. FirstRun: {settings.FirstRun}, " +
                                  $"LaunchArguments is null: {_launchArguments == null}, " +
                                  $"LaunchArguments.IsValid: {_launchArguments?.IsValid ?? false}. " +
                                  "Showing configuration.");
                ShowConfiguration = true;
                ShowLoading = false;
                return;
            }

            Services.Logger.Log($"Launch arguments detected: Type={_launchArguments.Type}, Release={_launchArguments.Release}, Map={_launchArguments.Map}");
            ShowConfiguration = false;
            ShowLoading = true;

            CountdownSeconds = settings.CustomLaunchDuration;
            Services.Logger.Log($"Starting countdown: {CountdownSeconds}s");
            _countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            CountdownSeconds--;
            
            if (CountdownSeconds <= 0)
            {
                _countdownTimer.Stop();
                _ = StartLaunchProcessAsync();
            }
        }

        [RelayCommand]
        private void Configure()
        {
            _countdownTimer.Stop();
            _cancellationTokenSource?.Cancel();
            ShowConfiguration = true;
            ShowLoading = false;
        }

        [RelayCommand]
        private async Task StartImmediately()
        {
            _countdownTimer.Stop();
            await StartLaunchProcessAsync();
        }

        private async Task StartLaunchProcessAsync()
        {
            Services.Logger.Log("Starting launch process...");
            try
            {
                if (_launchArguments == null || !_launchArguments.IsValid)
                {
                    Services.Logger.LogError("Invalid launch arguments in StartLaunchProcessAsync");
                    ShowError("Invalid launch arguments");
                    return;
                }

                _cancellationTokenSource = new CancellationTokenSource();
                
                var progress = new Progress<int>(value =>
                {
                    ProgressValue = value;
                });

                var status = new Progress<string>(message =>
                {
                    StatusMessage = message;
                });

                var (success, error, version) = await _updateService.CheckUpdateAndPrepareAsync(
                    _launchArguments, 
                    progress, 
                    status, 
                    _cancellationTokenSource.Token);

                if (!success || string.IsNullOrEmpty(version))
                {
                    Services.Logger.LogError($"Update check and prepare failed: {error ?? "Unknown error"}. Version: {version ?? "None"}");
                    ShowError(error ?? "Update failed");
                    return;
                }

                StatusMessage = "Launching Polytoria...";
                ProgressValue = 100;

                await Task.Delay(1000);

                Process? gameProcess;

                if (_launchArguments.IsCreator)
                {
                    Services.Logger.Log($"Launching Creator Version {version}...");
                    gameProcess = _updateService.LaunchCreator(version, _launchArguments);
                }
                else
                {
                    // Prepare mod files (HWID Spoofer, Executor) - only for Client
                    Services.Logger.Log($"Preparing mods for version {version} before launch...");
                    await _moddingService.PrepareModsAsync(version);

                    Services.Logger.Log($"Launching Client Version {version}...");
                    gameProcess = _updateService.LaunchClient(version, _launchArguments);
                }
                
                if (gameProcess != null)
                {
                    Services.Logger.Log($"Application launched successfully. PID: {gameProcess.Id}. Monitoring process...");
                    if (!_launchArguments.IsCreator)
                    {
                        _ = _moddingService.MonitorGameProcessAsync(version, gameProcess, _cancellationTokenSource.Token);
                    }
                }
                else
                {
                    Services.Logger.LogError("Failed to launch application process.");
                    ShowError($"Could not start Polytoria {(_launchArguments.IsCreator ? "Creator" : "Client")}.");
                    return;
                }

                Services.Logger.Log("PolyLauncher task completed. Shutting down launcher.");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                ShowError($"An error occurred: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            Services.Logger.LogError($"UI Error Shown: {message}");
            StatusMessage = $"Error: {message}";
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        [RelayCommand]
        private void CloseConfiguration()
        {
            var settings = _settingsService.LoadSettings();
            
            if (_launchArguments == null || !_launchArguments.IsValid)
            {
                // If we don't have valid launch arguments, just stay in the configuration view
                // and don't show the warning message as requested by the user.
                return;
            }

            RefreshVisualSettings();

            ShowConfiguration = false;
            ShowLoading = true;
            CountdownSeconds = settings.CustomLaunchDuration;
            _countdownTimer.Start();
        }
    }
}
