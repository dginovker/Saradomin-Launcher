using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Glitonea.Mvvm;
using Saradomin.Infrastructure.Services;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Utilities;
using Saradomin.View.Windows;
using static Saradomin.Utilities.SingleplayerManagement;

namespace Saradomin.ViewModel.Controls;

public class SingleplayerViewModel : ViewModelBase
{
    private readonly ISingleplayerUpdateService _singleplayerUpdateService;
    private readonly ISettingsService _settingsService;
    private readonly IJavaUpdateService _javaUpdateService;

    public string SingleplayerDownloadText { get; private set; } =
        Directory.Exists(CrossPlatform.GetSingleplayerHome()) ? "Update Singleplayer" : "Download Singleplayer";
    public bool CanLaunch { get; private set; } = File.Exists(CrossPlatform.LocateSingleplayerExecutable());
    public TextBox SingleplayerLogsTextBox { get; }
    public bool ShowLogPanel { get; private set; }
    public LauncherSettings Launcher => _settingsService.Launcher;

    public SingleplayerViewModel(ISettingsService settingsService,
        IJavaUpdateService javaUpdateService,
        ISingleplayerUpdateService iSingleplayerUpdateService)
    {
        _settingsService = settingsService;
        _javaUpdateService = javaUpdateService;
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
            PrintLog($"Singleplayer Download complete");
            PrintLog($"");
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
        if (File.Exists(CrossPlatform.LocateSingleplayerExecutable()) && !CanLaunch) return; // While the button could be disabled, this looks nicer visually (since we update the download progress in the button text)
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

    [Pure]
    public static bool IsServerTerminationLog(string log)
    {
        return Regex.IsMatch(log, @"^\[\d{2}:\d{2}:\d{2}\]: \[SystemTermination\] Server successfully terminated!\s*$"); 
    }
    
    private void PrintLog(string message)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) message = Regex.Replace(message, @"\e\[[0-9;]*m", string.Empty); 
        
        ShowLogPanel = true;
        Dispatcher.UIThread.Post(() => { SingleplayerLogsTextBox.Text += message + Environment.NewLine; },
            DispatcherPriority.Background);

        if (IsServerTerminationLog(message)) CanLaunch = true;
    }

    public async void LaunchSingleplayer()
    {
        string javaVersionOutput = CrossPlatform.RunCommandAndGetOutput(
            $"\"{Launcher.JavaExecutableLocation}\" -version"
        );
        if (!javaVersionOutput.Contains("11"))
        {
            PrintLog("You don't have Java 11 set! Saradomin will grab it's own copy..");
            await _javaUpdateService.DownloadAndSetJava11(_settingsService);
        }
        CanLaunch = false;
        PrintLog("Starting Singleplayer.. The Singleplayer client will launch when 2009scape is ready. Sit tight!");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            new Task(() => Utilities.Singleplayer.Windows.WindowsLaunchServerAndClient(Launcher.JavaExecutableLocation, PrintLog)).Start();
        }
        else
        {
            new Task(() =>
                CrossPlatform.RunCommandAndGetOutput(
                    $"{CrossPlatform.LocateSingleplayerExecutable()} \"{Launcher.JavaExecutableLocation}\"",
                    PrintLog,
                    PrintLog)
            ).Start();
        }
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

    public async void ResetToDefaults()
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        MakeBackup();
        PrintLog("Grabbing the default config from the latest singleplayer codebase..");
        try
        {
            string defaultConf = await httpClient.GetStringAsync("https://gitlab.com/2009scape/singleplayer/windows/-/raw/master/game/worldprops/default.conf");
            await File.WriteAllTextAsync(CrossPlatform.GetSingleplayerHome() + "/game/worldprops/default.conf",
                defaultConf);
        }
        catch (Exception ex)
        {
            PrintLog($"Couldn't reset game properties");
            PrintLog(ex.Message);
            return;
        }
        Cheats = GEAutoBuySell = Debug = false; // Force update UI
        FakePlayers = true;
        PrintLog("Saved new config.");
        PrintLog("");
    }
}