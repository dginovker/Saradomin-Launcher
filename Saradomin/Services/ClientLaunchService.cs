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
        
        private string JavaExecutablePath { get; }

        public ClientLaunchService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            
            JavaExecutablePath = CrossPlatform.LocateJavaExecutable();
        }
        
        public async Task LaunchClient()
        {
            var proc = new Process
            {
                StartInfo = new(JavaExecutablePath)
                {
                    Arguments = $"-jar {CrossPlatform.Locate2009scapeExecutable()}", // todo implement profiles
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