using Arbiter.App.Extensions;
using Arbiter.Net.Server.Messages;
using Arbiter.Net.Server.Types;

namespace Arbiter.App.Mappings.Server;

public class ServerTypeOverridesProvider : IInspectorMappingProvider
{
    public void RegisterMappings(InspectorMappingRegistry registry)
    {
        registry.RegisterOverrides<ServerMessageBoardInfo>(b =>
        {
            b.Property(m => m.Id, p => p.ToolTip("ID of the message board."))
                .Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display name of the message board."))
                .DisplayName(m => m.Name)
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerMessageBoardPostListing>(b =>
        {
            b.Property(m => m.Id, p => p.ToolTip("ID of the message board post."))
                .Property(m => m.Author, p => p.ToolTip("Display name of the author of the post."))
                .Property(m => m.Subject, p => p.ShowMultiline().ToolTip("Subject of the post."))
                .Property(m => m.Month, p => p.ToolTip("Month the post was created."))
                .Property(m => m.Day, p => p.ToolTip("Day the post was created."))
                .Property(m => m.IsHighlighted, p => p.ToolTip("Whether the post is highlighted."))
                .DisplayName(m => $"Post {m.Id}")
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerMessageBoardPost>(b =>
        {
            b.Property(m => m.Id, p => p.ToolTip("ID of the message board post."))
                .Property(m => m.Author, p => p.ToolTip("Display name of the author of the post."))
                .Property(m => m.Subject, p => p.ShowMultiline().ToolTip("Subject of the post."))
                .Property(m => m.Body, p => p.ShowMultiline().ToolTip("Content of the post."))
                .Property(m => m.Month, p => p.ToolTip("Month the post was created."))
                .Property(m => m.Day, p => p.ToolTip("Day the post was created."))
                .Property(m => m.IsHighlighted, p => p.ToolTip("Whether the post is highlighted."))
                .DisplayName(m => $"Post {m.Id}")
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerCreatureEntity>(b =>
        {
            b.Property(m => m.Id, p => p.ShowHex().ToolTip("ID of the creature entity."))
                .Property(m => m.Sprite, p => p.ShowHex().ToolTip("Sprite used to display the creature."))
                .Property(m => m.X, p => p.ToolTip("X-coordinate of the creature."))
                .Property(m => m.Y, p => p.ToolTip("Y-coordinate of the creature."))
                .Property(m => m.CreatureType, p => p.ToolTip("Type of creature entity."))
                .Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display name of the creature."))
                .Property(m => m.Direction, p => p.ToolTip("Direction the creature is facing."))
                .Property(m => m.Unknown, p => p.ShowHex())
                .DisplayName(m => $"Entity {m.Id:X}")
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerDialogMenuChoice>(b =>
        {
            b.Property(m => m.Text, p => p.ShowMultiline().ToolTip("Display text of the menu choice."))
                .Property(m => m.PursuitId, p => p.ToolTip("ID of the pursuit the menu choice will begin."))
                .DisplayName(m => m.Text)
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerEquipmentInfo>(b =>
        {
            b.Property(m => m.Slot, p => p.ToolTip("Equipment slot the item belongs to."))
                .Property(m => m.Sprite, p => p.ShowHex().ToolTip("Sprite used to display the equipment item."))
                .Property(m => m.Color, p => p.ToolTip("Override (dye) color applied to the sprite."))
                .DisplayName(m => m.Slot.ToString().ToNaturalWording())
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerGroupBox>(b =>
        {
            b.Property(m => m.Leader, p => p.ToolTip("Name of the group leader."))
                .Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display text for the floating group box."))
                .Property(m => m.Note, p => p.ShowMultiline().ToolTip("Description for the group."))
                .Property(m => m.MinLevel, p => p.ToolTip("Minimum level for group members."))
                .Property(m => m.MaxLevel, p => p.ToolTip("Maximum level for group members."))
                .Property(m => m.CurrentWarriors, p => p.ToolTip("Number of warriors in the group currently."))
                .Property(m => m.MaxWarriors, p => p.ToolTip("Number of desired warriors for the group"))
                .Property(m => m.CurrentWizards, p => p.ToolTip("Number of wizards in the group currently."))
                .Property(m => m.MaxWizards, p => p.ToolTip("Number of desired wizards for the group."))
                .Property(m => m.CurrentRogues, p => p.ToolTip("Number of rogues in the group currently."))
                .Property(m => m.MaxRogues, p => p.ToolTip("Number of desired rogues for the group."))
                .Property(m => m.CurrentPriests, p => p.ToolTip("Number of priests in the group currently."))
                .Property(m => m.MaxPriests, p => p.ToolTip("Number of desired priests for the group."))
                .Property(m => m.CurrentMonks, p => p.ToolTip("Number of monks in the group currently."))
                .Property(m => m.MaxMonks, p => p.ToolTip("Number of desired monks for the group."));
        });

        registry.RegisterOverrides<ServerItemEntity>(b =>
        {
            b.Property(m => m.Id, p => p.ShowHex().ToolTip("ID of the item entity."))
                .Property(m => m.Sprite, p => p.ShowHex().ToolTip("Sprite used to display the item."))
                .Property(m => m.Color, p => p.ToolTip("Override (dye) color applied to the sprite."))
                .Property(m => m.X, p => p.ToolTip("X-coordinate of the ground item."))
                .Property(m => m.Y, p => p.ToolTip("Y-coordinate of the ground item."))
                .Property(m => m.Unknown, p => p.ShowHex())
                .DisplayName(m => $"Item {m.Id:X}")
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerItemMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex().ToolTip("Sprite used to display the item."))
                .Property(m => m.Color, p => p.ToolTip("Override (dye) color applied to the sprite."))
                .Property(m => m.Price, p => p.ToolTip("Price of the item."))
                .Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display name of the item."))
                .Property(m => m.Description, p => p.ShowMultiline().ToolTip("Description of the item."))
                .DisplayName(m => m.Name)
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerLegendMark>(b =>
        {
            b.Property(m => m.Icon, p => p.ToolTip("Icon displayed with the legend mark."))
                .Property(m => m.Color, p => p.ToolTip("Color of the legend mark text."))
                .Property(m => m.Key, p => p.ToolTip("Unique key used to identify the legend mark."))
                .Property(m => m.Text, p => p.ShowMultiline().ToolTip("Text displayed for the legend mark."))
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerMetadataEntry>(b =>
        {
            b.Property(m => m.Name, p => p.ToolTip("Name of the metadata file."))
                .Property(m => m.Checksum, p => p.ShowHex().ToolTip("CRC-32 checksum of the metadata file."))
                .DisplayName(m => !string.IsNullOrWhiteSpace(m.Name) ? m.Name : "Metadata")
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerSpellMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex().ToolTip("Sprite used to display the spell icon."))
                .Property(m => m.Color, p => p.ToolTip("Override color applied to the sprite."))
                .Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display name of the spell."))
                .DisplayName(m => m.Name)
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerSkillMenuChoice>(b =>
        {
            b.Property(m => m.Sprite, p => p.ShowHex().ToolTip("Sprite used to display the skill icon."))
                .Property(m => m.Color, p => p.ToolTip("Override color applied to the sprite."))
                .Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display name of the skill."))
                .DisplayName(m => m.Name)
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerWorldListUser>(b =>
        {
            b.Property(m => m.Class, p => p.ToolTip("Base class of the user."))
                .Property(m => m.Flags, p => p.ShowHex().ToolTip("Flags for displaying the user in the list."))
                .Property(m => m.Color, p => p.ToolTip("Color of the user's name within the list."))
                .Property(m => m.Status, p => p.ToolTip("Social status of the user."))
                .Property(m => m.Title, p => p.ToolTip("Title displayed next to the user's name."))
                .Property(m => m.IsMaster, p => p.ToolTip("Whether the user is a master."))
                .Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display name of the user."))
                .DisplayName(m => m.Name)
                .IsExpanded(_ => false);
        });

        registry.RegisterOverrides<ServerWorldMapNode>(b =>
        {
            b.Property(m => m.ScreenX, p => p.ToolTip("X-coordinate of the node on the world map screen."))
                .Property(m => m.ScreenY, p => p.ToolTip("Y-coordinate of the node on the world map screen."))
                .Property(m => m.Name, p => p.ShowMultiline().ToolTip("Display name of the destination."))
                .Property(m => m.MapId, p => p.ToolTip("ID of the destination map."))
                .Property(m => m.MapX, p => p.ToolTip("X-coordinate of the destination on the map."))
                .Property(m => m.MapY, p => p.ToolTip("Y-coordinate of the destination on the map."))
                .Property(m => m.Checksum, p => p.ShowHex().ToolTip("CRC-16 checksum of the world map node."))
                .DisplayName(m => m.Name)
                .IsExpanded(_ => false);
        });
    }
}