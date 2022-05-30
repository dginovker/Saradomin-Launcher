using System;
using System.Diagnostics;
using System.IO;
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
        
        private string JavaExecutablePath { get; }

        public ClientLaunchService(ISettingsService settingsService,
                                   IClientUpdateService clientUpdateService)
        {
            _settingsService = settingsService;
            _clientUpdateService = clientUpdateService;
            
            JavaExecutablePath = CrossPlatform.LocateJavaExecutable();
        }
        
        public async Task LaunchClient()
        {
            var proc = new Process
            {
                StartInfo = new(JavaExecutablePath)
                {
                    Arguments = $"-jar {_clientUpdateService.PreferredTargetFilePath}",
                    WorkingDirectory = CrossPlatform.Locate2009scapeHome(),
                    UseShellExecute = true
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