using System;
using System.Threading.Tasks;
using Glitonea.Mvvm;

namespace Saradomin.Infrastructure.Services
{
    public interface ISingleplayerUpdateService : IService
    {
        event EventHandler<Tuple<float, bool>> SingleplayerDownloadProgressChanged;
        Task DownloadSingleplayer();
    }
}
