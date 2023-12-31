using Glitonea.Mvvm;
using Saradomin.Model.Settings.Client;
using Saradomin.Model.Settings.Launcher;

namespace Saradomin.Infrastructure.Services
{
    public interface ISettingsService : IService
    {
        LauncherSettings Launcher { get; }
        ClientSettings Client { get; }

        void SaveAll();
    }
}