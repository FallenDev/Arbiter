using System;

namespace Arbiter.App.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class InspectSectionAttribute : Attribute
{
    public required string Header { get; set; }
    public int Order { get; set; } = int.MaxValue;
}