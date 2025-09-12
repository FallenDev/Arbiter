using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbiter.App.Mappings;

public class InspectorTypeMapping
{
    public Type MessageType { get; set; }
    public string? DisplayName { get; set; }
    public IReadOnlyList<InspectorSectionMapping> Sections { get; set; }

    public InspectorTypeMapping(Type messageType, IEnumerable<InspectorSectionMapping> sections,
        string? displayName = null)
    {
        MessageType = messageType;
        DisplayName = displayName;
        Sections = sections.ToList();
    }
}