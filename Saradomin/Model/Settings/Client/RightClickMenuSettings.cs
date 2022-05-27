using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings
{
    public class RightClickMenuSettings : SettingsComponent
    {
        public class StylesSettings : SettingsComponent
        {
            [JsonPropertyName("presets")]
            public string Presets { get; set; } = "custom";

            [JsonPropertyName("rs3border")]
            public bool UseRuneScape3Border { get; set; } = false;

            [JsonPropertyName("Presets provide default customizations.")]
            public string DummyThingWhatever { get; set; } =
                "rs3, classic, or custom. custom allows you to define your own values above. Classic is standard 2009.";
        }
        
        [JsonPropertyName("left_click_attack")]
        public bool AttackWithLeftClick { get; set; } = false;
        
        [JsonPropertyName("border")]
        public ColorSettings Border { get; set; } = new("#FFFFFF");

        [JsonPropertyName("background")]
        public ColorSettings Background { get; set; } = new("#5D5447");

        [JsonPropertyName("title_bar")]
        public ColorSettings TitleBar { get; set; } = new("#000000", "#FFFFFF", 255);

        [JsonPropertyName("styles")]
        public StylesSettings Styles { get; set; } = new();
    }
}