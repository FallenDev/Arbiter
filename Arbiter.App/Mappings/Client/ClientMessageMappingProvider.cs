using Arbiter.Net.Client.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.Mappings.Client;

public class ClientMessageMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        RegisterClientAssailMapping(registry);
        RegisterClientAuthenticateMapping(registry);
        RegisterClientBeginSpellCastMapping(registry);
        RegisterClientBoardActionMapping(registry);
        RegisterClientCastSpellMapping(registry);
        RegisterClientChangePasswordMapping(registry);
        RegisterClientCreateCharacterAppearance(registry);
        RegisterClientCreateCharacterNameMapping(registry);
        RegisterClientDialogChoiceMapping(registry);
        RegisterClientDropGoldMapping(registry);
        RegisterClientDropItemMapping(registry);
        RegisterClientEditNotepadMapping(registry);
        RegisterClientEmoteMapping(registry);
        RegisterClientExceptionMapping(registry);
        RegisterClientExchangeActionMapping(registry);
        RegisterClientGiveGoldMapping(registry);
        RegisterClientGiveItemMapping(registry);
        RegisterClientGroupInviteMapping(registry);
        RegisterClientHeartbeatMapping(registry);
        RegisterClientIgnoreUserMapping(registry);
        RegisterClientInteractMapping(registry);
        RegisterClientLoginMapping(registry);
        RegisterClientMenuChoiceMapping(registry);
        RegisterClientPickupItemMapping(registry);
        RegisterClientRaiseStatMapping(registry);
        RegisterClientRequestEntityMapping(registry);
        RegisterClientRequestExitMapping(registry);
        RegisterClientRequestHomepageMapping(registry);
        RegisterClientRequestLoginNoticeMapping(registry);
        RegisterClientRequestMetadataMapping(registry);
        RegisterClientRequestProfileMapping(registry);
        RegisterClientRequestSequenceMapping(registry);
        RegisterClientRequestServerTableMapping(registry);
        RegisterClientSayMapping(registry);
        RegisterClientSetStatusMapping(registry);
        RegisterClientSpellChantMapping(registry);
        RegisterClientSwapSlotMapping(registry);
        RegisterClientSyncTicksMapping(registry);
        RegisterClientToggleSettingMapping(registry);
        RegisterClientTurnMapping(registry);
        RegisterClientUnequipItemMapping(registry);
        RegisterClientUseItem(registry);
        RegisterClientUserPortraitMapping(registry);
        RegisterClientUseSkillMapping(registry);
        RegisterClientVersionMapping(registry);
        RegisterClientWalkMapping(registry);
        RegisterClientWhisperMapping(registry);
        RegisterClientWorldMapClickMapping(registry);
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

    private static void RegisterClientBeginSpellCastMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientBeginSpellCastMessage>(b =>
        {
            b.Section("Spell")
                .Property(m => m.LineCount);
        });
    }

    private static void RegisterClientBoardActionMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientBoardActionMessage>(b =>
        {
            b.Section("Action")
                .Property(m => m.Action);
            b.Section("Board")
                .Property(m => m.BoardId)
                .Property(m => m.StartPostId)
                .IsExpanded(m => m.BoardId.HasValue && m.Action != MessageBoardAction.SendMail);
            b.Section("Post")
                .Property(m => m.PostId)
                .IsExpanded(m => m.PostId.HasValue);
            b.Section("Navigation")
                .Property(m => m.Navigation)
                .IsExpanded(m => m.Action == MessageBoardAction.ViewPost);
            b.Section("Create Post")
                .Property(m => m.Subject, p => p.ShowMultiline())
                .Property(m => m.Body, p => p.ShowMultiline())
                .IsExpanded(m => m.Action == MessageBoardAction.CreatePost);
            b.Section("Send Mail")
                .Property(m => m.Recipient)
                .Property(m => m.MailSubject, p => p.ShowMultiline())
                .Property(m => m.MailBody, p => p.ShowMultiline())
                .IsExpanded(m => m.Action == MessageBoardAction.SendMail);
            b.Section("Unknown")
                .Property(m => m.Unknown, p => p.ShowHex())
                .IsExpanded(m => m.Action == MessageBoardAction.ViewBoard);
        });
    }

    private static void RegisterClientCastSpellMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientCastSpellMessage>(b =>
        {
            b.Section("Spell")
                .Property(m => m.Slot);
            b.Section("Target")
                .Property(m => m.TargetId, p => p.ShowHex())
                .Property(m => m.TargetX)
                .Property(m => m.TargetY)
                .IsExpanded(m => m.TargetId.HasValue);
            b.Section("Text Input")
                .Property(m => m.TextInput, p => p.ShowMultiline())
                .IsExpanded(m => !string.IsNullOrEmpty(m.TextInput));
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

    private static void RegisterClientCreateCharacterAppearance(InspectorMappingRegistry registry)
    {
        registry.Register<ClientCreateCharacterAppearanceMessage>(b =>
        {
            b.Section("Appearance")
                .Property(m => m.HairStyle)
                .Property(m => m.Gender)
                .Property(m => m.HairColor);
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

    private static void RegisterClientDialogChoiceMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientDialogChoiceMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityType)
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Dialog")
                .Property(m => m.DialogId)
                .Property(m => m.PursuitId);
            b.Section("Arguments")
                .Property(m => m.ArgsType)
                .Property(m => m.MenuChoice)
                .Property(m => m.TextInputs, p => p.ShowMultiline())
                .IsExpanded(m => m.ArgsType != DialogArgsType.None);
        });
    }

    private static void RegisterClientDropGoldMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientDropGoldMessage>(b =>
        {
            b.Section("Gold")
                .Property(m => m.Amount);
            b.Section("Position")
                .Property(m => m.X)
                .Property(m => m.Y);
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

    private static void RegisterClientEditNotepadMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientEditNotepadMessage>(b =>
        {
            b.Section("Item")
                .Property(m => m.Slot);
            b.Section("Content")
                .Property(m => m.Content, p => p.ShowMultiline());
        });
    }

    private static void RegisterClientEmoteMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientEmoteMessage>(b =>
        {
            b.Section("Emote")
                .Property(m => m.Emote);
        });
    }

    private static void RegisterClientExceptionMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientExceptionMessage>(b =>
        {
            b.Section("Exception")
                .Property(m => m.Message, p => p.ShowMultiline());
        });
    }

    private static void RegisterClientExchangeActionMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientExchangeActionMessage>(b =>
        {
            b.Section("Action")
                .Property(m => m.Action);
            b.Section("Target")
                .Property(m => m.TargetId, p => p.ShowHex());
            b.Section("Item")
                .Property(m => m.Slot)
                .Property(m => m.Quantity)
                .IsExpanded(m =>
                    m.Action is ExchangeClientActionType.AddItem or ExchangeClientActionType.AddStackableItem);
            b.Section("Gold")
                .Property(m => m.GoldAmount)
                .IsExpanded(m => m.Action == ExchangeClientActionType.SetGold);
        });
    }
    
    private static void RegisterClientGiveGoldMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientGiveGoldMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Gold")
                .Property(m => m.Amount);
        });
    }
    
    private static void RegisterClientGiveItemMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientGiveItemMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Item")
                .Property(m => m.Slot)
                .Property(m => m.Quantity);
        });
    }

    private static void RegisterClientGroupInviteMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientGroupInviteMessage>(b =>
        {
            b.Section("Action")
                .Property(m => m.Action);
            b.Section("Target")
                .Property(m => m.TargetName);
            b.Section("Group Box")
                .Property(m => m.GroupBox)
                .IsExpanded(m => m.GroupBox is not null);
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

    private static void RegisterClientIgnoreUserMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientIgnoreUserMessage>(b =>
        {
            b.Section("Ignore")
                .Property(m => m.Action)
                .Property(m => m.Name);
        });
    }

    private static void RegisterClientInteractMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientInteractMessage>(b =>
        {
            b.Section("Interaction")
                .Property(m => m.InteractionType);
            b.Section("Target Entity")
                .Property(m => m.TargetId, p => p.ShowHex())
                .IsExpanded(m => m.InteractionType == InteractionType.Entity);
            b.Section("Target Tile")
                .Property(m => m.TargetX)
                .Property(m => m.TargetY)
                .IsExpanded(m => m.InteractionType == InteractionType.Tile);
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

    private static void RegisterClientMenuChoiceMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientMenuChoiceMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityType)
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Menu")
                .Property(m => m.PursuitId);
            b.Section("Menu Choice")
                .Property(m => m.MenuChoice)
                .IsExpanded(m => m.MenuChoice.HasValue);
            b.Section("Text Input")
                .Property(m => m.TextInputs, p => p.ShowMultiline())
                .IsExpanded(m => m.TextInputs.Count > 0);
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

    private static void RegisterClientRaiseStatMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRaiseStatMessage>(b =>
        {
            b.Section("Stat")
                .Property(m => m.Stat);
        });
    }

    private static void RegisterClientRequestEntityMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientRequestEntityMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
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

    private static void RegisterClientSayMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientSayMessage>(b =>
        {
            b.Section("Message")
                .Property(m => m.MessageType)
                .Property(m => m.Content, p => p.ShowMultiline());
        });
    }

    private static void RegisterClientSetStatusMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientSetStatusMessage>(b =>
        {
            b.Section("Status")
                .Property(m => m.Status);
        });
    }

    private static void RegisterClientSpellChantMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientSpellChantMessage>(b =>
        {
            b.Section("Message")
                .Property(m => m.Content);
        });
    }

    private static void RegisterClientSwapSlotMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientSwapSlotMessage>(b =>
        {
            b.Section("Interface")
                .Property(m => m.Pane);
            b.Section("Slot")
                .Property(m => m.SourceSlot)
                .Property(m => m.TargetSlot);
        });
    }

    private static void RegisterClientSyncTicksMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientSyncTicksMessage>(b =>
        {
            b.Section("Ticks")
                .Property(m => m.ClientTickCount)
                .Property(m => m.ServerTickCount);
        });
    }

    private static void RegisterClientToggleSettingMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientToggleSettingMessage>(b =>
        {
            b.Section("Setting")
                .Property(m => m.OptionIndex);
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

    private static void RegisterClientUnequipItemMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientUnequipItemMessage>(b =>
        {
            b.Section("Equipment")
                .Property(m => m.Slot);
        });   
    }

    private static void RegisterClientUseItem(InspectorMappingRegistry registry)
    {
        registry.Register<ClientUseItemMessage>(b =>
        {
            b.Section("Item")
                .Property(m => m.Slot);
        });
    }

    private static void RegisterClientUserPortraitMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientUserPortraitMessage>(b =>
        {
            b.Section("Portrait")
                .Property(m => m.Portrait, p => p.ShowMultiline())
                .IsExpanded(m => m.Portrait.Count > 0);
            b.Section("Bio")
                .Property(m => m.Bio, p => p.ShowMultiline())
                .IsExpanded(m => !string.IsNullOrEmpty(m.Bio));
        });
    }

    private static void RegisterClientUseSkillMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientUseSkillMessage>(b =>
        {
            b.Section("Skill")
                .Property(m => m.Slot);
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
    
    private static void RegisterClientWhisperMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientWhisperMessage>(b =>
        {
            b.Section("Whisper")
                .Property(m => m.Target)
                .Property(m => m.Content, p => p.ShowMultiline());
        });
    }
    
    private static void RegisterClientWorldMapClickMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ClientWorldMapClickMessage>(b =>
        {
            b.Section("Location")
                .Property(m => m.MapId)
                .Property(m => m.X)
                .Property(m => m.Y);
            b.Section("Checksum")
                .Property(m => m.Checksum, p => p.ShowHex());
        });
    }
}