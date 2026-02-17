using Newtonsoft.Json;

namespace PolyLauncher.Models
{
    public class LauncherSettings
    {
        [JsonProperty("enableHwidSpoofer")]
        public bool EnableHwidSpoofer { get; set; } = false;

        [JsonProperty("autoStartExecutor")]
        public bool AutoStartExecutor { get; set; } = false;

        [JsonProperty("forceRedownloadMods")]
        public bool ForceRedownloadMods { get; set; } = false;

        [JsonProperty("allowMulticlient")]
        public bool AllowMulticlient { get; set; } = true;

        [JsonProperty("customLaunchDuration")]
        public int CustomLaunchDuration { get; set; } = 5;

        [JsonProperty("customLoadingIcon")]
        public string? CustomLoadingIcon { get; set; }

        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; } = "#1E1E1E";

        [JsonProperty("loadingBarColor")]
        public string LoadingBarColor { get; set; } = "#007ACC";

        [JsonProperty("loadingText")]
        public string LoadingText { get; set; } = "Loading Polytoria";

        [JsonProperty("textColor")]
        public string TextColor { get; set; } = "#FFFFFF";

        [JsonProperty("clientManifest")]
        public List<VersionManifest> ClientManifest { get; set; } = new();

        [JsonProperty("creatorManifest")]
        public List<VersionManifest> CreatorManifest { get; set; } = new();

        [JsonProperty("skipUpdates")]
        public bool SkipUpdates { get; set; } = false;

        [JsonProperty("firstRun")]
        public bool FirstRun { get; set; } = true;

        [JsonProperty("installPath")]
        public string? InstallPath { get; set; }
    }

    public class VersionManifest
    {
        [JsonProperty("release")]
        public string Release { get; set; } = "Stable";

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("installedAt")]
        public DateTime InstalledAt { get; set; }
    }
}
