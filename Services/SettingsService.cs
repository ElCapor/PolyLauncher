using Newtonsoft.Json;
using System.IO;

namespace PolyLauncher.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private Models.LauncherSettings? _settings;

        public static event EventHandler? SettingsChanged;

        public SettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var launcherDir = Path.Combine(appData, "PolyLauncher");
            Directory.CreateDirectory(launcherDir);
            _settingsPath = Path.Combine(launcherDir, "Settings.json");
            
            // Listen for changes from other instances to clear local cache
            SettingsChanged += (s, e) => _settings = null;
        }

        public Models.LauncherSettings LoadSettings()
        {
            if (_settings != null)
                return _settings;

            if (File.Exists(_settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonConvert.DeserializeObject<Models.LauncherSettings>(json);
                    if (_settings != null)
                        return _settings;
                }
                catch
                {
                    // If settings are corrupted, create new ones
                }
            }

            _settings = new Models.LauncherSettings();
            SaveSettings();
            return _settings;
        }

        public void SaveSettings()
        {
            if (_settings == null)
                return;

            var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
            File.WriteAllText(_settingsPath, json);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateSettings(Action<Models.LauncherSettings> updateAction)
        {
            var settings = LoadSettings();
            updateAction(settings);
            SaveSettings();
        }

        public void ResetSettings()
        {
            _settings = new Models.LauncherSettings();
            SaveSettings();
        }

        public string GetClientDirectory()
        {
            // Use official Polytoria client directory
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var clientDir = Path.Combine(appData, "Polytoria", "Client");
            Directory.CreateDirectory(clientDir);
            return clientDir;
        }

        public string GetTempDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "PolyLauncher");
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }
    }
}
