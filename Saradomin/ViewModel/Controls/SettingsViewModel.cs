using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Glitonea.Extensions;
using Glitonea.Mvvm;
using Glitonea.Mvvm.Messaging;
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

        public ObservableCollection<EnumDescription> ServerProfiles { get; private set; } = new()
        {
            ClientSettings.ServerProfile.Live.ToDescription(),
            ClientSettings.ServerProfile.Testing.ToDescription(),
            ClientSettings.ServerProfile.Local.ToDescription()
        };

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            Message.Subscribe<MainViewLoadedMessage>(this, OnMainViewLoaded);
        }

        public void LaunchScapeWebsite()
        {
            CrossPlatform.LaunchURL("https://2009scape.org");
        }

        public void OpenPluginTutorial()
        {
            CrossPlatform.LaunchURL("https://gitlab.com/2009scape/tools/client-plugins");
        }

        public void LaunchProjectWebsite()
        {
            CrossPlatform.LaunchURL("https://gitlab.com/2009scape/Saradomin-Launcher");
        }

        public void OnMainViewLoaded(MainViewLoadedMessage _)
        {
            Message.Subscribe<SettingsModifiedMessage>(this, OnSettingsModified);
        }

        public void OnSettingsModified(SettingsModifiedMessage _)
        {
            _settingsService.SaveAll();
        }

        public async Task BrowseForJavaExecutable()
        {
            var storageProvider = Application.Current!.GetMainWindow()!.StorageProvider;

            var storageFile = (await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Browse for Java...",
                AllowMultiple = false,
                SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                )
            })).FirstOrDefault();

            if (storageFile != null)
            {
                Launcher.JavaExecutableLocation = storageFile.TryGetLocalPath();
            }
        }

        public async Task BrowseForInstallationDirectory()
        {
            var storageProvider = Application.Current!.GetMainWindow()!.StorageProvider;
            
            var storageFolder = (await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Browse for Installation Directory...",
                AllowMultiple = false,
                SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                )
            })).FirstOrDefault();

            var path = storageFolder?.TryGetLocalPath();
            
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