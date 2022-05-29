using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings
{
    public class SlayerTrackerSettings : SettingsComponent
    {
        private MenuColor _backgroundColor = null;
        
        [JsonPropertyName("color")]
        public string ColorString { get; set; } = "#635A38";

        [JsonPropertyName("opacity")]
        public string OpacityString { get; set; } = "180";

        [JsonPropertyName("enabled")]
        public bool IsEnabled { get; set; } = true;

        [JsonIgnore]
        public MenuColor BackgroundColor
        {
            get
            {
                if (_backgroundColor == null)
                {
                    _backgroundColor = new MenuColor(ColorString);
                    _backgroundColor.A = Opacity;

                    _backgroundColor.PropertyChanged += RaiseBackgroundColorWrite;
                }

                return _backgroundColor;
            }

            set
            {
                if (value == null)
                    return;

                if (_backgroundColor != null)
                    _backgroundColor.PropertyChanged -= RaiseBackgroundColorWrite;

                _backgroundColor = value;
                Opacity = _backgroundColor.A;
                
                _backgroundColor.PropertyChanged += RaiseBackgroundColorWrite;

                UpdateBackgroundColorStrings(value);
            }
        }

        [JsonIgnore]
        public byte Opacity
        {
            get => byte.Parse(OpacityString);
            set
            {
                OpacityString = value.ToString();
                RaiseSettingsModified(this, new(nameof(Opacity)));
            }
        }

        public void SetDefaults()
        {
            BackgroundColor = new("#635A38", 180);
        }

        private void RaiseBackgroundColorWrite(object sender, PropertyChangedEventArgs e)
        {
            UpdateBackgroundColorStrings(BackgroundColor);
            RaiseSettingsModified(this, new(nameof(BackgroundColor)));
        }
        
        private void UpdateBackgroundColorStrings(MenuColor value)
        {
            ColorString = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
            Opacity = value.A;
        }
    }
}