using Arbiter.Net.Server.Messages;

namespace Arbiter.App.Mappings.Server;

public class ServerServerInfoMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ServerServerInfoMessage>(b =>
        {
            b.Section("Information")
                .Property(m => m.DataType)
                .Property(m => m.Value, p => p.ShowMultiline());
        });
    }
}