using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings
{
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    public class ColorSettings : SettingsComponent
    {
        private MenuColor _backgroundColor = null;
        private MenuColor _foregroundColor = null;

        [JsonPropertyName("color")]
        public string BackgroundColorString { get; set; } = "#069420";

        [JsonPropertyName("font_color")]
        public string ForegroundColorString { get; set; } = null;

        [JsonPropertyName("opacity")]
        public string OpacityString { get; set; }

        [JsonIgnore]
        public MenuColor BackgroundColor
        {
            get
            {
                if (_backgroundColor == null)
                {
                    _backgroundColor = new MenuColor(BackgroundColorString);
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
                _backgroundColor.PropertyChanged += RaiseBackgroundColorWrite;

                UpdateBackgroundColorStrings(value);
            }
        }

        [JsonIgnore]
        public MenuColor ForegroundColor
        {
            get
            {
                if (_foregroundColor == null)
                {
                    _foregroundColor = new MenuColor(ForegroundColorString);
                    _foregroundColor.PropertyChanged += RaiseForegroundColorWrite;
                }

                return _foregroundColor;
            }
            
            set
            {
                if (value == null)
                    return;

                if (_foregroundColor != null)
                    _foregroundColor.PropertyChanged -= RaiseForegroundColorWrite;

                _foregroundColor = value;
                _foregroundColor.PropertyChanged += RaiseForegroundColorWrite;

                UpdateForegroundColorString(value);
            }
        }

        [JsonIgnore]
        public byte Opacity
        {
            get => byte.Parse(OpacityString ?? "0");
            set => OpacityString = value.ToString();
        }

        public ColorSettings()
        {
        }

        public ColorSettings(string defaultBackgroundColor)
            : this(defaultBackgroundColor, null, 255)
        {
        }

        public ColorSettings(string defaultBackgroundColor, byte defaultOpacity)
            : this(defaultBackgroundColor, null, defaultOpacity)
        {
        }

        public ColorSettings(string defaultBackgroundColor, string defaultForegroundColor, byte defaultOpacity)
        {
            BackgroundColorString = defaultBackgroundColor;
            ForegroundColorString = defaultForegroundColor;

            BackgroundColor = new MenuColor(defaultBackgroundColor);

            if (!string.IsNullOrEmpty(defaultForegroundColor))
                ForegroundColor = new MenuColor(defaultForegroundColor);

            Opacity = defaultOpacity;
        }

        protected void RaiseBackgroundColorWrite(object sender, PropertyChangedEventArgs e)
        {
            UpdateBackgroundColorStrings(BackgroundColor);
            RaiseSettingsModified(this, new(nameof(BackgroundColor)));
        }

        protected void RaiseForegroundColorWrite(object sender, PropertyChangedEventArgs e)
        {
            UpdateForegroundColorString(ForegroundColor);
            RaiseSettingsModified(this, new(nameof(ForegroundColor)));
        }

        private void UpdateForegroundColorString(MenuColor value)
        {
            ForegroundColorString = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        }
        
        private void UpdateBackgroundColorStrings(MenuColor value)
        {
            BackgroundColorString = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
            Opacity = value.A;
        }
    }
}