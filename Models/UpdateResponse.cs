using Newtonsoft.Json;

namespace PolyLauncher.Models
{
    public class UpdateResponse
    {
        [JsonProperty("maintenance")]
        public bool Maintenance { get; set; }

        [JsonProperty("client")]
        public UpdateInfo? Client { get; set; }

        [JsonProperty("creator")]
        public UpdateInfo? Creator { get; set; }

        [JsonProperty("launcher")]
        public UpdateInfo? Launcher { get; set; }
    }

    public class UpdateInfo
    {
        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("download")]
        public string? Download { get; set; }

        [JsonProperty("release")]
        public string Release { get; set; } = "Stable";
    }
}
