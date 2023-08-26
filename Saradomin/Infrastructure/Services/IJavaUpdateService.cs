using System;
using System.Threading.Tasks;
using Glitonea.Mvvm;

namespace Saradomin.Infrastructure.Services
{
    public interface IJavaUpdateService : IService
    {
        event EventHandler<float> JavaDownloadProgressChanged;
        Task DownloadAndSetJava11(ISettingsService settingsService);
    }
}
