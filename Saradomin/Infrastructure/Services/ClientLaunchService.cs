using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Glitonea.Extensions;
using Saradomin.Utilities;

namespace Saradomin.Infrastructure.Services
{
    public class ClientLaunchService : IClientLaunchService
    {
        private readonly ISettingsService _settingsService;
        private readonly IClientUpdateService _clientUpdateService;

        public ClientLaunchService(
            ISettingsService settingsService,
            IClientUpdateService clientUpdateService
        )
        {
            _settingsService = settingsService;
            _clientUpdateService = clientUpdateService;
        }

        public async Task LaunchClient()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo(
                    $"{_settingsService.Launcher.JavaExecutableLocation}"
                )
                {
                    Arguments =
                        $"-Dsun.java2d.uiScale={_settingsService.Client.UiScale} "
                        + $"-DclientHomeOverride=\"{_settingsService.Launcher.InstallationDirectory}/\" "
                        + $"-jar \"{_clientUpdateService.PreferredTargetFilePath}\"",
                    WorkingDirectory = $"{_settingsService.Launcher.InstallationDirectory}",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            proc.Start();

            if (_settingsService.Launcher.ExitAfterLaunchingClient)
            {
                Application.Current.GetDesktopLifetime().Shutdown();
                return;
            }

            await proc.WaitForExitAsync();
        }

        
    }
}
