using System;
using System.Collections.ObjectModel;
using System.Reflection;
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
        
        public async Task BrowseForJavaExecutable()
        {
            var window = Application.Current.GetMainWindow();
            var pickerOptions = new FilePickerOpenOptions
            {
                Title = "Browse for Java...",
                AllowMultiple = false,
                SuggestedStartLocation =await window.StorageProvider.TryGetFolderFromPathAsync(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                )
            };

            var storageFiles = await window.StorageProvider.OpenFilePickerAsync(pickerOptions);
            
            if (storageFiles.Count > 0)
            {
                Launcher.JavaExecutableLocation = storageFiles[0].Path.AbsolutePath;
            }
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

        private void OnMainViewLoaded(MainViewLoadedMessage _)
        {
            Message.Subscribe<SettingsModifiedMessage>(this, OnSettingsModified);
        }

        private void OnSettingsModified(SettingsModifiedMessage _)
        {
            _settingsService.SaveAll();
        }
    }
}