using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using Glitonea.Extensions;
using Glitonea.Mvvm;
using Glitonea.Utilities;
using Saradomin.Messaging;
using Saradomin.Model.Settings.Client;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Services;

namespace Saradomin.ViewModel.Controls
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;

        public LauncherSettings Launcher => _settingsService.Launcher;
        public ClientSettings Client => _settingsService.Client;

        public string LoginMusicTheme
        {
            get => Launcher.UserFriendlySongName;
            set
            {
                // Workaround for client behavior where it fails to retrieve
                // songs with apostrophes in names from the cache.
                Client.Customization.LoginMusicTheme = value.Replace("'", "");
                Launcher.UserFriendlySongName = value;
                
                OnPropertyChanged(nameof(LoginMusicTheme));
            }
        }

        public string ServerAddress
        {
            get => Client.ServerAddress;
            set
            {
                Client.ServerAddress = value;
                Client.IpManagement = value;

                OnPropertyChanged(nameof(ServerAddress));
            }
        }

        public bool IsServerAddressBeingEdited { get; set; }

        public bool IsLiveServerSelected => !IsServerAddressBeingEdited && ServerAddress == ClientSettings.LiveServerAddress;
        public bool IsTestingServerSelected => !IsServerAddressBeingEdited && ServerAddress == ClientSettings.TestServerAddress;
        
        public ObservableCollection<string> MusicTitles { get; private set; }
        
        public ObservableCollection<EnumDescription> DropModes { get; private set; } = new()
        {
            XpTrackerSettings.DropModeSetting.Instant.ToDescription(),
            XpTrackerSettings.DropModeSetting.Incremental.ToDescription(),
        };

        public ObservableCollection<EnumDescription> TrackingModes { get; private set; } = new()
        {
            XpTrackerSettings.TrackingModeSetting.TotalXP.ToDescription(),
            XpTrackerSettings.TrackingModeSetting.RecentSkill.ToDescription(),
        };

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            
            App.Messenger.Register<MainViewLoadedMessage>(this, OnMainViewLoaded);
            
            InitializeMusicTitleRepository();
        }

        private void InitializeMusicTitleRepository()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "Saradomin.Resources.Lists.MusicTracks.json"
            );
            
            MusicTitles = JsonSerializer.Deserialize<ObservableCollection<string>>(stream!);
        }

        private void OnMainViewLoaded(MainViewLoadedMessage _)
        {
            App.Messenger.Register<SettingsModifiedMessage>(this, OnSettingsModified);
        }

        private void OnSettingsModified(SettingsModifiedMessage _)
        {
            _settingsService.SaveAll();
        }

        private void ResetSlayerTracker()
        {
            Client.Customization.SlayerTracker.SetDefaults();
        }

        private void ResetRightClickMenu()
        {
            Client.Customization.RightClickMenu.SetDefaults();
        }

        private void ResetServerConfiguration()
        {
            Client.GameServerPort = 43594;
            Client.CacheServerPort = 43595;
            Client.WorldListServerPort = 5555;
            
            ServerAddress = ClientSettings.LiveServerAddress;
        }

        private void UpdateServerProfile(string param)
        {
            ServerAddress = param switch
            {
                "live" => ClientSettings.LiveServerAddress,
                "testing" => ClientSettings.TestServerAddress,
                _ => throw new NotSupportedException($"{param} is not a supported parameter.")
            };
        }
    }
}