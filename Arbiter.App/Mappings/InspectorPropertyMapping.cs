using System;
using System.Reflection;

namespace Arbiter.App.Mappings;

public class InspectorPropertyMapping
{
    public string Name { get; set; }
    public MemberInfo Member { get; }
    public Type PropertyType { get; }
    public Func<object, object?> Getter { get; }
    
    public bool ShowHex { get; }
    public bool ShowMultiline { get; }
    public Func<object, string>? Formatter { get; }

    public InspectorPropertyMapping(string name, MemberInfo member, Type propertyType, Func<object, object?> getter,
        bool showHex = false, bool showMultiline = false, Func<object, string>? formatter = null)
    {
        Name = name;
        Member = member;
        PropertyType = propertyType;
        Getter = getter;
        ShowHex = showHex;
        ShowMultiline = showMultiline;
        Formatter = formatter;
    }
}