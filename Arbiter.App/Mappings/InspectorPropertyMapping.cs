using System;
using System.Reflection;

namespace Arbiter.App.Mappings;

public class InspectorPropertyMapping
{
    public string Name { get; set; }
    public MemberInfo? Member { get; }
    public Type PropertyType { get; }
    public Func<object, object?> Getter { get; }
    
    public bool ShowHex { get; set; }
    public bool ShowMultiline { get; set; }
    public Func<object, string>? Formatter { get; set; }
    public bool IsMasked { get; set; }
    public char? MaskCharacter { get; set; }
    public string? ToolTip { get; set; }

    public InspectorPropertyMapping(string name, MemberInfo? member, Type propertyType, Func<object, object?> getter)
    {
        Name = name;
        Member = member;
        PropertyType = propertyType;
        Getter = getter;
    }
}