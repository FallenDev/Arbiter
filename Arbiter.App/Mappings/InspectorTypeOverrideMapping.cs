using System;
using System.Collections.Generic;

namespace Arbiter.App.Mappings;

public class InspectorTypeOverrideMapping
{
    public Type TargetType { get; }
    public IReadOnlyDictionary<string, InspectorPropertyOverrides> Properties { get; }

    public InspectorTypeOverrideMapping(Type targetType, IDictionary<string, InspectorPropertyOverrides> properties)
    {
        TargetType = targetType;
        Properties = new Dictionary<string, InspectorPropertyOverrides>(properties, StringComparer.Ordinal);
    }

    public bool TryGetProperty(string propertyName, out InspectorPropertyOverrides? overrides)
    {
        return Properties.TryGetValue(propertyName, out overrides);
    }
}