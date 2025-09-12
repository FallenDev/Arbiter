using Arbiter.Net.Client.Messages;

namespace Arbiter.App.Mappings.Client;

public class ClientRequestHomepageMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestHomepageMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.NeedsHomepage);
        });
    }
}