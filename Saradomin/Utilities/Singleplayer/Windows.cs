using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace Saradomin.Utilities.Singleplayer;

public static class Windows
{
    private static bool IsPortInUse(int port)
    {
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

        return tcpConnInfoArray.Any(endpoint => endpoint.Port == port);
    }        
    
    private static Process StartJavaProcess(string javaExecutable, string jarPath, string memoryAllocation, Action<string> outputHandler, Action onExit)
    {
        Process process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = javaExecutable,
            Arguments = $"-Xmx{memoryAllocation} -Xms{memoryAllocation} -jar \"{jarPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.Combine(CrossPlatform.GetSingleplayerHome(), "game")
        };

        process.OutputDataReceived += (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data)) return;
            outputHandler?.Invoke(args.Data);
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data)) return;
            outputHandler?.Invoke(args.Data);
        };

        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => onExit?.Invoke();

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }
    
    public static void WindowsLaunchServerAndClient(string javaExecutableLocation, Action<string> log)
    {
        string serverJar = CrossPlatform.GetSingleplayerHome() + @"\game\server.jar";
        string clientJar = CrossPlatform.GetSingleplayerHome() + @"\game\client.jar";

        if (IsPortInUse(43595))
        {
            log("Port 43595 is in use. Cannot start the server again.");
            return;
        }

        Process serverProcess = StartJavaProcess(javaExecutableLocation, serverJar, "2G", log, null);
        
        while (!IsPortInUse(43595)) Thread.Sleep(1000);

        StartJavaProcess(javaExecutableLocation, clientJar, "1G", null, () =>
        {
            serverProcess.Kill();
        });
    }
    
}