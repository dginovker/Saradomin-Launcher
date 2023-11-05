using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace Saradomin.Utilities;

public static class SingleplayerManagement
{
    private static readonly string[] DirsToBackup =
    {
        "game/data/players",
        "game/data/serverstore",
        "game/worldprops"
    };

    private static readonly string[] FilesToBackup =
    {
        "game/data/eco/ge_resource.emp",
        "game/data/eco/grandexchange.db"
    };

    public static void MakeBackup(Action<string> log)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string newBackupDir = Path.Combine(
            CrossPlatform.GetSingleplayerBackupsHome(),
            timestamp
        );

        foreach (string dir in DirsToBackup)
        {
            string sourcePath = Path.Combine(CrossPlatform.GetSingleplayerHome(), dir);
            if (!Directory.Exists(sourcePath))
            {
                log($"  Skipping backup of {dir} because it doesn't exist (Full path attempted: {sourcePath})");
                continue;
            }

            string destPath = Path.Combine(newBackupDir, dir);
            CopyDirectory(sourcePath, destPath);
        }

        foreach (string file in FilesToBackup)
        {
            string sourceFilePath = Path.Combine(CrossPlatform.GetSingleplayerHome(), file);
            if (!File.Exists(sourceFilePath))
            {
                log($"  Skipping backup of {file} because it doesn't exist (Full path attempted: {sourceFilePath})");
                continue;
            }

            string destFilePath = Path.Combine(newBackupDir, file);
            string directoryForFile = Path.GetDirectoryName(destFilePath);
            if (!Directory.Exists(directoryForFile))
                Directory.CreateDirectory(directoryForFile);
            File.Copy(sourceFilePath, destFilePath, true);
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(destinationDir))
            Directory.CreateDirectory(destinationDir);

        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(filePath);
            string destFilePath = Path.Combine(destinationDir, fileName);
            File.Copy(filePath, destFilePath, true);
        }

        foreach (string subDirPath in Directory.GetDirectories(sourceDir))
        {
            string subDirName = Path.GetFileName(subDirPath);
            string destSubDirPath = Path.Combine(destinationDir, subDirName);
            // Recurse!
            CopyDirectory(subDirPath, destSubDirPath);
        }
    }

    public static string FindMostRecentBackupDirectory(IEnumerable<string> directories)
    {
        return directories.Select(Path.GetFileName).MaxBy(name => name);
    }

    public static void ApplyLatestBackup(Action<string> log)
    {
        var backupHome = CrossPlatform.GetSingleplayerBackupsHome();
        var singleplayerHome = CrossPlatform.GetSingleplayerHome();

        // Get all backup directories
        string[] backupDirectories = Directory.Exists(backupHome) ? Directory.GetDirectories(backupHome) : Array.Empty<string>();

        string mostRecentBackupDirName = FindMostRecentBackupDirectory(backupDirectories);

        if (mostRecentBackupDirName == null)
        {
            log("  No backups found.");
            return;
        }

        string mostRecentBackupFullPath = Path.Combine(backupHome, mostRecentBackupDirName);

        // Get the list of files in the most recent backup
        var filesInBackup = Directory.GetFiles(
            mostRecentBackupFullPath,
            "*",
            SearchOption.AllDirectories
        );

        foreach (var file in filesInBackup)
        {
            // Calculate the file's destination path in the singleplayer home directory
            var relativePath = file.Substring(mostRecentBackupFullPath.Length)
                .TrimStart(Path.DirectorySeparatorChar);
            var destFilePath = Path.Combine(singleplayerHome, relativePath);

            // Ensure the destination directory exists
            var destDirectory = Path.GetDirectoryName(destFilePath);
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            // Copy the file to the destination, overwriting any existing file
            File.Copy(file, destFilePath, true);
        }

        log($"  Backup from {mostRecentBackupDirName} has been successfully applied.");
    }

    private static Dictionary<string, string> _confCache;
    private static string ConfPath => Path.Combine(CrossPlatform.GetSaradominHome(), "game", "worldprops", "default.conf");
    public static Dictionary<string, string> GrabConfCache()
    {
        return File.ReadLines(ConfPath)
            .Where(line => line.Contains('=') && !line.TrimStart().StartsWith("#"))
            .Select(l => l.Split(new[] { '#', '=' }, 3))
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    }
    
    public static T ParseConf<T>(string key, T defaultValue = default)
    {
        Console.WriteLine($"Getting key {key}..");
        if (!File.Exists(ConfPath)) return defaultValue;
        _confCache ??= GrabConfCache();
        if (!_confCache.TryGetValue(key, out var value))
        {
            Console.WriteLine($"Cache not hit - returning default {value}");
            return defaultValue;
        }
        Console.WriteLine($"Cache hit - returning {value}");
        return (T)Convert.ChangeType(value, typeof(T));
    }

    public static void WriteConf(string key, object value)
    {
        Console.WriteLine($"Writing {key}");
        _confCache[key] = value.ToString().ToLowerInvariant(); // Update cache
        var filePath = CrossPlatform.GetSingleplayerHome() + "/game/worldprops/default.conf";
        var lines = File.ReadAllLines(filePath).Select(l =>
            l.StartsWith(key + " =")
                ? $"{key} = {_confCache[key]}" + (l.Contains("#") ? " " + l.Substring(l.IndexOf('#')) : "")
                : l);
        File.WriteAllLines(filePath, lines);
    }
}