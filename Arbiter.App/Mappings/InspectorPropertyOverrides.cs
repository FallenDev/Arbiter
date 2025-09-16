using System;
using System.Reflection;

namespace Arbiter.App.Mappings;

public class InspectorPropertyOverrides
{
    public string PropertyName { get; }
    public MemberInfo? Member { get; }

    // Options are nullable to indicate "not specified" so we can merge cleanly
    public bool? ShowHex { get; set; }
    public bool? ShowMultiline { get; set; }
    public Func<object, string>? Formatter { get; set; }
    public bool? IsMasked { get; set; }
    public char? MaskCharacter { get; set; }
    public string? ToolTip { get; set; }
    public string? DisplayName { get; set; }
    public bool? IsExpanded { get; set; }

    public InspectorPropertyOverrides(string propertyName, MemberInfo? member = null)
    {
        PropertyName = propertyName;
        Member = member;
    }
}