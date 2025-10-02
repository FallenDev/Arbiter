using System;

namespace Arbiter.App.Models;

public class DebugSettings : ICloneable
{
    public bool ShowDialogId { get; set; }
    public bool ShowNpcId { get; set; }
    public bool ShowMonsterId { get; set; }
    public bool ShowMonsterClickId { get; set; }
    public bool ShowHiddenPlayers { get; set; }
    public bool IgnoreEmptyMessages { get; set; }

    public bool CheckEnabled() => ShowDialogId || ShowNpcId || ShowMonsterId || ShowMonsterClickId ||
                                  ShowHiddenPlayers || IgnoreEmptyMessages;

    public object Clone() => new DebugSettings
    {
        ShowDialogId = ShowDialogId,
        ShowNpcId = ShowNpcId,
        ShowMonsterId = ShowMonsterId,
        ShowMonsterClickId = ShowMonsterClickId,
        ShowHiddenPlayers = ShowHiddenPlayers,
        IgnoreEmptyMessages = IgnoreEmptyMessages
    };
}