using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitLabApiClient;
using GitLabApiClient.Models.Trees.Responses;
using Saradomin.Model;
using Saradomin.View.Windows;

namespace Saradomin.Infrastructure.Services
{
    public class PluginDownloadService : IPluginDownloadService
    {
        private const string GroupName = "2009scape";
        private const string ProjectName = "Tools/client-plugins";
        private const string BranchName = "master";

        private readonly GitLabClient _gitLabClient;
        private static IList<Tree> _cachedQuery;

        public PluginDownloadService()
        {
            _gitLabClient = new GitLabClient("https://gitlab.com");
        }

        private async Task PerformInitialQuery()
        {
            _cachedQuery = await _gitLabClient.Trees.GetAsync($"{GroupName}/{ProjectName}", o =>
                {
                    o.Recursive = true;
                    o.Reference = BranchName;
                }
            );
        }

        public async Task<List<string>> FetchPluginMetadataPaths()
        {
            if (_cachedQuery == null) await PerformInitialQuery();

            return _cachedQuery
                .Where(x => x.Type == "blob" && x.Path.Contains("properties"))
                .Select(x => x.Path)
                .ToList();
        }
        
        public async Task<List<string>> FetchFileListForPlugin(string pluginName)
        {
            if (_cachedQuery == null) await PerformInitialQuery();

            return _cachedQuery
                .Where(x => x.Path.Contains(pluginName) && x.Type == "blob")
                .Select(x => x.Path)
                .ToList();
        }

        public async Task DownloadPluginFiles(string pluginName, string pluginRepositoryPath)
        {
            var directoryPath = Path.Combine(pluginRepositoryPath, pluginName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            else
            {
                foreach (var file in Directory.EnumerateFiles(directoryPath))
                {
                    File.Delete(file);
                }
            }
            
            var pluginFiles = await FetchFileListForPlugin(pluginName);
            foreach (var filePath in pluginFiles)
            {
                var file = await _gitLabClient.Files.GetAsync($"{GroupName}/{ProjectName}", filePath, BranchName);

                var downloadDirectoryPath = Path.Combine(pluginRepositoryPath, Path.GetDirectoryName(filePath)!);
                var downloadFilePath = Path.Combine(pluginRepositoryPath, filePath);
                
                Directory.CreateDirectory(downloadDirectoryPath);
                
                await File.WriteAllBytesAsync(
                    downloadFilePath,
                    Convert.FromBase64String(file.Content)
                );
            }
        }

        public async Task <List<PluginInfo>> GetAllMetadata (string pluginRepositoryPath, bool isUpdateCheck, bool writePersistentUpdateFlag)
        {
            try
            {
                var metadataPaths = await FetchPluginMetadataPaths();
                var infos = new List<PluginInfo>();

                foreach (var path in metadataPaths)
                {
                    infos.Add(await ProcessMetadataPath(path, pluginRepositoryPath, isUpdateCheck,
                        writePersistentUpdateFlag));
                }
                
                return isUpdateCheck ? infos.Where(x => x.UpdateAvailable).ToList() : infos;
            }
            catch (HttpRequestException e)
            {
                NotificationBox.DisplayNotification("Error!", $"Plugin service unavailable: {e.Message}");
                return new List<PluginInfo>();
            }
        }

        private async Task<PluginInfo> ProcessMetadataPath(string path, string pluginRepositoryPath, bool isUpdateCheck, bool writePersistentUpdateFlag)
        {
            var pluginName = path.Split("/")[0];
            var pluginFolder = Path.Combine(pluginRepositoryPath, pluginName);

            if (!Directory.Exists(pluginFolder))
                Directory.CreateDirectory(pluginFolder);
                
            var filePath = Path.Combine(pluginFolder, "plugin.properties");
            var fileContent = "";
            var updateContent = "";
                
            if (!File.Exists(filePath) || isUpdateCheck)
            {
                var file = await _gitLabClient.Files.GetAsync($"{GroupName}/{ProjectName}", path, BranchName);
                if (isUpdateCheck && File.Exists(filePath))
                {
                    fileContent = await File.ReadAllTextAsync(filePath);
                    updateContent = file.ContentDecoded;
                } 
                else if (isUpdateCheck)
                    fileContent = updateContent = file.ContentDecoded;
                else
                    fileContent = file.ContentDecoded;
                    
                if (!isUpdateCheck)
                {
                    await File.WriteAllBytesAsync(
                        filePath,
                        Convert.FromBase64String(file.Content)
                    );
                } 
            }
            else if (File.Exists(filePath))
            {
                fileContent = await File.ReadAllTextAsync(filePath);
            }

            var info = ParseInfo(pluginName, fileContent);
            var updatedInfo = !string.IsNullOrEmpty(updateContent) ? ParseInfo(pluginName, updateContent) : info;

            if (isUpdateCheck && updatedInfo.Version != info.Version)
            {
                if (writePersistentUpdateFlag)
                    await File.WriteAllTextAsync(filePath, fileContent + "\r\nUPDATEAVAILABLE=1");
                info.UpdateAvailable = true;
            }
                
            info.Installed = File.Exists(Path.Combine(pluginFolder, "plugin.class"));
            return info;
        }

        public static PluginInfo ParseInfo(string pluginName, string text)
        {
            var lines = new Regex("\r\n|\r|\n").Split(text);
            var parsedData = new Dictionary<string, string>();
            var lastKey = "";
            
            foreach (var lineInfo in lines.Select(t => t.Split("=")))
            {
                if (lineInfo.Length > 1)
                {
                    parsedData.Add (lineInfo[0], lineInfo[1].Trim('\'').Trim());
                    lastKey = lineInfo[0];
                }
                else
                {
                    parsedData[lastKey] += lineInfo[0];
                }
            }

            var info = new PluginInfo(pluginName);
            foreach (var (key, value) in parsedData)
            {
                switch (key.ToLower())
                {
                    case "author":
                        info.Author = value;
                        break;
                    case "description":
                        info.Description = value.Replace("\\", System.Environment.NewLine);
                        break;
                    case "version":
                        info.Version = value;
                        break;
                    case "updateavailable":
                        info.UpdateAvailable = true;
                        break;
                }
            }

            return info;
        }
    }
}