using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Metadata;
using Glitonea.Mvvm;
using Glitonea.Mvvm.Messaging;
using HtmlAgilityPack;
using Saradomin.Infrastructure;
using Saradomin.Infrastructure.Services;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Utilities;
using Saradomin.View.Windows;

namespace Saradomin.ViewModel.Windows
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IClientLaunchService _launchService;
        private readonly IClientUpdateService _updateService;
        private readonly IJavaUpdateService _javaUpdateService;
        private readonly IRemoteConfigService _remoteConfigService;
        private readonly ISettingsService _settingsService;

        private LauncherSettings Launcher { get; }

        public string Title { get; set; } = "2009scape launcher";
        
        public bool CanLaunch { get; private set; } = true;
        public string LaunchText { get; private set; } = "Play!";

        public bool DimContent { get; private set; }
        public InlineCollection HtmlInlines { get; private set; }

        public MainWindowViewModel(IClientLaunchService launchService,
            IClientUpdateService updateService,
            ISettingsService settingsService,
            IRemoteConfigService remoteConfigService,
            IJavaUpdateService javaUpdateService)
        {
            _launchService = launchService;
            _updateService = updateService;
            _updateService.DownloadProgressChanged += OnClientDownloadProgressUpdated;
            _remoteConfigService = remoteConfigService;
            _javaUpdateService = javaUpdateService;
            _javaUpdateService.JavaDownloadProgressChanged += OnJavaDownloadProgressUpdated;

            _settingsService = settingsService;
            Launcher = _settingsService.Launcher;

            Message.Subscribe<MainViewLoadedMessage>(this, MainViewLoaded);
            Message.Subscribe<NotificationBoxStateChangedMessage>(this, NotificatationBoxStateChanged);
            Message.Subscribe<ClientLaunchRequestedMessage>(this, ClientLaunchRequested);

            _settingsService.Launcher.JavaExecutableLocation ??= CrossPlatform.LocateJavaExecutable();
        }

        public void ExitApplication()
        {
            Environment.Exit(0);
        }

        public async void ClientLaunchRequested(ClientLaunchRequestedMessage _)
        {
            if (CanLaunch)
                await ExecuteLaunchSequence();
        }

        public async void MainViewLoaded(MainViewLoadedMessage _)
        {
            using (var httpClient = new HttpClient())
            {
                HtmlNode node;
                var doc = new HtmlDocument();
                
                try
                {
                    var response =
                        await httpClient.GetAsync("https://2009scape.org/services/m=news/archives/latest.html");
                    doc.Load(await response.Content.ReadAsStreamAsync());
                    node = doc.DocumentNode.SelectSingleNode("//div[@class='msgcontents']");
                }
                catch (HttpRequestException)
                {
                    doc.LoadHtml("<html><h3>Not Available<h3><br/><body>This content is unavailable, likely due to a lack of an internet connection.</body></html>");
                    node = doc.DocumentNode;
                }

                var renderer = new HtmlRenderer(node);
                HtmlInlines = renderer.Render();
            }
        }

        public void NotificatationBoxStateChanged(NotificationBoxStateChangedMessage msg)
        {
            DimContent = msg.WasOpened;
        }

        public void LaunchPage(object parameter)
        {
            var url = parameter switch
            {
                "news" => "https://2009scape.org/services/m=news/archives/latest.html",
                "issues" => "https://gitlab.com/2009scape/2009scape/-/issues",
                "hiscores" => "https://2009scape.org/services/m=hiscore/hiscores.html?world=2",
                "forums" => "https://forum.2009scape.org",
                "discord" => "https://discord.gg/43YPGND",
                _ => throw new ArgumentException($"{parameter} is not a valid page parameter.")
            };

            CrossPlatform.LaunchURL(url);
        }

        [DependsOn(nameof(CanLaunch))]
        public bool CanExecuteLaunchSequence(object parameter)
            => CanLaunch;

        //Stub to maintain compatibility with AXAML
        public async Task ExecuteLaunchSequence()
        {
            await ExecuteLaunchSequence(false);
        }

        private async Task ExecuteLaunchSequence(bool forceWait)
        {
            CanLaunch = false;

            try
            {
                if (!File.Exists(_updateService.PreferredTargetFilePath) ||
                    _settingsService.Launcher.CheckForClientUpdatesOnLaunch)
                    await AttemptUpdate();
            }
            catch (Exception e)
            {
                CanLaunch = true;
                LaunchText = $"Failed to update 2009scape: {e.Message}";
                return;
            }

            try
            {
                if (!IsJavaVersion11())
                {
                    await _javaUpdateService.DownloadAndSetJava11(_settingsService);
                }
            } catch (Exception e)
            {
                CanLaunch = true;
                LaunchText = $"Failed to download and set Java 11: {e.Message}";
                return;
            }
            

            if (!File.Exists(CrossPlatform.GetServerProfilePath(CrossPlatform.Get2009scapeHome())) ||
                _settingsService.Launcher.CheckForServerProfilesOnLaunch)
                await AttemptServerProfileUpdate();

            try
            {
                LaunchText = "Play! (already running)";
                {
                    // Will block this task until client process exits.
                    var t = _launchService.LaunchClient();

                    if (!_settingsService.Launcher.AllowMultiboxing || forceWait)
                        await t;
                }
            }
            catch (Exception e)
            {
                NotificationBox.DisplayNotification(
                    "Error",
                    $"Unable to launch the 2009scape client.\n\n{e.Message}"
                );
            }
            finally
            {
                CanLaunch = true;
                LaunchText = "Play!";
                Message.Broadcast<ClientClosedMessage>();
            }
        }

        private async Task AttemptServerProfileUpdate()
        {
            var serverProfilePath = CrossPlatform.GetServerProfilePath(CrossPlatform.Get2009scapeHome());

            try
            {
                await _remoteConfigService.FetchServerProfileConfig(
                    serverProfilePath
                );
            }
            catch
            {
                // Ignore. See next steps.
            }

            try
            {
                await _remoteConfigService.LoadServerProfileConfig(
                    serverProfilePath
                );
            }
            catch
            {
                _remoteConfigService.LoadFailsafeDefaults();
            }

            var relevantServerProfile = _remoteConfigService.AvailableProfiles.FirstOrDefault(
                x => x.GameServerAddress == _settingsService.Client.GameServerAddress
            );

            if (relevantServerProfile == null)
                return;

            _settingsService.Client.GameServerPort = relevantServerProfile.GameServerPort;
            _settingsService.Client.CacheServerPort = relevantServerProfile.CacheServerPort;
            _settingsService.Client.WorldListServerPort = relevantServerProfile.WorldListServerPort;
        }

        private async Task AttemptUpdate()
        {
            LaunchText = "Updating...";

            var localClientHash = string.Empty;
            var remoteClientHash = string.Empty;

            try
            {
                LaunchText = "Updating... (Computing local checksum)";
                localClientHash = await _updateService.ComputeLocalClientHashAsync();
            }
            catch (FileNotFoundException)
            {
                // Ignore. Client hash will stay empty.
            }

            if (!string.IsNullOrEmpty(localClientHash))
            {
                LaunchText = "Updating... (Fetching remote client checksum)";
                remoteClientHash = await _updateService.FetchRemoteClientHashAsync(CancellationToken.None);
            }

            if (string.IsNullOrEmpty(localClientHash)
                || remoteClientHash.Trim().ToLower() != localClientHash!.Trim().ToLower())
            {

                LaunchText = $"Updating... (Downloading client: 0%)";
                Directory.CreateDirectory(CrossPlatform.Get2009scapeHome());

                try
                {
                    await _updateService.FetchRemoteClientExecutableAsync(CancellationToken.None);
                }
                catch (Exception)
                {
                    var clientPath = _updateService.PreferredTargetFilePath;

                    if (!File.Exists(clientPath))
                    {
                        LaunchText = "Cannot launch. Missing client executable. Click me again to re-try.";
                        throw;
                    }
                }
            }
        }

        private bool IsJavaVersion11()
        {
            string javaVersionOutput = CrossPlatform.RunCommandAndGetOutput(
                $"\"{Launcher.JavaExecutableLocation}\" -version"
            );
            return javaVersionOutput.Contains("11");
        }
        
        private void OnClientDownloadProgressUpdated(object sender, float e)
        {
            LaunchText = $"Updating... (Downloading client - {e * 100:F2}%)";
        }
        private void OnJavaDownloadProgressUpdated(object sender, Tuple<float, bool> e)
        {
            if (e.Item2)
            {
                LaunchText = "Play! (Multiplayer)";
                return;
            }
            if (e.Item1 >= 0.999f)
            {
                LaunchText = "Updating... (Extracting Java 11)";
                return;
            }
            LaunchText = $"Updating... (Downloading Java 11 - {e.Item1 * 100:F2}%)";
        }
    }
}
