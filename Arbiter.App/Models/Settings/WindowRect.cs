
using System;
using System.Text.Json.Serialization;

namespace Arbiter.App.Models.Settings;

public class WindowRect : ICloneable
{
    public int? X { get; set; }
    public int? Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }

    [JsonPropertyName("maximized")] public bool IsMaximized { get; set; }

    public object Clone() => MemberwiseClone();
}