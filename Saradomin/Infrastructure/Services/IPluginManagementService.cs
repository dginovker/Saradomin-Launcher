using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using Glitonea.Mvvm;

namespace Saradomin.Infrastructure.Services
{
    public interface IPluginManagementService : IService
    {
        string PluginRepositoryPath { get; set; }
        
        Task<List<string>> EnumerateInstalledPlugins();
        Task<bool> IsPluginInstalled(string pluginName);

        Task UninstallPlugin(string pluginName);
        Task InstallPlugin(ZipArchive zipArchive, string pluginName);
    }
}