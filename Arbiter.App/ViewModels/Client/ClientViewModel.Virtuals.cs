using System;
using Arbiter.App.Models.Player;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Client;

public partial class ClientViewModel
{
    public void AddVirtualSkill(int desiredSlot, ushort icon, string name, Action onUse)
    {
        if (!_connection.IsLoggedIn)
        {
            return;
        }

        // Determine the first available slot
        var availableSlot = Player.Skillbook.GetFirstEmptySlot(desiredSlot);
        if (availableSlot is null)
        {
            return;
        }

        var slot = availableSlot.Value;

        // Add the skill to the skillbook
        var virtualSkill = new SkillbookItem(slot, icon, name, onUse);
        Player.Skillbook.SetSlot(slot, virtualSkill);

        // Send the message to the client to add the skill to the skillbook
        var addSkillMessage = new ServerAddSkillMessage
        {
            Slot = (byte)slot,
            Name = name,
            Icon = icon,
        };
        _connection.EnqueueMessage(addSkillMessage);
    }

    public bool RemoveVirtualSkill(string name)
    {
        if (!_connection.IsLoggedIn || !Player.Skillbook.TryRemoveSkill(name, out var slot))
        {
            return false;
        }

        var removeSkillMessage = new ServerRemoveSkillMessage
        {
            Slot = (byte)slot
        };
        _connection.EnqueueMessage(removeSkillMessage);
        return true;
    }
}