using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitLabApiClient;

namespace Saradomin.Infrastructure.Services
{
    public class PluginDownloadService : IPluginDownloadService
    {
        private const string GroupName = "2009scape";
        private const string ProjectName = "Tools/client-plugins";
        private const string BranchName = "master";

        private readonly GitLabClient _gitLabClient;

        public PluginDownloadService()
        {
            _gitLabClient = new GitLabClient("https://gitlab.com");
        }

        public async Task<List<string>> FetchAvailablePluginNames()
        {
            return (await _gitLabClient.Trees.GetAsync($"{GroupName}/{ProjectName}", o => o.Reference = BranchName))
                .Where(x => x.Type == "tree")
                .Select(x => x.Name)
                .ToList();
        }

        public async Task<List<string>> FetchFileListForPlugin(string pluginName)
        {
            var enumeration = await _gitLabClient.Trees.GetAsync(
                $"{GroupName}/{ProjectName}",
                o =>
                {
                    o.Path = pluginName;
                    o.Recursive = true;
                    o.Reference = BranchName;
                }
            );

            return enumeration
                .Where(x => x.Type == "blob")
                .Select(x => x.Path)
                .ToList();
        }

        public async Task DownloadPluginFiles(string pluginName, string pluginRepositoryPath)
        {
            var directoryPath = Path.Combine(pluginRepositoryPath, pluginName);
            if (Directory.Exists(directoryPath))
            {
                throw new InvalidOperationException("Plugin directory exists.");
            }
            Directory.CreateDirectory(directoryPath);
            
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
    }
}