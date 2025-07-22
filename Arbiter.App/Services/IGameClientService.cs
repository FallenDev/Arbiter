using System.Threading.Tasks;

namespace Arbiter.App.Services;

public interface IGameClientService
{
    Task<int> LaunchLoopbackClient(string clientExecutablePath, int port = 2610);
}