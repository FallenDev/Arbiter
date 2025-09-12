using Arbiter.Net.Server.Messages;

namespace Arbiter.App.Mappings.Server;

public class ServerHelloMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ServerHelloMessage>(b =>
        {
            b.Section("Greeting")
                .Property(m => m.Message, p => p.ShowMultiline());
        });
    }
}