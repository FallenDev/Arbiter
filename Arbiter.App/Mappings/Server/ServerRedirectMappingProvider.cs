using Arbiter.Net.Server.Messages;

namespace Arbiter.App.Mappings.Server;

public class ServerRedirectMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ServerRedirectMessage>(b =>
        {
            b.Section("Server")
                .Property(m => m.Address)
                .Property(m => m.Port);

            b.Section("Encryption")
                .Property(m => m.Seed)
                .Property(m => m.PrivateKey, p => p.ShowMultiline());

            b.Section("Connection")
                .Property(m => m.ConnectionId, p => p.ShowHex())
                .Property(m => m.Name);
        });
    }
}