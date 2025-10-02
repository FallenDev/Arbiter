using System;

namespace Arbiter.App.Models;

public class DebugSettings : ICloneable
{
    public bool ShowDialogId { get; set; }
    public bool ShowNpcId { get; set; }
    public bool ShowMonsterId { get; set; }
    public bool ShowMonsterClickId { get; set; }
    public bool ShowHiddenPlayers { get; set; }
    public bool ShowPlayerNames { get; set; }
    public bool DisableWeatherEffects { get; set; }
    public bool EnableTabMap { get; set; }
    public bool IgnoreEmptyMessages { get; set; }

    public bool CheckEnabled() => ShowDialogId || ShowNpcId || ShowMonsterId || ShowMonsterClickId ||
                                  ShowHiddenPlayers || ShowPlayerNames || EnableTabMap || DisableWeatherEffects ||
                                  IgnoreEmptyMessages;

    public object Clone() => new DebugSettings
    {
        ShowDialogId = ShowDialogId,
        ShowNpcId = ShowNpcId,
        ShowMonsterId = ShowMonsterId,
        ShowMonsterClickId = ShowMonsterClickId,
        ShowHiddenPlayers = ShowHiddenPlayers,
        ShowPlayerNames = ShowPlayerNames,
        EnableTabMap = EnableTabMap,
        DisableWeatherEffects = DisableWeatherEffects,
        IgnoreEmptyMessages = IgnoreEmptyMessages
    };
}