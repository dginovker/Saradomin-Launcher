using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using Glitonea.Mvvm;
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
        
        public ObservableCollection<string> MusicTitles { get; private set; }

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
    }
}