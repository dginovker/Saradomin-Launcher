using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
                    return false;

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
            else
            {
                throw new NotSupportedException("Your platform is not supported.");
            }
        }

        public static string Locate2009scapeHome()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local/share/2009scape"
                );
            }
            else
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
        }
        
        public static string Locate2009scapeLegacyExecutable()
        {
            return Path.Combine(Locate2009scapeHome(), "2009scape.jar");
        }
        
        public static string Locate2009scapeExperimentalExecutable()
        {
            return Path.Combine(Locate2009scapeHome(), "2009scape_pazaz.jar");
        }
    }
}