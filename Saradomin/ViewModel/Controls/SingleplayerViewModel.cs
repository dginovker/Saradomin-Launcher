using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Glitonea.Mvvm;
using Saradomin.Infrastructure.Services;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Utilities;
using static Saradomin.Utilities.SingleplayerManagement;

namespace Saradomin.ViewModel.Controls;

public class SingleplayerViewModel : ViewModelBase
{
    private readonly ISingleplayerUpdateService _singleplayerUpdateService;
    private readonly ISettingsService _settingsService;

    public string SingleplayerDownloadText { get; private set; } =
        Directory.Exists(CrossPlatform.GetSingleplayerHome()) ? "Update Singleplayer" : "Download Singleplayer";

    public bool CanDownload { get; private set; } = true;
    public bool CanLaunch { get; private set; } = File.Exists(CrossPlatform.LocateSingleplayerExecutable());
    public TextBox SingleplayerLogsTextBox { get; }
    public bool ShowLogPanel { get; private set; }
    public LauncherSettings Launcher => _settingsService.Launcher;

    public SingleplayerViewModel(ISettingsService settingsService,
        ISingleplayerUpdateService iSingleplayerUpdateService)
    {
        _settingsService = settingsService;
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
        if (finished)
        {
            SingleplayerDownloadText = "Update Singleplayer";
            ApplyLatestBackup(PrintLog);
            PrintLog($"");
            PrintLog($"Singleplayer Download complete");
            CanLaunch = true;
            return;
        }

        if (progress >= 1f)
        {
            SingleplayerDownloadText = "Extracting...";
            return;
        }

        SingleplayerDownloadText = $"Downloading... {progress * 100:F2}%";
    }

    public void DownloadSingleplayer()
    {
        CanLaunch = false;
        MakeBackup();
        PrintLog($"Starting singleplayer download...");
        _singleplayerUpdateService.DownloadSingleplayer();
    }

    private void MakeBackup()
    {
        PrintLog("Starting backup of player saves and economy files..");
        SingleplayerManagement.MakeBackup(PrintLog);
        PrintLog($"Done backing up player saves and economy files");
        PrintLog($"");
    }

    public void OpenBackupFolder()
    {
        if (!Directory.Exists(CrossPlatform.GetSingleplayerBackupsHome()))
            Directory.CreateDirectory(CrossPlatform.GetSingleplayerBackupsHome());
        CrossPlatform.OpenFolder(CrossPlatform.GetSingleplayerBackupsHome());
    }

    private void PrintLog(string message)
    {
        ShowLogPanel = true;
        Dispatcher.UIThread.Post(() => { SingleplayerLogsTextBox.Text += message + Environment.NewLine; },
            DispatcherPriority.Background);
    }

    public void LaunchSingleplayer()
    {
        CanLaunch = false;
        new Task(() =>
            CrossPlatform.RunCommandAndGetOutput(
                $"{CrossPlatform.LocateSingleplayerExecutable()} {Launcher.JavaExecutableLocation}",
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

    public bool Cheats 
    { 
        get => ParseConf<bool>("noauth_default_admin");
        set => WriteConf("noauth_default_admin", value);
    }

    public bool FakePlayers
    {
        get => ParseConf<bool>("enable_bots", true);
        set => WriteConf("enable_bots", value);
    }

    public bool GEAutoBuySell 
    { 
        get => ParseConf<bool>("i_want_to_cheat");
        set => WriteConf("i_want_to_cheat", value);
    }

    public bool Debug 
    { 
        get => ParseConf<bool>("debug");
        set => WriteConf("debug", value);
    }

    public void ResetToDefaults()
    {
        
    }
}