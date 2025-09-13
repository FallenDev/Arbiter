using Arbiter.Net.Client.Messages;

namespace Arbiter.App.Mappings.Client;

public class ClientMessageMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        RegisterClientAssailMapping(registry);
        RegisterClientAuthenticateMapping(registry);
        RegisterClientChangePasswordMapping(registry);
        RegisterClientCreateCharacterNameMapping(registry);
        RegisterClientDropItemMapping(registry);
        RegisterClientHeartbeatMapping(registry);
        RegisterClientLoginMapping(registry);
        RegisterClientPickupItemMapping(registry);
        RegisterClientRequestExitMapping(registry);
        RegisterClientRequestHomepageMapping(registry);
        RegisterClientRequestLoginNoticeMapping(registry);
        RegisterClientRequestMetadataMapping(registry);
        RegisterClientRequestProfileMapping(registry);
        RegisterClientRequestSequenceMapping(registry);
        RegisterClientRequestServerTableMapping(registry);
        RegisterClientTurnMapping(registry);
        RegisterClientUserPortraitMapping(registry);
        RegisterClientVersionMapping(registry);
        RegisterClientWalkMapping(registry);
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

    private static void RegisterClientCreateCharacterNameMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientCreateCharacterNameMessage>(b =>
        {
            b.Section("Credentials")
                .Property(m => m.Name)
                .Property(m => m.Password, p => p.Mask())
                .Property(m => m.Email);
        });
    }

    private static void RegisterClientDropItemMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientDropItemMessage>(b =>
        {
            b.Section("Item")
                .Property(m => m.Slot)
                .Property(m => m.Quantity);

            b.Section("Position")
                .Property(m => m.X)
                .Property(m => m.Y);
        });
    }

    private static void RegisterClientHeartbeatMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientHeartbeatMessage>(b =>
        {
            b.Section("Heartbeat")
                .Property(m => m.Reply, p => p.ShowHex());
        });
    }

    private static void RegisterClientLoginMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientLoginMessage>(b =>
        {
            b.Section("Credentials")
                .Property(m => m.Name)
                .Property(m => m.Password, p => p.Mask());

            b.Section("Client")
                .Property(m => m.ClientId)
                .Property(m => m.Checksum, p => p.ShowHex());
        });
    }

    private static void RegisterClientPickupItemMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientPickupItemMessage>(b =>
        {
            b.Section("Item")
                .Property(m => m.Slot);

            b.Section("Position")
                .Property(m => m.X)
                .Property(m => m.Y);
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

    private static void RegisterClientRequestLoginNoticeMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestLoginNoticeMessage>(b =>
        {
            // No mappings
        });
    }

    private static void RegisterClientRequestMetadataMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestMetadataMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.RequestType)
                .Property(m => m.Name);
        });
    }

    private static void RegisterClientRequestProfileMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestProfileMessage>(b =>
        {
            // No mappings
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

    private static void RegisterClientTurnMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientTurnMessage>(b =>
        {
            b.Section("Movement")
                .Property(m => m.Direction);
        });
    }

    private static void RegisterClientUserPortraitMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientUserPortraitMessage>(b =>
        {
            b.Section("Portrait")
                .Property(m => m.Portrait, p => p.ShowMultiline());
            
            b.Section("Bio")
                .Property(m => m.Bio, p => p.ShowMultiline());
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

    private static void RegisterClientWalkMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientWalkMessage>(b =>
        {
            b.Section("Movement")
                .Property(m => m.Direction)
                .Property(m => m.StepCount);
        });
    }
}