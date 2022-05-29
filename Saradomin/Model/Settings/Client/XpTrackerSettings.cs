using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings.Client
{
    public class XpTrackerSettings : SettingsComponent
    {
        public enum DropModeSetting
        {
            [Description("Separate skill XP;XP will increase as the icons rise to the top.")]
            Incremental,
            
            [Description("Combined skill XP;XP will increase instantly, regardless of the icons' position.")]
            Instant
        }

        public enum TrackingModeSetting
        {
            [Description("Total experience gained")]
            TotalXP,
            
            [Description("Most recent skill trained")]
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