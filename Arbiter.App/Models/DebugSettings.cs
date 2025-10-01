using System;

namespace Arbiter.App.Models;

public class DebugSettings : ICloneable
{
    public bool ShowNpcId { get; set; }
    public bool ShowMonsterId { get; set; }

    public object Clone() => new DebugSettings
    {
        ShowNpcId = ShowNpcId,
        ShowMonsterId = ShowMonsterId,
    };
}