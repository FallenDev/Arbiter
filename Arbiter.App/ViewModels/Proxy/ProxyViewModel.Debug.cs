using System.Collections.Generic;
using Arbiter.App.Models;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Proxy;

public partial class ProxyViewModel
{
    private readonly IClientMessageFactory _clientMessageFactory = new ClientMessageFactory();
    private readonly IServerMessageFactory _serverMessageFactory = new ServerMessageFactory();

    public void ApplyDebugFilters(DebugSettings settings, List<MessageFilter> filters)
    {
        RemoveDebugFilters();

        // Ideally, these would all be moved to Lua scripts
        // For now they are done in code for simplicity

        AddDebugEntityFilters(settings);
        AddDebugDialogFilters(settings);
        AddDebugPlayerFilters(settings);
        AddDebugEffectsFilters(settings);
        AddDebugMapFilters(settings);
        AddDebugMessageFilters(settings, filters);
    }

    public void RemoveDebugFilters()
    {
        RemoveDebugEntityFilters();
        RemoveDebugDialogFilters();
        RemoveDebugPlayerFilters();
        RemoveDebugEffectsFilters();
        RemoveDebugMapFilters();
        RemoveDebugMessageFilters();
    }
}