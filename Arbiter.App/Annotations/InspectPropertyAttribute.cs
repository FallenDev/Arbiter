using System;

namespace Arbiter.App.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class InspectPropertyAttribute : Attribute
{
    public string? Name { get; set; }
    public int Order { get; set; } = int.MaxValue;
    public string? StringFormat { get; set; }
    public bool ShowHex { get; set; }
}