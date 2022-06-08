using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Saradomin.Model;

namespace Saradomin.Services
{
    public class RemoteConfigService : IRemoteConfigService
    {
        private const string ServerProfilesURL =
            "https://gitlab.com/2009scape/tools/remote-config/-/raw/main/server_profiles.json";

        public ObservableCollection<ServerProfile> AvailableProfiles { get; private set; } = new();

        public async Task FetchServerProfileConfig(string outputPath)
        {
            using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
            {
                var configJson = await httpClient.GetStringAsync(ServerProfilesURL);

                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
                else
                {
                    await File.WriteAllTextAsync(outputPath!, configJson);
                }
            }
        }

        public async Task LoadServerProfileConfig(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                AvailableProfiles = JsonSerializer.Deserialize<ObservableCollection<ServerProfile>>(
                    await sr.ReadToEndAsync()
                );
            }
        }

        public void LoadFailsafeDefaults()
        {
            AvailableProfiles = new ObservableCollection<ServerProfile>
            {
                new()
                {
                    Name = "Local server",
                    ManagementServerAddress = "localhost",
                    GameServerAddress = "localhost",
                    GameServerPort = 43594,
                    WorldListServerPort = 43595,
                    CacheServerPort = 43595
                },

                new()
                {
                    Name = "Live server",
                    ManagementServerAddress = "play.2009scape.org",
                    GameServerAddress = "play.2009scape.org",
                    GameServerPort = 43594,
                    WorldListServerPort = 43595,
                    CacheServerPort = 43595
                },
                
                new()
                {
                    Name = "Testing server",
                    ManagementServerAddress = "test.2009scape.org",
                    GameServerAddress = "test.2009scape.org",
                    GameServerPort = 43594,
                    WorldListServerPort = 43595,
                    CacheServerPort = 43595
                },
            };
        }
    }
}