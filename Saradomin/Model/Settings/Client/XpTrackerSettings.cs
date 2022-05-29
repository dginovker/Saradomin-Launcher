using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings.Client
{
    public class XpTrackerSettings : SettingsComponent
    {
        public enum DropModeSetting
        {
            [Description("Animated;XP will increase as soon as the respective icons reach the XP tracker box.")]
            Incremental,
            
            [Description("Immediate;XP text will increase instantly, as you get it.")]
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