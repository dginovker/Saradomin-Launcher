using System.Text.Json.Serialization;
using Saradomin.Model.Settings.Client;

namespace Saradomin.Model.Settings
{
    public class CustomizationSettings : SettingsComponent
    {
        [JsonPropertyName("aa_samples")]
        public byte AntiAliasingSampleCount { get; set; } = 0;

        [JsonPropertyName("login_theme")]
        public string LoginMusicTheme { get; set; } = "scape main";

        [JsonPropertyName("december_snow")]
        public bool SnowSeasonFeaturesEnabled { get; set; } = true;

        [JsonPropertyName("minimap_filter")]
        public bool MiniMapSmoothingEnabled { get; set; } = true;

        [JsonPropertyName("slayer")]
        public SlayerTrackerSettings SlayerTracker { get; set; } = new();

        [JsonPropertyName("rendering_options")]
        public RenderingSettings Rendering { get; set; } = new();

        [JsonPropertyName("right_click_menu")]
        public RightClickMenuSettings RightClickMenu { get; set; } = new();

        [JsonPropertyName("xpdrops")]
        public XpTrackerSettings XpTracker { get; set; } = new();

    }
}