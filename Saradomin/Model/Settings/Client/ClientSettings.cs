using System;
using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings.Client
{
    public class ClientSettings : SettingsBase
    {
        public const string FileName = "config.json";

        [JsonPropertyName("ip_management")]
        public string IpManagement { get; set; } = "play.2009scape.org";

        [JsonPropertyName("ip_address")]
        public string ServerAddress { get; set; } = "play.2009scape.org";
        
        [JsonPropertyName("world")]
        public ushort World { get; set; } = 1;
        
        [JsonPropertyName("server_port")]
        public ushort GameServerPort { get; set; } = 43594;
        
        [JsonPropertyName("wl_port")]
        public ushort WorldListServerPort { get; set; } = 5555;

        [JsonPropertyName("js5_port")]
        public ushort CacheServerPort { get; set; } = 43595;

        [JsonPropertyName("debug")]
        public DebuggingSettings Debugging { get; set; } = new();

        [JsonPropertyName("customization")]
        public CustomizationSettings Customization { get; set; } = new();

        [Obsolete("Here for compatibility. Use LauncherSettings instead.")]
        [JsonPropertyName("launcher")]
        public OfficialLauncherSettings OfficialLauncher { get; set; } = new();
    }
}