using System;
using System.Threading;
using System.Threading.Tasks;
using Glitonea.Mvvm;

namespace Saradomin.Services
{
    public interface IClientUpdateService : IService
    {
        event EventHandler<float> DownloadProgressChanged;
        
        Task<string> FetchRemoteClientHashAsync(CancellationToken cancellationToken);
        Task FetchRemoteClientExecutableAsync(CancellationToken cancellationToken, string targetPath = null);
        Task<string> ComputeLocalClientHashAsync(string filePath = null);
    }
}