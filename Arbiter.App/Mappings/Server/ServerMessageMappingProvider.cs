using Arbiter.Net.Server.Messages;

namespace Arbiter.App.Mappings.Server;

public class ServerMessageMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        RegisterServerHelloMapping(registry);
        RegisterServerLoginNoticeMapping(registry);
        RegisterServerRedirectMapping(registry);
        RegisterServerServerInfoMapping(registry);
        RegisterServerServerListMapping(registry);
        RegisterServerServerTableMapping(registry);
    }

    private static void RegisterServerHelloMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerHelloMessage>(b =>
        {
            b.Section("Greeting")
                .Property(m => m.Message, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerLoginNoticeMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerLoginNoticeMessage>(b =>
        {
            b.Section("Notice")
                .Property(m => m.Checksum, p => p.ShowHex())
                .Property(m => m.Content, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerRedirectMapping(InspectorMappingRegistry registry)
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

    private static void RegisterServerServerInfoMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerServerInfoMessage>(b =>
        {
            b.Section("Information")
                .Property(m => m.DataType)
                .Property(m => m.Value, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerServerListMapping(InspectorMappingRegistry registry)
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

    private static void RegisterServerServerTableMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerServerTableMessage>(b =>
        {
            b.Section("Servers")
                .Property(m => m.Servers);
        });
    }
}