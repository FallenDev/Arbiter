using Arbiter.Net.Server.Messages;

namespace Arbiter.App.Mappings.Server;

public class ServerLoginNoticeMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.Register<ServerLoginNoticeMessage>(b =>
        {
            b.Section("Notice")
                .Property(m => m.Checksum, p => p.ShowHex())
                .Property(m => m.Content, p => p.ShowMultiline());
        });
    }
}