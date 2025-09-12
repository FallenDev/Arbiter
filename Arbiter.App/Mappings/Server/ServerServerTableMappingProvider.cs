using Arbiter.Net.Server.Messages;

namespace Arbiter.App.Mappings.Server;

public class ServerServerTableMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ServerServerTableMessage>(b =>
        {
            b.Section("Servers")
                .Property(m => m.Servers);
        });
    }
}