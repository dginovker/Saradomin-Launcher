using System.Threading.Tasks;
using Glitonea.Mvvm;

namespace Saradomin.Services
{
    public interface IClientLaunchService : IService
    {
        Task LaunchClient();
    }
}