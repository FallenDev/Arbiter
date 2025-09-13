using Arbiter.Net.Client.Messages;

namespace Arbiter.App.Mappings.Client;

public class ClientMessageMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        RegisterClientAssailMapping(registry);
        RegisterClientAuthenticateMapping(registry);
        RegisterClientChangePasswordMapping(registry);
        RegisterClientRequestExitMapping(registry);
        RegisterClientRequestHomepageMapping(registry);
        RegisterClientRequestSequenceMapping(registry);
        RegisterClientRequestServerTableMapping(registry);
        RegisterClientVersionMapping(registry);
    }

    private static void RegisterClientAssailMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientAssailMessage>(b =>
        {
            // No mappings
        });
    }
    
    private static void RegisterClientAuthenticateMapping(InspectorMappingRegistry registry)
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
    
    private static void RegisterClientChangePasswordMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientChangePasswordMessage>(b =>
        {
            b.Section("Credentials")
                .Property(m => m.Name)
                .Property(m => m.CurrentPassword, p => p.Mask())
                .Property(m => m.NewPassword, p => p.Mask());
        });
    }
    
    private static void RegisterClientRequestExitMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestExitMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.Reason);
        });
    }
    
    private static void RegisterClientRequestHomepageMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestHomepageMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.NeedsHomepage);
        });
    }
    
    private static void RegisterClientRequestSequenceMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestSequenceMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.Sequence, p => p.ShowHex())
                .Property(m => m.Unknown, p => p.ShowHex());
        });
    }

    private static void RegisterClientRequestServerTableMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestServerTableMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.NeedsServerTable);
        });
    }
    
    private static void RegisterClientVersionMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientVersionMessage>(b =>
        {
            b.Section("Client Version")
                .Property(m => m.Version)
                .Property(m => m.Checksum, p => p.ShowHex());
        });
    }
}