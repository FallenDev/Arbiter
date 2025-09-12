using Arbiter.Net.Client.Messages;

namespace Arbiter.App.Mappings.Client;

public class ClientRequestSequenceMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestSequenceMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.Sequence, p => p.ShowHex())
                .Property(m => m.Unknown, p => p.ShowHex());
        });
    }
}