using Arbiter.Net.Client.Messages;

namespace Arbiter.App.Mappings.Client;

public class ClientAuthenticateMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ClientAuthenticateMessage>(b =>
        {
            b.Section("Connection")
                .Property(m => m.ConnectionId, p => p.ShowHex())
                .Property(m => m.Name);

            b.Section("Encryption")
                .Property(m => m.Seed)
                .Property(m => m.PrivateKey, p => p.ShowMultiline());
        });
    }
}