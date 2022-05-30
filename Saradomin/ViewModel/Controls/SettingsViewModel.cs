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

        public ClientSettings.ServerProfile ServerProfile
        {
            get
            {
                switch (Client.GameServerAddress)
                {
                    case ClientSettings.LiveServerAddress:
                        return ClientSettings.ServerProfile.Live;
                    
                    case ClientSettings.TestServerAddress:
                        return ClientSettings.ServerProfile.Testing;
                    
                    case ClientSettings.LocalServerAddress:
                        return ClientSettings.ServerProfile.Local;
                    
                    default:
                        Client.ManagementServerAddress = ClientSettings.ServerProfile.Live.ToDescription().Hint;
                        Client.GameServerAddress = ClientSettings.ServerProfile.Live.ToDescription().Hint;
                        return ClientSettings.ServerProfile.Live;
                }
            }

            set
            {
                Client.ManagementServerAddress = value.ToDescription().Hint;
                Client.GameServerAddress = value.ToDescription().Hint;

                OnPropertyChanged(nameof(ServerProfile));
            }
        }

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

        public ObservableCollection<EnumDescription> ServerProfiles { get; private set; } = new()
        {
            ClientSettings.ServerProfile.Live.ToDescription(),
            ClientSettings.ServerProfile.Testing.ToDescription(),
            ClientSettings.ServerProfile.Local.ToDescription()
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
    }
}