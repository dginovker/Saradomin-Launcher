using System.Text.Json.Serialization;
using Avalonia.Media;

namespace Saradomin.Model.Settings
{
    public class SlayerTrackerSettings : SettingsComponent
    {
        [JsonPropertyName("color")]
        public string ColorString { get; set; } = "#635A38";

        [JsonPropertyName("opacity")]
        public string OpacityString { get; set; } = "180";

        [JsonPropertyName("enabled")]
        public bool IsEnabled { get; set; } = true;

        [JsonIgnore]
        public SolidColorBrush ColorBrush
        {
            get => SolidColorBrush.Parse(ColorString);
            set
            {
                var color = value.Color;

                ColorString = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                Opacity = color.A;
            }
        }

        [JsonIgnore]
        public byte Opacity
        {
            get => byte.Parse(OpacityString);
            set => OpacityString = value.ToString();
        }
    }
}