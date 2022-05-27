using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings.Client
{
    public class XpTrackerSettings : SettingsComponent
    {
        public enum DropModeSetting
        {
            Instant,
            Incremental
        }

        public enum TrackingModeSetting
        {
            TotalXP,
            RecentSkill
        }

        [JsonPropertyName("drop_mode")]
        public DropModeSetting DropMode { get; set; }

        [JsonPropertyName("track_mode")]
        public TrackingModeSetting TrackingMode { get; set; }

        [JsonPropertyName("enabled")]
        public bool IsEnabled { get; set; }
    }
}