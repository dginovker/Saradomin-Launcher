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
                Process.Start(url);
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

        public static string LocateJavaExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var envPath = Environment.GetEnvironmentVariable("JAVA_HOME");

                if (!string.IsNullOrEmpty(envPath))
                    return Path.Combine(envPath, "bin/java.exe");
                
                using (var rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\JavaSoft\\Java Runtime Environment\\"))
                {
                    var currentVersion = rk.GetValue("CurrentVersion").ToString();
                    using (var key = rk.OpenSubKey(currentVersion))
                    {
                        envPath = key.GetValue("JavaHome").ToString();
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

                throw new DirectoryNotFoundException("Failed to find Java. Make sure it's installed!");
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