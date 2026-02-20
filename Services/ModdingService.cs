using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Linq;

namespace PolyLauncher.Services
{
    /// <summary>
    /// Service for managing modding features like HWID spoofing and executor auto-start.
    /// </summary>
    public class ModdingService
    {
        private readonly SettingsService _settingsService;
        private readonly HttpClient _httpClient;
        private const string HwidSpooferUrl = "https://github.com/ElCapor/polytoria-executor/raw/master/.download/version.dll";
        private const string ExecutorUrl = "https://github.com/ElCapor/polytoria-executor/raw/master/.download/wowiezz.dll";

        public ModdingService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PolyLauncher/1.0");
        }

        private string GetModCacheDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var cacheDir = Path.Combine(appData, "PolyLauncher", "ModCache");
            Directory.CreateDirectory(cacheDir);
            return cacheDir;
        }

        private async Task<string?> EnsureFileDownloadedAsync(string url, string fileName, bool forceRedownload = false)
        {
            var cacheDir = GetModCacheDirectory();
            var filePath = Path.Combine(cacheDir, fileName);
            
            if (!forceRedownload && File.Exists(filePath))
            {
                return filePath;
            }

            Logger.Log($"Downloading mod file: {fileName} from {url}");

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, data);
                Logger.Log($"Successfully downloaded {fileName}");
                return filePath;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to download {fileName}", ex);
                return null;
            }
        }

        private bool IsClientRunning()
        {
            try
            {
                return Process.GetProcessesByName("Polytoria Client").Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Prepares mod files (DLLs) in the client directory based on settings.
        /// </summary>
        public async Task PrepareModsAsync(string version)
        {
            Logger.Log($"Preparing mods for version {version}...");

            if (IsClientRunning())
            {
                Logger.Log("Polytoria Client is already running. Skipping mod file replacement to avoid conflicts.");
                return;
            }

            var settings = _settingsService.LoadSettings();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var clientVersionDir = Path.Combine(appData, "Polytoria", "Client", version);

            if (!Directory.Exists(clientVersionDir))
            {
                 Logger.Log($"Client version directory not found for mod preparation: {clientVersionDir}", "WARNING");
                 return;
            }

            // HWID Spoofer (version.dll)
            var versionDllPath = Path.Combine(clientVersionDir, "version.dll");
            try
            {
                if (settings.EnableHwidSpoofer)
                {
                    Logger.Log("HWID Spoofer is enabled.");
                    var downloadedPath = await EnsureFileDownloadedAsync(HwidSpooferUrl, "version.dll", settings.ForceRedownloadMods);
                    if (downloadedPath != null)
                    {
                        File.Copy(downloadedPath, versionDllPath, true);
                        Logger.Log($"HWID Spoofer (version.dll) applied to client directory: {versionDllPath}");
                    }
                }
                else if (File.Exists(versionDllPath))
                {
                    File.Delete(versionDllPath);
                    Logger.Log($"HWID Spoofer (version.dll) removed from client directory: {versionDllPath}");
                }
            }
            catch (IOException ex)
            {
                Logger.Log($"Could not update version.dll: {ex.Message}. Skipping.");
            }

            // Executor Auto-start (wowiezz.dll)
            var wowiezzDllPath = Path.Combine(clientVersionDir, "wowiezz.dll");
            try
            {
                if (settings.AutoStartExecutor)
                {
                    Logger.Log("Executor Auto-start is enabled.");
                    var downloadedPath = await EnsureFileDownloadedAsync(ExecutorUrl, "wowiezz.dll", settings.ForceRedownloadMods);
                    if (downloadedPath != null)
                    {
                        File.Copy(downloadedPath, wowiezzDllPath, true);
                        Logger.Log($"Executor (wowiezz.dll) applied to client directory: {wowiezzDllPath}");
                    }
                }
                else if (File.Exists(wowiezzDllPath))
                {
                    File.Delete(wowiezzDllPath);
                    Logger.Log($"Executor (wowiezz.dll) removed from client directory: {wowiezzDllPath}");
                }
            }
            catch (IOException ex)
            {
                Logger.Log($"Could not update wowiezz.dll: {ex.Message}. Skipping.");
            }
            Logger.Log("Mod preparation complete.");
        }

        /// <summary>
        /// Monitors the game process and applies modding features when appropriate.
        /// </summary>
        public async Task MonitorGameProcessAsync(string version, Process gameProcess, CancellationToken cancellationToken = default)
        {
            Logger.Log($"Monitoring game process (PID: {gameProcess.Id}) for version {version}...");
            try
            {
                // Wait for process to exit
                while (!gameProcess.HasExited && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Game monitoring was cancelled.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error monitoring game process", ex);
            }
            finally
            {
                Logger.Log("Game process exited or monitoring stopped. Performing cleanup...");
                Cleanup(version);
            }
        }

        /// <summary>
        /// Cleans up any modding-related resources or modifications.
        /// </summary>
        public void Cleanup(string version)
        {
            Logger.Log($"Cleanup mods for version {version}...");
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var clientVersionDir = Path.Combine(appData, "Polytoria", "Client", version);

            if (!Directory.Exists(clientVersionDir))
                return;

            try
            {
                var versionDllPath = Path.Combine(clientVersionDir, "version.dll");
                if (File.Exists(versionDllPath))
                {
                    File.Delete(versionDllPath);
                    Logger.Log("Deleted version.dll");
                }
            }
            catch (IOException ex)
            {
                Logger.Log($"Could not delete version.dll during cleanup: {ex.Message}");
            }

            try
            {
                var wowiezzDllPath = Path.Combine(clientVersionDir, "wowiezz.dll");
                if (File.Exists(wowiezzDllPath))
                {
                    File.Delete(wowiezzDllPath);
                    Logger.Log("Deleted wowiezz.dll");
                }
            }
            catch (IOException ex)
            {
                Logger.Log($"Could not delete wowiezz.dll during cleanup: {ex.Message}");
            }
        }
    }
}
