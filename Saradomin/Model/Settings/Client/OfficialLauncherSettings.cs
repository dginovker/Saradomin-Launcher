using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings.Client
{
    public class OfficialLauncherSettings : SettingsComponent
    {
        [JsonPropertyName("closeOnClientLaunch")]
        public bool CloseOnClientLaunch { get; set; } = true;

        [JsonPropertyName("notifyUpdates")]
        public bool CheckForUpdates { get; set; } = true;
    }
}