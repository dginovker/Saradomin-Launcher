using System.Threading.Tasks;
using Glitonea.Mvvm;

namespace Saradomin.Infrastructure.Services
{
    public interface IClientLaunchService : IService
    {
        Task LaunchClient();
    }
}