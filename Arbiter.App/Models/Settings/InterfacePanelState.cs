using System;
using System.Text.Json.Serialization;

namespace Arbiter.App.Models.Settings;

public class InterfacePanelState : ICloneable
{
    [JsonPropertyName("collapsed")]
    public bool IsCollapsed { get; set; }
    
    public double? Width { get; set; }
    public double? Height { get; set; }
    
    public object Clone() => MemberwiseClone();
}