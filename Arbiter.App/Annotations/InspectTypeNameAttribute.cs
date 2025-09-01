using System;

namespace Arbiter.App.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class InspectTypeNameAttribute : Attribute
{
    public string Name { get; set; }

    public InspectTypeNameAttribute(string name)
    {
        Name = name;
    }
}