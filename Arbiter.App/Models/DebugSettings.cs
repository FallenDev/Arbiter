using System;

namespace Arbiter.App.Models;

public class DebugSettings : ICloneable
{
    public bool ShowDialogId { get; set; }
    public bool ShowNpcId { get; set; }
    public bool ShowMonsterId { get; set; }

    public bool CheckEnabled() => ShowDialogId || ShowNpcId || ShowMonsterId;
    
    public object Clone() => new DebugSettings
    {
        ShowDialogId = ShowDialogId,
        ShowNpcId = ShowNpcId,
        ShowMonsterId = ShowMonsterId,
    };
}