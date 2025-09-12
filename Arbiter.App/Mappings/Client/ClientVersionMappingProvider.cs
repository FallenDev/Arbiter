using Arbiter.Net.Client.Messages;

namespace Arbiter.App.Mappings.Client;

public class ClientVersionMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ClientVersionMessage>(b =>
        {
            b.Section("Version")
                .Property(m => m.Version)
                .Property(m => m.Checksum, p => p.ShowHex());
        });
    }
}