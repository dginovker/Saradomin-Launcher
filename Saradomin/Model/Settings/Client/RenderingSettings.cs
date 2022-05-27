
using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings
{
    public class RenderingSettings : SettingsComponent
    {
        public class SkyBoxSettings : SettingsComponent
        {
            [JsonPropertyName("skybox_color")]
            public string SkyBoxColor { get; set; } = "Not supported yet.";
        }

        public class TechnicalSettings : SettingsComponent
        {
            [JsonPropertyName("render_distance_increase")]
            public bool IncreaseRenderDistance { get; set; } = true;
        }

        [JsonPropertyName("skybox")]
        public SkyBoxSettings SkyBox { get; set; } = new();

        [JsonPropertyName("technical")]
        public TechnicalSettings Technical { get; set; } = new();
    }
}