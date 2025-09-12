using Arbiter.Net.Client.Messages;

namespace Arbiter.App.Mappings.Client;

public class ClientRequestServerTableMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestServerTableMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.NeedsServerTable);
        });   
    }
}