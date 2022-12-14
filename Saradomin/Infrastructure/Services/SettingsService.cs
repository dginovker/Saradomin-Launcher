using System.IO;
using System.Text.Json;
using Saradomin.Model.Settings.Client;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Utilities;

namespace Saradomin.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        public LauncherSettings Launcher { get; private set; } = new();
        public ClientSettings Client { get; private set; } = new();

        private string ConfigDirectory { get; } = CrossPlatform.Locate2009scapeHome();

        private string ClientSettingsPath
            => Path.Combine(ConfigDirectory, ClientSettings.FileName);
        
        private string LauncherSettingsPath
            => Path.Combine(ConfigDirectory, LauncherSettings.FileName);

        public SettingsService()
        {
            TryReadConfigurationData();
        }

        public void SaveAll()
        {
            SaveLauncherSettings();
            SaveClientSettings();
        }

        private void TryReadConfigurationData()
        {
            if (!Directory.Exists(CrossPlatform.Locate2009scapeHome()))
                Directory.CreateDirectory(CrossPlatform.Locate2009scapeHome());
            
            if (File.Exists(LauncherSettingsPath))
            {
                using (var stream = File.OpenRead(LauncherSettingsPath))
                {
                    Launcher = JsonSerializer.Deserialize<LauncherSettings>(stream);
                }
            }
            else
            {
                SaveLauncherSettings();
            }

            if (File.Exists(ClientSettingsPath))
            {
                using (var stream = File.OpenRead(ClientSettingsPath))
                {
                    Client = JsonSerializer.Deserialize<ClientSettings>(stream);
                }
            }
            else
            {
                SaveClientSettings();
            }
        }

        private void SaveLauncherSettings()
        {
            File.WriteAllText(
                LauncherSettingsPath,
                JsonSerializer.Serialize(Launcher, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );
        }

        private void SaveClientSettings()
        {
            File.WriteAllText(
                ClientSettingsPath,
                JsonSerializer.Serialize(Client, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );
        }
    }
}