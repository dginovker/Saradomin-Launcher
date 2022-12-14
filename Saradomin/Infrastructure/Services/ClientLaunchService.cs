using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Saradomin.Utilities;

namespace Saradomin.Infrastructure.Services
{
    public class ClientLaunchService : IClientLaunchService
    {
        private readonly ISettingsService _settingsService;
        private readonly IClientUpdateService _clientUpdateService;

        public event PropertyChangedEventHandler PropertyChanged;

        public string scl = "";

        public ClientLaunchService(ISettingsService settingsService,
            IClientUpdateService clientUpdateService)
        {
            _settingsService = settingsService;
            _clientUpdateService = clientUpdateService;
        }

        public async Task LaunchClient()
        {
            if(_settingsService.Client.OfficialLauncher.Scale2x)
            {
                scl = "-Dsun.java2d.uiScale=2";
            } 
            var proc = new Process
            {
                StartInfo = new(_settingsService.Launcher.JavaExecutableLocation)
                {
                    Arguments = $"-jar {scl} {_clientUpdateService.PreferredTargetFilePath}",
                    WorkingDirectory = _settingsService.Launcher.InstallationDirectory,
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