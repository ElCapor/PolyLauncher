using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace PolyLauncher.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        private readonly SettingsService _settingsService;
        private const string ApiUrl = "https://api.polytoria.com/v1/launcher/updates";

        public UpdateService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Gets the official Polytoria directory (Client or Creator)
        /// </summary>
        public string GetPolytoriaDirectory(string type)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Polytoria", type);
        }

        public string GetPolytoriaClientDirectory() => GetPolytoriaDirectory("Client");
        public string GetPolytoriaCreatorDirectory() => GetPolytoriaDirectory("Creator");

        /// <summary>
        /// Detects the currently installed version for a given type and release.
        /// </summary>
        public string? GetInstalledVersion(string type, string release)
        {
            // First check our manifest
            var settings = _settingsService.LoadSettings();
            var manifest = type.ToLower() == "creator" ? settings.CreatorManifest : settings.ClientManifest;
            var manifestVersion = manifest
                .FirstOrDefault(v => v.Release == release)?.Version;

            if (!string.IsNullOrEmpty(manifestVersion))
            {
                var versionPath = Path.Combine(GetPolytoriaDirectory(type), manifestVersion);
                if (Directory.Exists(versionPath))
                    return manifestVersion;
            }

            // Fallback: scan the directory for version folders
            var baseDir = GetPolytoriaDirectory(type);
            if (!Directory.Exists(baseDir))
                return null;

            var versionDirs = Directory.GetDirectories(baseDir)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name) && IsVersionString(name!))
                .OrderByDescending(v => v)
                .FirstOrDefault();

            return versionDirs;
        }

        public string? GetInstalledClientVersion(string release) => GetInstalledVersion("Client", release);
        public string? GetInstalledCreatorVersion(string release) => GetInstalledVersion("Creator", release);

        /// <summary>
        /// Checks if a string looks like a version number (e.g., "1.4.155")
        /// </summary>
        private static bool IsVersionString(string name)
        {
            return name.Split('.').All(part => int.TryParse(part, out _));
        }

        /// <summary>
        /// Gets the User-Agent string using the installed client version
        /// </summary>
        public string GetUserAgent()
        {
            var version = GetInstalledClientVersion("Stable") ?? GetInstalledClientVersion("Beta") ?? "1.4.155";
            return $"PolytoriaLauncher/{version}";
        }

        /// <summary>
        /// Checks for updates from the Polytoria API
        /// </summary>
        public async Task<Models.UpdateResponse?> CheckForUpdatesAsync(string release, string? token = null)
        {
            Logger.Log($"Checking for updates. Release: {release}");
            try
            {
                var url = $"{ApiUrl}?os=windows&release={release.ToLower()}";
                
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", GetUserAgent());
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", token);
                }

                var response = await _httpClient.SendAsync(request);
                Logger.Log($"Update check API response: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Models.UpdateResponse>(json);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to check for updates", ex);
                return null;
            }
        }

        /// <summary>
        /// Checks if an update is needed by comparing installed version with API version
        /// </summary>
        public async Task<(bool NeedsUpdate, string? NewVersion, string? DownloadUrl)> CheckUpdateNeededAsync(
            string type,
            string release, 
            string? token = null)
        {
            var installedVersion = GetInstalledVersion(type, release);
            Logger.Log($"Currently installed version for {type} ({release}): {installedVersion ?? "None"}");
            var updateResponse = await CheckForUpdatesAsync(release, token);

            if (updateResponse == null)
                return (false, null, null);

            if (updateResponse.Maintenance)
                return (false, null, null);

            var info = type.ToLower() == "creator" ? updateResponse.Creator : updateResponse.Client;
            if (info == null || string.IsNullOrEmpty(info.Version) || string.IsNullOrEmpty(info.Download))
            {
                Logger.Log($"No {type} update information found in API response.");
                return (false, null, null);
            }

            Logger.Log($"Latest version available: {info.Version}");

            // If no version installed, or version differs, need update
            if (string.IsNullOrEmpty(installedVersion) || installedVersion != info.Version)
            {
                Logger.Log($"Update needed: {installedVersion ?? "None"} -> {info.Version}");
                return (true, info.Version, info.Download);
            }

            Logger.Log($"{type} is up to date.");
            return (false, installedVersion, null);
        }

        public async Task<(bool NeedsUpdate, string? NewVersion, string? DownloadUrl)> CheckClientUpdateNeededAsync(string release, string? token = null)
            => await CheckUpdateNeededAsync("Client", release, token);

        public async Task<(bool NeedsUpdate, string? NewVersion, string? DownloadUrl)> CheckCreatorUpdateNeededAsync(string release, string? token = null)
            => await CheckUpdateNeededAsync("Creator", release, token);

        /// <summary>
        /// Downloads and extracts the update for a given type
        /// </summary>
        public async Task<bool> DownloadAndExtractAsync(
            string type,
            string downloadUrl, 
            string version, 
            string release,
            string? oldVersion = null,
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Logger.Log($"Downloading {type} update: {version} ({release})");
            try
            {
                var baseDir = GetPolytoriaDirectory(type);
                Directory.CreateDirectory(baseDir);

                // Remove old version directory if exists and different from new version
                if (!string.IsNullOrEmpty(oldVersion) && oldVersion != version)
                {
                    var oldVersionDir = Path.Combine(baseDir, oldVersion);
                    if (Directory.Exists(oldVersionDir))
                    {
                        Logger.Log($"Removing old version directory: {oldVersionDir}");
                        try { Directory.Delete(oldVersionDir, true); } catch { }
                    }
                }

                var versionDir = Path.Combine(baseDir, version);
                
                // Remove existing version directory if it exists
                if (Directory.Exists(versionDir))
                {
                    try { Directory.Delete(versionDir, true); } catch { }
                }
                
                Directory.CreateDirectory(versionDir);

                var tempDir = _settingsService.GetTempDirectory();
                Directory.CreateDirectory(tempDir);
                
                var archiveExtension = new Uri(downloadUrl).AbsolutePath.EndsWith(".7z") ? ".7z" : ".zip";
                var archivePath = Path.Combine(tempDir, $"{type}{archiveExtension}");

                // Remove existing archive if it exists
                if (File.Exists(archivePath))
                    File.Delete(archivePath);

                // Download the archive
                Logger.Log($"Downloading from: {downloadUrl}");
                using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                request.Headers.Add("User-Agent", GetUserAgent());

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                Logger.Log($"Download size: {totalBytes} bytes");
                var downloadedBytes = 0L;

                using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fileStream = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        downloadedBytes += bytesRead;
                        
                        if (totalBytes > 0)
                        {
                            var percentage = (int)((downloadedBytes * 100) / totalBytes);
                            progress?.Report(percentage);
                        }
                    }
                }

                // Extract the archive
                Logger.Log($"Extracting archive to: {versionDir}");
                if (archiveExtension == ".7z")
                {
                    // Use 7zip for .7z files - would need SevenZipExtractor or similar
                    // For now, throw an exception indicating 7z support needed
                    throw new NotSupportedException("7z extraction requires additional library. Please install SharpCompress or use the official launcher.");
                }
                else
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, versionDir);
                }
                
                Logger.Log("Extraction complete.");

                // Cleanup archive
                if (File.Exists(archivePath))
                    File.Delete(archivePath);

                // Update manifest
                Logger.Log("Updating settings manifest.");
                _settingsService.UpdateSettings(settings =>
                {
                    var manifest = type.ToLower() == "creator" ? settings.CreatorManifest : settings.ClientManifest;
                    manifest.RemoveAll(v => v.Release == release);
                    manifest.Add(new Models.VersionManifest
                    {
                        Release = release,
                        Version = version,
                        InstalledAt = DateTime.UtcNow
                    });
                });

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to download or extract {type} update", ex);
                return false;
            }
        }

        public async Task<bool> DownloadAndExtractClientAsync(
            string downloadUrl, 
            string version, 
            string release,
            string? oldVersion = null,
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default)
            => await DownloadAndExtractAsync("Client", downloadUrl, version, release, oldVersion, progress, cancellationToken);

        public async Task<bool> DownloadAndExtractCreatorAsync(
            string downloadUrl, 
            string version, 
            string release,
            string? oldVersion = null,
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default)
            => await DownloadAndExtractAsync("Creator", downloadUrl, version, release, oldVersion, progress, cancellationToken);

        /// <summary>
        /// Gets the path to the executable for a specific type and version
        /// </summary>
        public string? GetExecutablePath(string type, string version)
        {
            var baseDir = GetPolytoriaDirectory(type);
            var versionDir = Path.Combine(baseDir, version);
            var exeName = type.ToLower() == "creator" ? "Polytoria Creator.exe" : "Polytoria Client.exe";
            var exePath = Path.Combine(versionDir, exeName);
            
            bool exists = File.Exists(exePath);
            if (!exists) Logger.Log($"{type} executable not found at: {exePath}", "WARNING");
            
            return exists ? exePath : null;
        }

        public string? GetClientExecutablePath(string version) => GetExecutablePath("Client", version);
        public string? GetCreatorExecutablePath(string version) => GetExecutablePath("Creator", version);

        /// <summary>
        /// Launches the Polytoria application (Client or Creator)
        /// </summary>
        public Process? Launch(string type, string version, Models.LaunchArguments args)
        {
            Logger.Log($"Attempting to launch {type} version {version}");
            var exePath = GetExecutablePath(type, version);
            if (exePath == null)
            {
                Logger.LogError($"Cannot launch {type}: Executable not found.");
                return null;
            }

            var settings = _settingsService.LoadSettings();

            // Kill any existing process first if multi-client is disabled (for Client)
            // Creator might not need this but let's keep it consistent if desired
            var procName = type.ToLower() == "creator" ? "Polytoria Creator" : "Polytoria Client";
            if (!settings.AllowMulticlient || type.ToLower() == "creator")
            {
                try
                {
                    var existingProcesses = Process.GetProcessesByName(procName);
                    foreach (var proc in existingProcesses)
                    {
                        try 
                        { 
                            Logger.Log($"Killing existing {type} process (PID: {proc.Id})");
                            proc.Kill(); 
                            proc.WaitForExit(1000); 
                        } catch { }
                    }
                }
                catch { }
            }

            // Build arguments based on launch type
            string arguments;
            if (type.ToLower() == "creator")
            {
                arguments = $"-asset {args.Map} -token {args.Token}";
            }
            else if (args.IsTest)
            {
                arguments = $"-solo {args.Map}";
            }
            else
            {
                arguments = $"-network client -token {args.Token}";
            }

            Logger.Log($"Launching with arguments: {arguments}");

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            };

            var procStart = Process.Start(startInfo);
            if (procStart != null)
            {
                Logger.Log($"{type} launched successfully. PID: {procStart.Id}");
            }
            else
            {
                Logger.LogError("Process.Start returned null.");
            }

            return procStart;
        }

        public Process? LaunchClient(string version, Models.LaunchArguments args) => Launch("Client", version, args);
        public Process? LaunchCreator(string version, Models.LaunchArguments args) => Launch("Creator", version, args);

        /// <summary>
        /// Performs the full update check and launch flow
        /// </summary>
        public async Task<(bool Success, string? Error, string? Version)> CheckUpdateAndPrepareAsync(
            Models.LaunchArguments args,
            IProgress<int>? progress = null,
            IProgress<string>? status = null,
            CancellationToken cancellationToken = default)
        {
            var release = args.Release;
            var token = args.Token;

            status?.Report("Checking for updates...");

            var type = args.IsCreator ? "Creator" : "Client";
            var (needsUpdate, newVersion, downloadUrl) = await CheckUpdateNeededAsync(type, release, token);

            if (needsUpdate && !string.IsNullOrEmpty(downloadUrl) && !string.IsNullOrEmpty(newVersion))
            {
                var oldVersion = GetInstalledVersion(type, release);
                
                status?.Report($"Downloading {type} update ({newVersion})...");
                
                var success = await DownloadAndExtractAsync(
                    type,
                    downloadUrl, 
                    newVersion, 
                    release,
                    oldVersion,
                    progress, 
                    cancellationToken);

                if (!success)
                    return (false, $"Failed to download and extract {type} update", null);

                return (true, null, newVersion);
            }

            var installedVersion = GetInstalledVersion(type, release);
            if (string.IsNullOrEmpty(installedVersion))
                return (false, $"No {type} version installed and no update available", null);

            var exePath = GetExecutablePath(type, installedVersion);
            if (exePath == null)
                return (false, $"{type} executable not found", null);

            return (true, null, installedVersion);
        }
    }
}
