using System.Collections.Generic;
using System.Threading.Tasks;
using Glitonea.Mvvm;

namespace Saradomin.Infrastructure.Services
{
    public interface IPluginDownloadService : IService
    {
        Task<List<string>> FetchAvailablePluginNames();
        Task<List<string>> FetchFileListForPlugin(string pluginName);
        Task DownloadPluginFiles(string pluginName, string pluginRepositoryPath);
    }
}