using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Saradomin.Utilities;

namespace Saradomin.Services
{
    public class ClientLaunchService : IClientLaunchService
    {
        private readonly ISettingsService _settingsService;
        private readonly IClientUpdateService _clientUpdateService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ClientLaunchService(ISettingsService settingsService,
            IClientUpdateService clientUpdateService)
        {
            _settingsService = settingsService;
            _clientUpdateService = clientUpdateService;
        }

        public async Task LaunchClient()
        {
            var proc = new Process
            {
                StartInfo = new(_settingsService.Launcher.JavaExecutableLocation)
                {
                    Arguments = $"-jar {_clientUpdateService.PreferredTargetFilePath}",
                    WorkingDirectory = CrossPlatform.Locate2009scapeHome(),
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            proc.Start();

            if (_settingsService.Launcher.ExitAfterLaunchingClient)
            {
                (Application.Current!.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)!.Shutdown();
                return;
            }

            await proc.WaitForExitAsync();
        }
    }
}