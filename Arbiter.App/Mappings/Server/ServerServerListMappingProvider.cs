using Arbiter.Net.Server.Messages;

namespace Arbiter.App.Mappings.Server;

public class ServerServerListMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ServerServerListMessage>(b =>
        {
            b.Section("Server List")
                .Property(m => m.Checksum, p => p.ShowHex());

            b.Section("Encryption")
                .Property(m => m.Seed)
                .Property(m => m.PrivateKey, p => p.ShowMultiline());
        });
    }
}