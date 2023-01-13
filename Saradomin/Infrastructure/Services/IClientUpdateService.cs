using System;
using System.Threading;
using System.Threading.Tasks;
using Glitonea.Mvvm;

namespace Saradomin.Infrastructure.Services
{
    public interface IClientUpdateService : IService
    {
        string ClientDownloadURL { get; }
        string PreferredTargetFilePath { get; }
        
        event EventHandler<float> DownloadProgressChanged;
        
        Task<string> FetchRemoteClientHashAsync(CancellationToken cancellationToken);
        Task FetchRemoteClientExecutableAsync(CancellationToken cancellationToken, string targetPath = null);
        Task<string> ComputeLocalClientHashAsync(string filePath = null);
    }
}