using System.Collections.Generic;
using System.Threading.Tasks;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public interface IGameClientService
{
    Task<int> LaunchLoopbackClient(string clientExecutablePath, int port = 2610);

    IEnumerable<GameClientWindow> GetGameClients();
}