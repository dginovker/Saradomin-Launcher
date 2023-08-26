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
        private readonly ISettingsService _settingsService;  // Use the interface here
        private readonly IClientUpdateService _clientUpdateService;

        public ClientLaunchService(
            ISettingsService settingsService,  // Use the interface here
            IClientUpdateService clientUpdateService
        )
        {
            _settingsService = settingsService;
            _clientUpdateService = clientUpdateService;
        }

        public async Task LaunchClient()
        {
            Console.WriteLine("Launching client...");
            if (!IsJavaVersion11())
            {
                Console.WriteLine("Java version is not 11. Downloading and setting Java 11.");
                await DownloadAndSetJava11();
            }
            
            Console.WriteLine($"Done making sure Java 11 is set. Launching client with Java 11 at {_settingsService.Launcher.JavaExecutableLocation}.");

            // Original code to launch the game
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

        private bool IsJavaVersion11()
        {
            string javaVersionOutput = RunCommandAndGetOutput(
                $"{_settingsService.Launcher.JavaExecutableLocation} -version"
            );
            Console.WriteLine($"Checking if Java version is 11. Output: {javaVersionOutput}");
            return javaVersionOutput.Contains("11");
        }

        private string RunCommandAndGetOutput(string command)
        {
            Process process = new Process();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
            }
            else if (
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            )
            {
                process.StartInfo = new ProcessStartInfo("bash", "-c \"" + command + "\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
            }

            process.Start();
            var output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        private async Task DownloadAndSetJava11()
        {
            string downloadUrl = GetJava11DownloadUrl();
            Console.WriteLine($"Downloading Java 11 from {downloadUrl}.");
            string downloadPath = Path.Combine(
                CrossPlatform.LocateDefault2009scapeHome(),
                "jre11" + Path.GetExtension(downloadUrl)
            );
            string extractedPath = Path.Combine(
                CrossPlatform.LocateDefault2009scapeHome(),
                "jre11"
            );
            Console.WriteLine($"Download path: {downloadPath}, extracted path: {extractedPath}.");

            using (HttpClient httpClient = new HttpClient())
            {
                var data = await httpClient.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(downloadPath, data);
            }

            if (Path.GetExtension(downloadUrl) == ".zip")
            {
                ZipFile.ExtractToDirectory(downloadPath, extractedPath);
            }
            else if (Path.GetExtension(downloadUrl) == ".tar.gz")
            {
                if (!Directory.Exists(extractedPath)) Directory.CreateDirectory(extractedPath);
                string extractOutput = RunCommandAndGetOutput($"tar xf {downloadPath} -C {extractedPath} --strip-components 1");
                Console.WriteLine($"Extract output: {extractOutput}");
            }

            Console.WriteLine($"Old Java executable location: {_settingsService.Launcher.JavaExecutableLocation}.");
            _settingsService.Launcher.JavaExecutableLocation = Path.Combine(
                extractedPath,
                "bin/java"
            );
            _settingsService.SaveAll();
            
            Console.WriteLine($"New Java executable location: {_settingsService.Launcher.JavaExecutableLocation}.");
        }

        private string GetJava11DownloadUrl()
        {
            string architecture = GetSystemArchitecture();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.20%2B8/OpenJDK11U-jre_x64_windows_hotspot_11.0.20_8.zip";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return architecture == "x64"
                    ? "https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.20%2B8/OpenJDK11U-jre_x64_linux_hotspot_11.0.20_8.tar.gz"
                    : "https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.20%2B8/OpenJDK11U-jre_aarch64_linux_hotspot_11.0.20_8.tar.gz";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return architecture == "x64"
                    ? "https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.20%2B8/OpenJDK11U-jre_x64_mac_hotspot_11.0.20_8.tar.gz"
                    : "https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.20%2B8/OpenJDK11U-jre_aarch64_mac_hotspot_11.0.20_8.tar.gz";
            }
            else
            {
                throw new NotSupportedException("Your platform is not supported.");
            }
        }

        private string GetSystemArchitecture()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                return "x64";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                return "aarch64";
            }
            else
            {
                throw new NotSupportedException("Your architecture is not supported.");
            }
        }
    }
}
