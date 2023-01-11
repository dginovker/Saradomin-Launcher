using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Glitonea.Extensions;
using Glitonea.Mvvm;
using Glitonea.Utilities;
using Saradomin.Infrastructure.Messaging;
using Saradomin.Infrastructure.Services;
using Saradomin.Model.Settings.Client;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Utilities;
using Saradomin.View.Windows;

namespace Saradomin.ViewModel.Controls
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;

        public LauncherSettings Launcher => _settingsService.Launcher;
        public ClientSettings Client => _settingsService.Client;

        public bool CanCustomize => false;

        public string VersionString
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version!;
                return $"Version {version.Major}.{version.Minor}.{version.Build}";
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
                        return ClientSettings.ServerProfile.Unsupported;
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

        private void LaunchScapeWebsite()
        {
            CrossPlatform.LaunchURL("https://2009scape.org");
        }

        private void OpenPluginTutorial()
        {
            CrossPlatform.LaunchURL("https://gitlab.com/2009scape/tools/client-plugins");
        }

        private void LaunchProjectWebsite()
        {
            CrossPlatform.LaunchURL("https://gitlab.com/2009scape/Saradomin-Launcher");
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

            OnPropertyChanged(nameof(CanCustomize));
        }

        private void ResetSlayerTracker()
        {
            Client.Customization.SlayerTracker.SetDefaults();
        }

        private void ResetRightClickMenu()
        {
            Client.Customization.RightClickMenu.SetDefaults();
        }

        private async Task BrowseForJavaExecutable()
        {
            var ofd = new OpenFileDialog
            {
                Title = "Browse for Java...",
                AllowMultiple = false,
                Directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            var paths = await ofd.ShowAsync(Application.Current.GetMainWindow());

            if (paths != null && paths.Length > 0)
            {
                Launcher.JavaExecutableLocation = paths[0];
            }
        }
        
        private async Task BrowseForInstallationDirectory()
        {
            var ofd = new OpenFolderDialog
            {
                Title = "Browse for Installation Directory...",
                Directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            var path = await ofd.ShowAsync(Application.Current.GetMainWindow());

            if (path != null)
            {
                if (CrossPlatform.IsDirectoryWritable(path))
                {
                    Launcher.InstallationDirectory = path;
                }
                else
                {
                    NotificationBox.DisplayNotification(
                        "Access denied",
                        "The location you have selected is not writable. Select the one you have permissions for.",
                        Application.Current.GetMainWindow()
                    );
                }
            }
        }
    }
}