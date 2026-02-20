using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace PolyLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string[] _args = Array.Empty<string>();

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args != null)
                _args = e.Args;
                
            // Initialize logging first
            Services.Logger.Initialize();
            Console.SetOut(new Services.LogWriter());
            Console.SetError(new Services.LogWriter());

            // Handle unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                Services.Logger.LogError("Unhandled AppDomain Exception", args.ExceptionObject as Exception);
            };

            DispatcherUnhandledException += (s, args) =>
            {
                Services.Logger.LogError("Unhandled Dispatcher Exception", args.Exception);
                args.Handled = true;
            };

            base.OnStartup(e);

            Services.Logger.Log($"PolyLauncher starting up... Command line arguments: {string.Join(" ", e.Args)}");

            var settingsService = new Services.SettingsService();
            var installService = new Services.InstallationService(settingsService);

            var launchArgs = Services.ProtocolService.ParseProtocolArguments(e.Args);
            
            if (launchArgs != null)
            {
                 Services.Logger.Log($"Parsed protocol arguments: Type={launchArgs.Type}, Token={launchArgs.Token}, Map={launchArgs.Map}");
            }
            else
            {
                 Services.Logger.Log("No protocol arguments parsed or invalid format.");
            }

            if (e.Args.Length > 0 && e.Args[0] == "--configure")
            {
                var configWindow = new Views.ConfigurationWindow();
                configWindow.ShowDialog();
                Shutdown();
                return;
            }

            if (e.Args.Length > 0 && e.Args[0] == "--register-protocol")
            {
                Services.ProtocolService.RegisterAsProtocolHandler();
                MessageBox.Show("Protocol handler registered successfully!\n\nYou can now launch Polytoria games from your browser.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            if (e.Args.Length > 0 && e.Args[0] == "--unregister-protocol")
            {
                Services.ProtocolService.UnregisterProtocolHandler();
                MessageBox.Show("Protocol handler unregistered successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            if (e.Args.Length > 0 && (e.Args[0] == "--install" || e.Args[0] == "-i"))
            {
                var path = e.Args.Length > 1 ? e.Args[1] : installService.GetDefaultInstallPath();
                _ = installService.InstallAsync(path);
                Shutdown();
                return;
            }

            if (e.Args.Length > 0 && (e.Args[0] == "--uninstall" || e.Args[0] == "-u"))
            {
                installService.Uninstall();
                Shutdown();
                return;
            }

            if (e.Args.Length > 0 && e.Args[0] == "--reset-settings")
            {
                var result = MessageBox.Show(
                    "Are you sure you want to reset all settings to defaults?",
                    "Confirm Reset",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var settingsServ = new Services.SettingsService();
                    settingsServ.ResetSettings();
                    MessageBox.Show("Settings have been reset to defaults!", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Shutdown();
                return;
            }

            if (!Services.ProtocolService.IsRegisteredAsProtocolHandler())
            {
                var result = MessageBox.Show(
                    "PolyLauncher is not registered as the protocol handler for polytoria:// links.\n\n" +
                    "Would you like to register it now?",
                    "Protocol Handler Registration",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Services.ProtocolService.RegisterAsProtocolHandler();
                }
            }

            var settings = settingsService.LoadSettings();
            if (settings.FirstRun && !installService.IsRunningFromInstallPath())
            {
                var installResult = MessageBox.Show(
                    "PolyLauncher is not installed on your system.\n\n" +
                    "Would you like to install it now to the default location?\n" +
                    installService.GetDefaultInstallPath(),
                    "Installation Suggested",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (installResult == MessageBoxResult.Yes)
                {
                    _ = installService.InstallAsync(installService.GetDefaultInstallPath());
                    // After install, we could relaunch, or just continue. 
                    // Let's just continue for simplicity, or we could relaunch if install succeeds.
                }
            }

            var updateService = new Services.UpdateService(settingsService);
            var viewModel = new ViewModels.MainViewModel(settingsService, updateService);

            var mainWindow = new MainWindow(viewModel);
            mainWindow.Show();

            _ = viewModel.InitializeAsync(launchArgs);
        }

        public void Restart()
        {
            try
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (exePath != null)
                {
                    var argsString = string.Join(" ", _args.Select(a => $"\"{a}\""));
                    Process.Start(exePath, argsString);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                Services.Logger.LogError("Failed to restart launcher", ex);
            }
        }
    }

}
