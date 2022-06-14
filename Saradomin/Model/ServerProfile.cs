using System.Text.Json.Serialization;

namespace Saradomin.Model
{
    public class ServerProfile 
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("ip_management")]
        public string ManagementServerAddress { get; set; }
        
        [JsonPropertyName("ip_address")]
        public string GameServerAddress { get; set; }
        
        [JsonPropertyName("server_port")]
        public ushort GameServerPort { get; set; }
        
        [JsonPropertyName("wl_port")]
        public ushort WorldListServerPort { get; set; }
        
        [JsonPropertyName("js5_port")]
        public ushort CacheServerPort { get; set; }
    }
}