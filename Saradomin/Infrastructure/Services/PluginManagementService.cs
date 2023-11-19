using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Saradomin.Utilities;

namespace Saradomin.Infrastructure.Services
{
    public class PluginManagementService : IPluginManagementService
    {
        public string PluginRepositoryPath { get; set; }

        public PluginManagementService(ISettingsService settings)
        {
            PluginRepositoryPath = Path.Combine(CrossPlatform.Get2009scapeHome(), "plugins");
        }

        public Task<List<string>> EnumerateInstalledPlugins()
        {
            EnsurePluginRepositoryPathSane();

            return Task.FromResult(Directory
                .GetDirectories(PluginRepositoryPath, "*", SearchOption.TopDirectoryOnly)
                .Select(x => Path.GetFileName(x)).ToList());
        }

        public async Task<bool> IsPluginInstalled(string pluginName)
        {
            EnsurePluginRepositoryPathSane();

            return (await EnumerateInstalledPlugins())
                .Contains(pluginName);
        }

        public Task UninstallPlugin(string pluginName)
        {
            EnsurePluginRepositoryPathSane();
            var pluginPath = GetPluginDirectoryPath(pluginName);

            if (Directory.Exists(pluginPath))
            {
                Directory.Delete(pluginPath, true);
            }

            return Task.CompletedTask;
        }

        public Task InstallPlugin(ZipArchive zipArchive, string pluginName)
        {
            throw new NotSupportedException("Feature not supported yet.");
        }

        private string GetPluginDirectoryPath(string pluginName)
            => Path.Combine(PluginRepositoryPath, pluginName);

        private void EnsurePluginRepositoryPathSane()
        {
            if (string.IsNullOrWhiteSpace(PluginRepositoryPath))
            {
                throw new InvalidOperationException("Plugin repository path has not been set.");
            }

            Directory.CreateDirectory(PluginRepositoryPath);
        }
    }
}