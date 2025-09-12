using Arbiter.Net.Client.Messages;

namespace Arbiter.App.Mappings.Client;

public class ClientRequestExitMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestExitMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.Reason);
        });
    }
}