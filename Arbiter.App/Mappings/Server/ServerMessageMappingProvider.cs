using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Types;

namespace Arbiter.App.Mappings.Server;

public class ServerMessageMappingProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        RegisterServerAddEntityMapping(registry);
        RegisterServerAddItemMapping(registry);
        RegisterServerAddSkillMapping(registry);
        RegisterServerAddSpellMapping(registry);
        RegisterServerAnimateEntityMapping(registry);
        RegisterServerCooldownMapping(registry);
        RegisterServerEntityTurnMapping(registry);
        RegisterServerEntityWalkMapping(registry);
        RegisterServerExitResponseMapping(registry);
        RegisterServerGroupMapping(registry);
        RegisterServerHealthBarMapping(registry);
        RegisterServerHeartbeatMapping(registry);
        RegisterServerHelloMapping(registry);
        RegisterServerLightLevelMapping(registry);
        RegisterServerLoginNoticeMapping(registry);
        RegisterServerLoginResultMapping(registry);
        RegisterServerMapChangedMapping(registry);
        RegisterServerMapChangingMapping(registry);
        RegisterServerMapDoorMapping(registry);
        RegisterServerMapInfoMapping(registry);
        RegisterServerMapLocationMapping(registry);
        RegisterServerMapTransferCompleteMapping(registry);
        RegisterServerMapTransferMapping(registry);
        RegisterServerMetadataMapping(registry);
        RegisterServerPlaySoundMapping(registry);
        RegisterServerPublicMessageMapping(registry);
        RegisterServerRedirectMapping(registry);
        RegisterServerRefreshCompletedMapping(registry);
        RegisterServerRemoveEntityMapping(registry);
        RegisterServerRemoveEquipmentMapping(registry);
        RegisterServerRemoveItemMapping(registry);
        RegisterServerRemoveSkillMapping(registry);
        RegisterServerRemoveSpellMapping(registry);
        RegisterServerRequestUserPortraitMapping(registry);
        RegisterServerSelfProfileMapping(registry);
        RegisterServerServerInfoMapping(registry);
        RegisterServerServerListMapping(registry);
        RegisterServerServerTableMapping(registry);
        RegisterServerSetEquipmentMapping(registry);
        RegisterServerShowDialogMapping(registry);
        RegisterServerShowEffectMapping(registry);
        RegisterServerShowMenuMapping(registry);
        RegisterServerShowNotepadMapping(registry);
        RegisterServerShowPlayerMapping(registry);
        RegisterServerStatusEffectMapping(registry);
        RegisterServerSwitchPaneMapping(registry);
        RegisterServerUpdateStatsMapping(registry);
        RegisterServerUserIdMapping(registry);
        RegisterServerUserProfileMapping(registry);
        RegisterServerWalkResponseMapping(registry);
        RegisterServerWorldMapMapping(registry);
        RegisterServerWorldMessageMapping(registry);
    }

    private static void RegisterServerAddEntityMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerAddEntityMessage>(b =>
        {
            b.Section("Entities")
                .Property(m => m.Entities);
        });
    }

    private static void RegisterServerAddItemMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerAddItemMessage>(b =>
        {
            b.Section("Item")
                .Property(m => m.Slot)
                .Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.DyeColor)
                .Property(m => m.Name, p => p.ShowMultiline());
            b.Section("Quantity")
                .Property(m => m.Quantity)
                .Property(m => m.IsStackable)
                .IsExpanded(m => m.Quantity > 1);
            b.Section("Durability")
                .Property(m => m.Durability)
                .Property(m => m.MaxDurability)
                .IsExpanded(m => m.MaxDurability > 0);
        });
    }

    private static void RegisterServerAddSkillMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerAddSkillMessage>(b =>
        {
            b.Section("Skill")
                .Property(m => m.Slot)
                .Property(m => m.Icon)
                .Property(m => m.Name, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerAddSpellMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerAddSpellMessage>(b =>
        {
            b.Section("Spell")
                .Property(m => m.Slot)
                .Property(m => m.Icon)
                .Property(m => m.Name, p => p.ShowMultiline())
                .Property(m => m.CastLines);
            b.Section("Target")
                .Property(m => m.TargetType)
                .Property(m => m.Prompt, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerAnimateEntityMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerAnimateEntityMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Animation")
                .Property(m => m.Animation)
                .Property(m => m.Speed);
            b.Section("Sound")
                .Property(m => m.Sound)
                .IsExpanded(m => m.Sound != 0xFF);
        });
    }

    private static void RegisterServerCooldownMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerCooldownMessage>(b =>
        {
            b.Section("Cooldown")
                .Property(m => m.Type)
                .Property(m => m.Slot)
                .Property(m => m.Seconds);
        });
    }

    private static void RegisterServerEntityTurnMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerEntityTurnMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Movement")
                .Property(m => m.Direction);
        });
    }

    private static void RegisterServerEntityWalkMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerEntityWalkMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Position")
                .Property(m => m.PreviousX)
                .Property(m => m.PreviousY);
            b.Section("Movement")
                .Property(m => m.Direction);
            b.Section("Unknown")
                .Property(m => m.Unknown, p => p.ShowHex());
        });
    }

    private static void RegisterServerExitResponseMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerExitResponseMessage>(b =>
        {
            b.Section("Response")
                .Property(m => m.Result)
                .Property(m => m.Unknown, p => p.ShowHex());
        });
    }

    private static void RegisterServerGroupMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerGroupMessage>(b =>
        {
            b.Section("Action")
                .Property(m => m.Action);
            b.Section("User")
                .Property(m => m.Name);
            b.Section("Group Box")
                .Property(m => m.GroupBox);
        });
    }

    private static void RegisterServerHealthBarMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerHealthBarMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Health Bar")
                .Property(m => m.Percent);
            b.Section("Sound")
                .Property(m => m.Sound)
                .IsExpanded(m => m.Sound != 0xFF);
            b.Section("Unknown")
                .Property(m => m.Unknown, p => p.ShowHex());
        });
    }

    private static void RegisterServerHeartbeatMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerHeartbeatMessage>(b =>
        {
            b.Section("Heartbeat")
                .Property(m => m.Request, p => p.ShowHex());
        });
    }

    private static void RegisterServerHelloMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerHelloMessage>(b =>
        {
            b.Section("Greeting")
                .Property(m => m.Message, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerLightLevelMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerLightLevelMessage>(b =>
        {
            b.Section("Light Level")
                .Property(m => m.Brightness);
            b.Section("Unknown")
                .Property(m => m.Unknown, p => p.ShowHex());
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

    private static void RegisterServerLoginResultMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerLoginResultMessage>(b =>
        {
            b.Section("Login")
                .Property(m => m.MessageType)
                .Property(m => m.Message, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerMapChangedMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerMapChangedMessage>(b =>
        {
            b.Section("Map")
                .Property(m => m.Result);
        });
    }

    private static void RegisterServerMapChangingMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerMapChangingMessage>(b =>
        {
            b.Section("Map")
                .Property(m => m.ChangeType);
            b.Section("Unknown")
                .Property(m => m.Unknown, p => p.ShowHex());
        });
    }

    private static void RegisterServerMapDoorMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerMapDoorMessage>(b =>
        {
            b.Section("Doors")
                .Property(m => m.Doors);
        });
    }

    private static void RegisterServerMapInfoMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerMapInfoMessage>(b =>
        {
            b.Section("Map")
                .Property(m => m.MapId)
                .Property(m => m.Name)
                .Property(m => m.Checksum, p => p.ShowHex());
            b.Section("Dimensions")
                .Property(m => m.Width)
                .Property(m => m.Height);
            b.Section("Weather")
                .Property(m => m.Weather);
        });
    }

    private static void RegisterServerMapLocationMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerMapLocationMessage>(b =>
        {
            b.Section("Position")
                .Property(m => m.X)
                .Property(m => m.Y);
            b.Section("Unknown")
                .Property(m => m.UnknownX, p => p.ShowHex())
                .Property(m => m.UnknownY, p => p.ShowHex());
        });
    }

    private static void RegisterServerMapTransferCompleteMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerMapTransferCompleteMessage>(b =>
        {
            b.Section("Result")
                .Property(m => m.Result);
        });
    }

    private static void RegisterServerMapTransferMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerMapTransferMessage>(b =>
        {
            b.Section("Row Index")
                .Property(m => m.RowY);
            b.Section("Data")
                .Property(m => m.Data, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerMetadataMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerMetadataMessage>(b =>
        {
            b.Section("Response")
                .Property(m => m.ResponseType);
            b.Section("Metadata")
                .Property(m => m.Name)
                .Property(m => m.Checksum, p => p.ShowHex())
                .Property(m => m.Data, p => p.ShowMultiline())
                .IsExpanded(m => m.ResponseType == ServerMetadataResponseType.Metadata);
            b.Section("Listing")
                .Property(m => m.MetadataFiles)
                .IsExpanded(m => m.ResponseType == ServerMetadataResponseType.Listing);
        });
    }

    private static void RegisterServerPlaySoundMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerPlaySoundMessage>(b =>
        {
            b.Section("Sound")
                .Property(m => m.Sound)
                .IsExpanded(m => m.Sound != 0xFF);
            b.Section("Music")
                .Property(m => m.Track)
                .Property(m => m.Unknown, p => p.ShowHex())
                .IsExpanded(m => m.Sound == 0xFF);
        });
    }

    private static void RegisterServerPublicMessageMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerPublicMessageMessage>(b =>
        {
            b.Section("Message")
                .Property(m => m.MessageType)
                .Property(m => m.SenderId, p => p.ShowHex())
                .Property(m => m.Message, p => p.ShowMultiline());
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

    private static void RegisterServerRefreshCompletedMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerRefreshCompleteMessage>(b =>
        {
            // Nothing to map
        });
    }

    private static void RegisterServerRemoveEntityMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerRemoveEntityMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
        });
    }

    private static void RegisterServerRemoveEquipmentMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerRemoveEquipmentMessage>(b =>
        {
            b.Section("Equipment")
                .Property(m => m.Slot);
        });
    }

    private static void RegisterServerRemoveItemMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerRemoveItemMessage>(b =>
        {
            b.Section("Item")
                .Property(m => m.Slot);
        });
    }

    private static void RegisterServerRemoveSkillMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerRemoveSkillMessage>(b =>
        {
            b.Section("Skill")
                .Property(m => m.Slot);
        });
    }

    private static void RegisterServerRemoveSpellMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerRemoveSpellMessage>(b =>
        {
            b.Section("Spell")
                .Property(m => m.Slot);
        });
    }

    private static void RegisterServerRequestUserPortraitMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerRequestUserPortraitMessage>(b =>
        {
            b.Section("Request")
                .Property(m => m.Unknown, p => p.ShowHex());
        });
    }

    private static void RegisterServerSelfProfileMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerSelfProfileMessage>(b =>
        {
            b.Section("Profile")
                .Property(m => m.Class)
                .Property(m => m.DisplayClass)
                .Property(m => m.Guild)
                .Property(m => m.GuildRank)
                .Property(m => m.Title);
            b.Section("Citizenship")
                .Property(m => m.Nation);
            b.Section("Group")
                .Property(m => m.IsGroupOpen)
                .Property(m => m.HasGroupInvite)
                .Property(m => m.GroupMembers, p => p.ShowMultiline());
            b.Section("Group Box")
                .Property(m => m.GroupBox, p => p.ShowMultiline())
                .IsExpanded(m => m.HasGroupInvite);
            b.Section("Metadata")
                .Property(m => m.ShowMasterMetadata)
                .Property(m => m.ShowAbilityMetadata);
            b.Section("Legend")
                .Property(m => m.LegendMarks)
                .IsExpanded(_ => false);
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

    private static void RegisterServerSetEquipmentMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerSetEquipmentMessage>(b =>
        {
            b.Section("Equipment")
                .Property(m => m.Slot)
                .Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.DyeColor)
                .Property(m => m.Name, p => p.ShowMultiline());
            b.Section("Durability")
                .Property(m => m.Durability)
                .Property(m => m.MaxDurability)
                .IsExpanded(m => m.MaxDurability > 0);
        });
    }

    private static void RegisterServerShowDialogMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerShowDialogMessage>(b =>
        {
            b.Section("Dialog")
                .Property(m => m.DialogType)
                .Property(m => m.DialogId)
                .Property(m => m.PursuitId);
            b.Section("Source")
                .Property(m => m.EntityType)
                .Property(m => m.SourceId, p => p.ShowHex())
                .Property(m => m.Name)
                .IsExpanded(m => m.DialogType != DialogType.CloseDialog);
            b.Section("Content")
                .Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.Color)
                .Property(m => m.Content, p => p.ShowMultiline())
                .Property(m => m.ShowGraphic)
                .IsExpanded(m => m.DialogType != DialogType.CloseDialog);
            b.Section("Menu Choices")
                .Property(m => m.MenuChoices)
                .IsExpanded(m => m.DialogType is DialogType.Menu or DialogType.CreatureMenu);
            b.Section("Text Input")
                .Property(m => m.InputPrompt)
                .Property(m => m.InputMaxLength)
                .IsExpanded(m => m.DialogType == DialogType.TextInput);
            b.Section("Navigation")
                .Property(m => m.HasPreviousButton)
                .Property(m => m.HasNextButton)
                .IsExpanded(m => m.DialogType != DialogType.CloseDialog);
            b.Section("Unknown")
                .Property(m => m.Unknown1, p => p.ShowHex())
                .Property(m => m.Unknown2, p => p.ShowHex())
                .IsExpanded(m => m.DialogType != DialogType.CloseDialog);
        });
    }

    private static void RegisterServerShowEffectMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerShowEffectMessage>(b =>
        {
            b.Section("Target Entity")
                .Property(m => m.TargetId, p => p.ShowHex())
                .IsExpanded(m => m.TargetId != 0);
            b.Section("Target Location")
                .Property(m => m.TargetX)
                .Property(m => m.TargetY)
                .IsExpanded(m => m.TargetId == 0);
            b.Section("Source Entity")
                .Property(m => m.SourceId, p => p.ShowHex())
                .IsExpanded(m => m.TargetId != 0);
            b.Section("Animation")
                .Property(m => m.TargetAnimation)
                .Property(m => m.SourceAnimation)
                .Property(m => m.AnimationSpeed);
        });
    }

    private static void RegisterServerShowMenuMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerShowMenuMessage>(b =>
        {
            b.Section("Menu")
                .Property(m => m.MenuType)
                .Property(m => m.PursuitId);
            b.Section("Source")
                .Property(m => m.EntityType)
                .Property(m => m.SourceId, p => p.ShowHex())
                .Property(m => m.Name);
            b.Section("Content")
                .Property(m => m.Sprite, p => p.ShowHex())
                .Property(m => m.Color)
                .Property(m => m.Content, p => p.ShowMultiline())
                .Property(m => m.ShowGraphic);
            b.Section("Arguments")
                .Property(m => m.Args, p => p.ShowMultiline())
                .IsExpanded(m => m.MenuType is DialogMenuType.MenuWithArgs or DialogMenuType.TextInputWithArgs);
            b.Section("Menu Choices")
                .Property(m => m.MenuChoices)
                .IsExpanded(m => m.MenuType is DialogMenuType.Menu or DialogMenuType.MenuWithArgs);
            b.Section("Item Choices")
                .Property(m => m.ItemChoices)
                .IsExpanded(m => m.MenuType == DialogMenuType.ItemChoices);
            b.Section("Skill Choices")
                .Property(m => m.SkillChoices)
                .IsExpanded(m => m.MenuType == DialogMenuType.SkillChoices);
            b.Section("Spell Choices")
                .Property(m => m.SpellChoices)
                .IsExpanded(m => m.MenuType == DialogMenuType.SpellChoices);
            b.Section("Player Inventory")
                .Property(m => m.InventorySlots)
                .IsExpanded(m => m.MenuType == DialogMenuType.PlayerInventory);
            b.Section("Unknown")
                .Property(m => m.Unknown1, p => p.ShowHex())
                .Property(m => m.Unknown2, p => p.ShowHex());
        });
    }

    private static void RegisterServerShowNotepadMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerShowNotepadMessage>(b =>
        {
            b.Section("Notepad")
                .Property(m => m.Slot)
                .Property(m => m.Style);
            b.Section("Dimensions")
                .Property(m => m.Width)
                .Property(m => m.Height);
            b.Section("Content")
                .Property(m => m.Content, p => p.ShowMultiline());
        });
    }

    private static void RegisterServerShowPlayerMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerShowPlayerMessage>(b =>
        {
            b.Section("Player")
                .Property(m => m.EntityId, p => p.ShowHex())
                .Property(m => m.NameStyle)
                .Property(m => m.Name);
            b.Section("Group Box")
                .Property(m => m.GroupBox, p => p.ShowMultiline());
            b.Section("Position")
                .Property(m => m.X)
                .Property(m => m.Y)
                .Property(m => m.Direction);
            b.Section("Body")
                .Property(m => m.HeadSprite, p => p.ShowHex())
                .Property(m => m.FaceShape)
                .Property(m => m.HairColor)
                .Property(m => m.BodySprite, p => p.ShowHex())
                .Property(m => m.SkinColor);
            b.Section("Visibility")
                .Property(m => m.IsTransparent)
                .Property(m => m.IsHidden)
                .IsExpanded(m => m.IsTransparent || m.IsHidden);
            b.Section("Equipment")
                .Property(m => m.Armor1Sprite, p => p.ShowHex())
                .Property(m => m.Armor2Sprite, p => p.ShowHex())
                .Property(m => m.PantsColor)
                .Property(m => m.BootsSprite, p => p.ShowHex())
                .Property(m => m.BootsColor)
                .Property(m => m.WeaponSprite, p => p.ShowHex())
                .Property(m => m.ShieldSprite, p => p.ShowHex())
                .Property(m => m.Accessory1Sprite, p => p.ShowHex())
                .Property(m => m.Accessory1Color)
                .Property(m => m.Accessory2Sprite, p => p.ShowHex())
                .Property(m => m.Accessory2Color)
                .Property(m => m.Accessory3Sprite, p => p.ShowHex())
                .Property(m => m.Accessory3Color)
                .Property(m => m.OvercoatColor)
                .Property(m => m.OvercoatSprite, p => p.ShowHex())
                .Property(m => m.Lantern)
                .IsExpanded(m => m.HeadSprite != 0xFFFF);
            b.Section("Resting")
                .Property(m => m.RestPosition)
                .IsExpanded(m => m.RestPosition != RestPosition.None);
            b.Section("Monster Form")
                .Property(m => m.MonsterSprite, p => p.ShowHex())
                .Property(m => m.MonsterUnknown)
                .IsExpanded(m => m.HeadSprite == 0xFFFF);
        });
    }

    private static void RegisterServerStatusEffectMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerStatusEffectMessage>(b =>
        {
            b.Section("Effect")
                .Property(m => m.Icon)
                .Property(m => m.Duration);
        });    
    }

    private static void RegisterServerSwitchPaneMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerSwitchPaneMessage>(b =>
        {
            b.Section("Interface")
                .Property(m => m.Pane);
        });
    }
    
    private static void RegisterServerUpdateStatsMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerUpdateStatsMessage>(b =>
        {
            b.Section("Fields")
                .Property(m => m.Fields);
            b.Section("Flags")
                .Property(m => m.IsAdmin)
                .Property(m => m.IsSwimming);
            b.Section("Level")
                .Property(m => m.Level)
                .Property(m => m.AbilityLevel)
                .IsExpanded(m => m.Level.HasValue);
            b.Section("Vitals")
                .Property(m => m.Health)
                .Property(m => m.MaxHealth)
                .Property(m => m.Mana)
                .Property(m => m.MaxMana)
                .IsExpanded(m => m.Fields.HasFlag(StatsFieldFlags.Vitals));
            b.Section("Stats")
                .Property(m => m.Strength)
                .Property(m => m.Intelligence)
                .Property(m => m.Wisdom)
                .Property(m => m.Constitution)
                .Property(m => m.Dexterity)
                .Property(m => m.HasStatPoints)
                .Property(m => m.StatPoints)
                .IsExpanded(m => m.Fields.HasFlag(StatsFieldFlags.Stats));
            b.Section("Modifiers")
                .Property(m => m.ArmorClass)
                .Property(m => m.MagicResist)
                .Property(m => m.DamageModifier)
                .Property(m => m.HitModifier)
                .Property(m => m.AttackElement)
                .Property(m => m.DefenseElement)
                .Property(m => m.IsBlinded)
                .Property(m => m.CanMove)
                .IsExpanded(m => m.Fields.HasFlag(StatsFieldFlags.Modifiers));
            b.Section("Experience")
                .Property(m => m.TotalExperience)
                .Property(m => m.ToNextLevel)
                .Property(m => m.TotalAbility)
                .Property(m => m.ToNextAbility)
                .IsExpanded(m => m.Fields.HasFlag(StatsFieldFlags.ExperienceGold));
            b.Section("Currency")
                .Property(m => m.Gold)
                .Property(m => m.GamePoints)
                .IsExpanded(m => m.Fields.HasFlag(StatsFieldFlags.ExperienceGold));
            b.Section("Weight")
                .Property(m => m.Weight)
                .Property(m => m.MaxWeight)
                .IsExpanded(m => m.Fields.HasFlag(StatsFieldFlags.Stats));
            b.Section("Mail")
                .Property(m => m.HasUnreadMail)
                .Property(m => m.HasUnreadParcels)
                .IsExpanded(m => m.Fields.HasFlag(StatsFieldFlags.Modifiers));
        });
    }

    private static void RegisterServerUserIdMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerUserIdMessage>(b =>
        {
            b.Section("User")
                .Property(m => m.UserId, p => p.ShowHex())
                .Property(m => m.Class);
            b.Section("Movement")
                .Property(m => m.Direction);
            b.Section("Guild")
                .Property(m => m.HasGuild);
        });
    }

    private static void RegisterServerUserProfileMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerUserProfileMessage>(b =>
        {
            b.Section("Entity")
                .Property(m => m.EntityId, p => p.ShowHex());
            b.Section("Profile")
                .Property(m => m.Name)
                .Property(m => m.DisplayClass)
                .Property(m => m.Guild)
                .Property(m => m.GuildRank)
                .Property(m => m.Title);
            b.Section("Citizenship")
                .Property(m => m.Nation);
            b.Section("Status")
                .Property(m => m.Status)
                .Property(m => m.IsGroupOpen);
            b.Section("Equipment")
                .Property(m => m.Equipment)
                .IsExpanded(_ => false);
            b.Section("Legend")
                .Property(m => m.LegendMarks)
                .IsExpanded(_ => false);
            b.Section("Portrait")
                .Property(m => m.Portrait, p => p.ShowMultiline())
                .IsExpanded(m => m.Portrait is not null && m.Portrait.Count > 0);
            b.Section("Bio")
                .Property(m => m.Bio, p => p.ShowMultiline())
                .IsExpanded(m => !string.IsNullOrEmpty(m.Bio));
        });
    }

    private static void RegisterServerWalkResponseMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerWalkResponseMessage>(b =>
        {
            b.Section("Movement")
                .Property(m => m.Direction);
            b.Section("Position")
                .Property(m => m.PreviousX)
                .Property(m => m.PreviousY);
            b.Section("Unknown")
                .Property(m => m.UnknownX, p => p.ShowHex())
                .Property(m => m.UnknownY, p => p.ShowHex())
                .Property(m => m.Unknown, p => p.ShowHex());
        });
    }

    private static void RegisterServerWorldMapMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerWorldMapMessage>(b =>
        {
            b.Section("Field")
                .Property(m => m.FieldIndex)
                .Property(m => m.FieldName);
            b.Section("Locations")
                .Property(m => m.Locations);
        });
    }

    private static void RegisterServerWorldMessageMapping(InspectorMappingRegistry registry)
    {
        registry.Register<ServerWorldMessageMessage>(b =>
        {
            b.Section("Message")
                .Property(m => m.MessageType)
                .Property(m => m.Message, p => p.ShowMultiline());
        });
    }
}