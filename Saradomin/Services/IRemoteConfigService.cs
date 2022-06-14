using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Glitonea.Mvvm;
using Saradomin.Model;

namespace Saradomin.Services
{
    public interface IRemoteConfigService : IService
    {
        ObservableCollection<ServerProfile> AvailableProfiles { get; }
        
        Task FetchServerProfileConfig(string outputPath);
        Task LoadServerProfileConfig(string filePath);
        
        void LoadFailsafeDefaults();
    }
}