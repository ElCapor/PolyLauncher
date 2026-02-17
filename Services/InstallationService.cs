using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace PolyLauncher.Services
{
    public class InstallationService
    {
        private readonly SettingsService _settingsService;

        public InstallationService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public string GetDefaultInstallPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "Programs", "PolyLauncher");
        }

        public bool IsRunningFromInstallPath()
        {
            var settings = _settingsService.LoadSettings();
            if (string.IsNullOrEmpty(settings.InstallPath))
                return false;

            var currentExe = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExe))
                return false;

            return currentExe.StartsWith(settings.InstallPath, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> InstallAsync(string targetPath)
        {
            try
            {
                var sourcePath = AppDomain.CurrentDomain.BaseDirectory;
                targetPath = Path.GetFullPath(targetPath);

                // Force overwrite: If directory exists, we overwrite. 
                // To ensure clean install, we could delete it, but current process might be running from there (unlikely if user followed promps)
                if (!IsRunningFromInstallPath() && Directory.Exists(targetPath))
                {
                    // Basic cleanup: try to delete files first to ensure a fresh state
                    try
                    {
                        foreach (var file in Directory.GetFiles(targetPath, "*.*", SearchOption.AllDirectories))
                        {
                            File.SetAttributes(file, FileAttributes.Normal); // Remove read-only etc
                            File.Delete(file);
                        }
                    }
                    catch
                    {
                        // Some files might be in use, we'll try to overwrite them anyway with File.Copy(..., true)
                    }
                }

                Directory.CreateDirectory(targetPath);

                // Copy all files from current directory
                var exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule!.FileName!);
                var exeInTarget = Path.Combine(targetPath, exeName);

                foreach (var file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(sourcePath, file);
                    var destFile = Path.Combine(targetPath, relativePath);
                    var destDir = Path.GetDirectoryName(destFile);
                    
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    File.Copy(file, destFile, true);
                }

                _settingsService.UpdateSettings(s => s.InstallPath = targetPath);

                // Re-register protocol handler to point to the new location
                ProtocolService.RegisterAsProtocolHandler(exeInTarget);

                // Create shortcuts for user visibility
                CreateShortcuts(exeInTarget);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation failed: {ex.Message}", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void Uninstall()
        {
            try
            {
                var settings = _settingsService.LoadSettings();
                var installPath = settings.InstallPath;

                // 1. Unregister protocol handler
                ProtocolService.UnregisterProtocolHandler();

                // 2. Remove shortcuts
                RemoveShortcuts();

                // 3. Clear install path in settings
                _settingsService.UpdateSettings(s => s.InstallPath = null);

                if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath))
                {
                    MessageBox.Show("Uninstall completed. No files were deleted as installation path was unknown or missing.", 
                        "Uninstall", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown();
                    return;
                }

                // 3. Initiate self-deletion and close
                var currentExe = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(currentExe))
                {
                    Application.Current.Shutdown();
                    return;
                }

                // Small batch to wait, delete, and remove folder
                // Use cmd /c "choice /c y /n /d y /t 2 & rd /s /q "{installPath}""
                // Choice gives time for the app to exit.
                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c choice /c y /n /d y /t 2 & rd /s /q \"{installPath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(processInfo);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uninstall failed: {ex.Message}", "Uninstall Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public void RelaunchFrom(string targetPath)
        {
            var exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule!.FileName!);
            var newExePath = Path.Combine(targetPath, exeName);
            
            if (File.Exists(newExePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = newExePath,
                    UseShellExecute = true
                });
                Application.Current.Shutdown();
            }
        }

        private void CreateShortcuts(string targetExePath)
        {
            try
            {
                string shortcutName = "PolyLauncher.lnk";
                string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), shortcutName);
                string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), shortcutName);

                CreateShortcut(desktopPath, targetExePath);
                CreateShortcut(startMenuPath, targetExePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create shortcuts: {ex.Message}");
            }
        }

        private void RemoveShortcuts()
        {
            try
            {
                string shortcutName = "PolyLauncher.lnk";
                string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), shortcutName);
                string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), shortcutName);

                if (File.Exists(desktopPath)) File.Delete(desktopPath);
                if (File.Exists(startMenuPath)) File.Delete(startMenuPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to remove shortcuts: {ex.Message}");
            }
        }

        private void CreateShortcut(string shortcutPath, string targetExePath)
        {
            try
            {
                // Escape single quotes for PowerShell
                string shortcutPathEsc = shortcutPath.Replace("'", "''");
                string targetExePathEsc = targetExePath.Replace("'", "''");
                string workingDirEsc = (Path.GetDirectoryName(targetExePath) ?? "").Replace("'", "''");
                string iconPathEsc = Path.Combine(workingDirEsc, "polylauncher.png");

                // PowerShell to create shortcut
                string command = $"$s = (New-Object -ComObject WScript.Shell).CreateShortcut('{shortcutPathEsc}'); $s.TargetPath = '{targetExePathEsc}'; $s.WorkingDirectory = '{workingDirEsc}'; $s.IconLocation = '{iconPathEsc}'; $s.Save();";
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -WindowStyle Hidden -Command \"{command}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                })?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create shortcut: {ex.Message}");
            }
        }
    }
}
