using System.Text.Json.Serialization;

namespace Saradomin.Model.Settings
{
    public class DebuggingSettings : SettingsComponent
    {
        [JsonPropertyName("hd_login_region_debug_verbose")]
        public bool HdLoginRegionVerboseDebuggingEnabled { get; set; }

        [JsonPropertyName("hd_login_region_debug")]
        public bool HdLoginRegionDebuggingEnabled { get; set; }

        [JsonPropertyName("npc_debug")]
        public bool NpcDebuggingEnabled { get; set; }

        [JsonPropertyName("world_map_debug")]
        public bool WorldMapDebuggingEnabled { get; set; }

        [JsonPropertyName("item_debug")]
        public bool ItemDebuggingEnabled { get; set; }

        [JsonPropertyName("object_debug")]
        public bool ObjectDebuggingEnabled { get; set; }

        [JsonPropertyName("cache_debug")]
        public bool CacheDebuggingEnabled { get; set; }
    }
}