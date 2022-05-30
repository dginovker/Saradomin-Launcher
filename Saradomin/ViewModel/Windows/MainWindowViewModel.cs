using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Metadata;
using Glitonea.Mvvm;
using HtmlAgilityPack;
using Saradomin.Messaging;
using Saradomin.Model.Settings.Launcher;
using Saradomin.Services;
using Saradomin.Utilities;

namespace Saradomin.ViewModel.Windows
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IClientLaunchService _launchService;
        private readonly IClientUpdateService _updateService;

        private bool JavaExecutableValid
            => CrossPlatform.IsJavaExecutableValid(Launcher.JavaExecutableLocation);

        public string Title { get; set; } = "2009scape launcher";
        public LauncherSettings Launcher { get; private set; }

        public bool CanLaunch { get; private set; } = true;
        public string LaunchText { get; private set; } = "Play!";

        public StackPanel ContentContainer { get; private set; }

        public MainWindowViewModel(IClientLaunchService launchService,
            IClientUpdateService updateService,
            ISettingsService settingsService)
        {
            _launchService = launchService;
            _updateService = updateService;
            _updateService.DownloadProgressChanged += OnClientDownloadProgressUpdated;

            Launcher = settingsService.Launcher;

            ContentContainer = new StackPanel
            {
                Margin = new(4)
            };

            App.Messenger.Register<MainViewLoadedMessage>(this, MainViewLoaded);
            App.Messenger.Register<SettingsModifiedMessage>(this, SettingsModified);

            if (!JavaExecutableValid)
            {
                Launcher.JavaExecutableLocation = CrossPlatform.LocateJavaExecutable();
            }
        }

        public void ExitApplication()
        {
            Environment.Exit(0);
        }

        public async void MainViewLoaded(MainViewLoadedMessage _)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://2009scape.org/services/m=news/archives/latest.html");
                var doc = new HtmlDocument();
                doc.Load(await response.Content.ReadAsStreamAsync());
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='msgcontents']");

                ContentContainer.MaxWidth = 760;
                var renderer = new HtmlRenderer(ContentContainer, node);
                renderer.RenderToContainer();
            }
        }

        public void SettingsModified(SettingsModifiedMessage msg)
        {
            if (msg.SettingName == nameof(LauncherSettings.JavaExecutableLocation))
            {
                if (!JavaExecutableValid)
                {
                    CanLaunch = false;
                    LaunchText = "Unable to locate Java. Find Java executable using Settings page.";
                }
                else
                {
                    CanLaunch = true;
                    LaunchText = "Play!";
                }
            }
        }

        public void LaunchPage(string parameter)
        {
            var url = parameter switch
            {
                "news" => "https://2009scape.org/services/m=news/archives/latest.html",
                "issues" => "https://gitlab.com/2009scape/2009scape/-/issues",
                "hiscores" => "https://2009scape.org/services/m=hiscore/hiscores.html?world=2",
                "discord" => "https://discord.gg/YY7WSttN7H",
                _ => throw new ArgumentException($"{parameter} is not a valid page parameter.")
            };

            CrossPlatform.LaunchURL(url);
        }

        [DependsOn(nameof(CanLaunch))]
        public bool CanExecuteLaunchSequence(object param)
            => CanLaunch;

        public async Task ExecuteLaunchSequence()
        {
            CanLaunch = false;

            try
            {
                if (!File.Exists(_updateService.PreferredTargetFilePath) || Launcher.CheckForClientUpdatesOnLaunch)
                    await AttemptUpdate();
            }
            catch (Exception e)
            {
                CanLaunch = true;
                LaunchText = $"Failed to update 2009scape: {e.Message}";
                return;
            }

            LaunchText = "Play! (already running)";
            {
                // Will block this task until client process exits.
                await _launchService.LaunchClient();
            }

            CanLaunch = true;
            LaunchText = "Play!";
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
                if (Launcher.ClientProfile == LauncherSettings.ClientReleaseProfile.Experimental
                    && File.Exists(CrossPlatform.Locate2009scapeExperimentalExecutable()))
                {
                    return;
                }

                LaunchText = $"Updating... (Downloading client: 0%)";
                Directory.CreateDirectory(CrossPlatform.Locate2009scapeHome());

                try
                {
                    await _updateService.FetchRemoteClientExecutableAsync(CancellationToken.None);
                }
                catch (Exception)
                {
                    var clientPath = CrossPlatform.Locate2009scapeLegacyExecutable();

                    if (!File.Exists(clientPath))
                    {
                        LaunchText = "Cannot launch. Missing client executable. Click me again to re-try.";
                        throw;
                    }
                }
            }
        }

        private void OnClientDownloadProgressUpdated(object sender, float e)
        {
            LaunchText = $"Updating... (Downloading client: {e * 100:F2}%)";
        }
    }
}