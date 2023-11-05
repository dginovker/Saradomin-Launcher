using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Glitonea.Mvvm;
using Glitonea.Mvvm.Messaging;
using Saradomin.Infrastructure.Messaging;
using Saradomin.Infrastructure.Services;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Utilities;

namespace Saradomin.ViewModel.Controls
{
    public class SingleplayerViewModel : ViewModelBase
    {
        private readonly ISingleplayerUpdateService _singleplayerUpdateService;
        private readonly ISettingsService _settingsService;

        public string SingleplayerDownloadText { get; private set; } = "Download Singleplayer";
        public bool CanDownload { get; private set; } = true;
        public bool CanLaunch => File.Exists(CrossPlatform.LocateSingleplayerExecutable()) && !_isRunning;
        public TextBox SingleplayerLogsTextBox { get; private set; }
        public bool ShowLogPanel {get; private set; }
        public LauncherSettings Launcher => _settingsService.Launcher;

        private bool _isRunning;
        public SingleplayerViewModel(ISettingsService settingsService, ISingleplayerUpdateService iSingleplayerUpdateService)
        { 
            _settingsService = settingsService; 
            Message.Subscribe<MainViewLoadedMessage>(this, OnMainViewLoaded); // todo test delete
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
                PrintLog($"Singleplayer Download/Update complete");
                SingleplayerManagement.ApplyLatestBackup();
                PrintLog($"Restored progress from last made backup");
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
            MakeBackup();
            PrintLog($"Starting singleplayer download/update...");
            _singleplayerUpdateService.DownloadSingleplayer();
        }

        public void MakeBackup()
        {
            PrintLog("Starting backup of player saves and economy files..");
            SingleplayerManagement.MakeBackup();
        }

        public void OpenBackupFolder()
        {
            if (!Directory.Exists(CrossPlatform.LocateSingleplayerBackupsHome()))
                Directory.CreateDirectory(CrossPlatform.LocateSingleplayerBackupsHome());
            CrossPlatform.OpenFolder(CrossPlatform.LocateSingleplayerBackupsHome());
        }

        private void PrintLog(string message)
        {
            ShowLogPanel = true;
            Dispatcher.UIThread.Post(() =>
            {
                SingleplayerLogsTextBox.Text += message + Environment.NewLine;
            }, DispatcherPriority.Background);
        }
        
        public void LaunchSingleplayer()
        {
            _isRunning = true;
            new Task(() => 
                    CrossPlatform.RunCommandAndGetOutput($"{CrossPlatform.LocateSingleplayerExecutable()} {Launcher.JavaExecutableLocation}",
                    PrintLog, 
                    PrintLog)
                ).Start();
        }

        private void LaunchFaq()
        {
            CrossPlatform.LaunchURL("https://2009scape.org/site/game_guide/singleplayer.html");
        }

        private void LaunchForums()
        {
            CrossPlatform.LaunchURL("https://forum.2009scape.org/viewforum.php?f=8-support");
        }
    }
}