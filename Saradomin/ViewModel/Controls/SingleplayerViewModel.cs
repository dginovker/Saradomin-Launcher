using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Glitonea.Extensions;
using Glitonea.Mvvm;
using Glitonea.Mvvm.Messaging;
using Saradomin.Infrastructure.Messaging;
using Saradomin.Infrastructure.Services;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Utilities;
using Saradomin.View.Windows;

namespace Saradomin.ViewModel.Controls
{
    public class SingleplayerViewModel : ViewModelBase
    {
        private readonly ISingleplayerUpdateService _singleplayerUpdateService;
        private readonly ISettingsService _settingsService;

        public string SingleplayerDownloadText { get; private set; } = "Download Singleplayer";
        public bool CanDownload { get; private set; } = true;
        public bool CanLaunch => File.Exists(CrossPlatform.LocateSingleplayerExecutable());
        public bool IsRunning {get ; private set;  }
        public TextBox SingleplayerLogsTextBox { get; private set; }

        public LauncherSettings Launcher => _settingsService.Launcher;

        public SingleplayerViewModel(ISettingsService settingsService, ISingleplayerUpdateService iSingleplayerUpdateService)
        { 
            _settingsService = settingsService; 
            Message.Subscribe<MainViewLoadedMessage>(this, OnMainViewLoaded);
            _singleplayerUpdateService = iSingleplayerUpdateService;
            _singleplayerUpdateService.SingleplayerDownloadProgressChanged += OnSingleplayerDownloadProgressChanged;
            
            SingleplayerLogsTextBox = new TextBox
            {
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = 12,
                Margin = new Thickness(8, 0, 0, 0),
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
            };
        }

        private void OnSingleplayerDownloadProgressChanged(object sender, Tuple<float, bool> e)
        {
            float progress = e.Item1;
            bool finished = e.Item2;
            CanDownload = finished;
            if (finished)
            {
                SingleplayerDownloadText = "Download Singleplayer";
                return;
            }
            
            if (progress >= 1f)
            {
                SingleplayerDownloadText = "Extracting...";
                return;
            }
            SingleplayerDownloadText = $"Downloading... {progress * 100:F2}%";
        }

        private void OnMainViewLoaded(MainViewLoadedMessage _)
        {
            Message.Subscribe<SettingsModifiedMessage>(this, OnSettingsModified);
        }

        private void OnSettingsModified(SettingsModifiedMessage _)
        {
            Console.WriteLine("Saving settings");
            _settingsService.SaveAll();
        }
        
        public void DownloadSingleplayer()
        {
            _singleplayerUpdateService.DownloadSingleplayer();
            Console.WriteLine("Download singleplayer button clicked");
        }

        public void PrintLog(string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                SingleplayerLogsTextBox.Text += message + Environment.NewLine;
            }, DispatcherPriority.Background);
        }
        
        public void LaunchSingleplayer()
        {
            IsRunning = true;
            new Task(() => CrossPlatform.RunCommandAndGetOutput(CrossPlatform.LocateSingleplayerExecutable(), PrintLog, PrintLog)).Start();
        }

        private void LaunchFaq()
        {
            CrossPlatform.LaunchURL("https://2009scape.org/site/game_guide/singleplayer.html");
        }

        private void LaunchForums()
        {
            CrossPlatform.LaunchURL("https://forum.2009scape.org/viewforum.php?f=8-support");
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