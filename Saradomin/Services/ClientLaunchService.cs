using System.Diagnostics;
using System.IO;
using Saradomin.Utilities;

namespace Saradomin.Services
{
    public class ClientLaunchService : IClientLaunchService
    {
        private string JavaExecutablePath { get; }
        private string ScapeHome { get; }

        public ClientLaunchService()
        {
            JavaExecutablePath = CrossPlatform.LocateJavaExecutable();
            ScapeHome = CrossPlatform.Locate2009scapeHome();
        }
        
        public void LaunchClient()
        {
            new Process
            {
                StartInfo = new(JavaExecutablePath)
                {
                    Arguments = $"-jar {Path.Combine(ScapeHome)}/2009scape.jar", // todo implement profiles
                    WorkingDirectory = ScapeHome,
                    UseShellExecute = true
                }
            }.Start();
        }
    }
}