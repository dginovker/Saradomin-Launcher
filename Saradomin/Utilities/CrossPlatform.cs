using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Mono.Unix;

namespace Saradomin.Utilities
{
    public static class CrossPlatform
    {
        public static void LaunchURL(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }

        public static bool IsJavaExecutableValid(string location)
        {
            try
            {
                if (!File.Exists(location))
                {
                    return false;
                }

                using (var fileStream = File.OpenRead(location))
                {
                    var bytes = new byte[4];
                    fileStream.Read(bytes, 0, 4);

                    if (bytes[0] == 0x7F
                        && bytes[1] == 0x45
                        && bytes[2] == 0x4C
                        && bytes[3] == 0x46)
                    {
                        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
                    }

                    if (bytes[0] == 'M'
                        && bytes[1] == 'Z')
                    {
                        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    }

                    if ((bytes[0] == 0xCF
                        && bytes[1] == 0xFA)
                        || (bytes[0] == 0xCA
                        && bytes[1] == 0xFE))
                    {
                        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
                    }

                }
            }
            catch
            {
                // Ignore
            }
            
            return false;
        }

        public static string LocateJavaExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var envPath = Environment.GetEnvironmentVariable("JAVA_HOME");

                if (!string.IsNullOrEmpty(envPath))
                    return Path.Combine(envPath, "bin/java.exe");
                
                using (var rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\JavaSoft\\Java Runtime Environment\\"))
                {
                    if (rk == null)
                        return null;

                    var currentVersion = rk.GetValue("CurrentVersion")?.ToString();

                    if (currentVersion == null)
                        return null;
                    
                    using (var key = rk.OpenSubKey(currentVersion))
                    {
                        if (key == null)
                            return null;
                        
                        envPath = key.GetValue("JavaHome")?.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(envPath))
                    return Path.Combine(envPath, "bin/java.exe");

                throw new FileNotFoundException("Failed to find Java. Make sure it's installed!");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                     || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                var proc = new Process
                {
                    StartInfo = new("/bin/which")
                    {
                        Arguments = "java",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };

                proc.Start();
                proc.WaitForExit();
                var data = proc.StandardOutput.ReadToEnd();

                if (!string.IsNullOrEmpty(data))
                    return UnixPath.GetCompleteRealPath(data.Trim());

                throw new FileNotFoundException("Failed to find Java. Make sure it's installed!");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var proc = new Process
                {
                    StartInfo = new("/usr/bin/which")
                    {
                        Arguments = "java",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };

                proc.Start();
                proc.WaitForExit();
                var data = proc.StandardOutput.ReadToEnd();

                if (!string.IsNullOrEmpty(data))
                    return Path.Combine(UnixPath.GetCompleteRealPath(data.Trim()));

                throw new FileNotFoundException("Failed to find Java. Make sure it's installed!");
            }
            else
            {
                throw new NotSupportedException("Your platform is not supported.");
            }
        }

        public static string LocateUnixUserHome()
        {
            return Environment.GetEnvironmentVariable("XDG_DATA_HOME")
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local",
                    "share"
                );
        }

        public static string LocateDefault2009scapeHome()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return Path.Combine(
                    // Get the XDG_DATA_HOME environment variable, or if it doesn't exist, use the default ~/.local/share
                    LocateUnixUserHome(),
                    "2009scape"
                );
            }
            else
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "2009scape"
                );
            }
        }

        public static string LocateSaradominHome()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return Path.Combine(
                    // Get the XDG_DATA_HOME environment variable, or if it doesn't exist, use the default ~/.local/share
                    LocateUnixUserHome(),
                    "2009scape",
                    "saradomin"
                );
            }
            else
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "2009scape",
                    "saradomin"
                );
            }
        }
        
        public static string Locate2009scapeExecutable(string baseDirectory)
        {
            baseDirectory ??= LocateDefault2009scapeHome();
            return Path.Combine(baseDirectory, "2009scape.jar");
        }

        public static string LocateServerProfilesPath(string baseDirectory)
        {
            baseDirectory ??= LocateDefault2009scapeHome();
            return Path.Combine(baseDirectory, "server_profiles.json");
        }

        public static string RunCommandAndGetOutput(string command)
        {
            Process process = new Process();
            StringBuilder output = new StringBuilder();

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

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) output.AppendLine(e.Data);
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            return output.ToString();
        }

        public static string GetJava11DownloadUrl()
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

        private static string GetSystemArchitecture()
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
        public static bool IsDirectoryWritable(string directoryPath)
        {
            var testFilePath = Path.Combine(directoryPath, "test");

            try
            {
                File.Create(testFilePath).Dispose();
                File.Delete(testFilePath);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}