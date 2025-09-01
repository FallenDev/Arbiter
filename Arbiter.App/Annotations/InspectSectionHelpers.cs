using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Arbiter.App.Annotations;

public static class InspectSectionHelpers
{
    private static readonly ConcurrentDictionary<(Type type, string handler), MethodInfo> HandlerCache = [];

    public static bool? EvaluateIsExpanded(InspectSectionAttribute attribute, object? target)
    {
        if (target == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(attribute.IsExpandedHandler))
        {
            return null;
        }

        var targetType = target.GetType();
        var key = (targetType, attribute.IsExpandedHandler);

        if (!HandlerCache.TryGetValue(key, out var handler))
        {
            handler = targetType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == attribute.IsExpandedHandler && m.GetParameters().Length == 0);

            if (handler is not null)
            {
                HandlerCache.TryAdd(key, handler);
            }
        }

        var result = (bool?)handler?.Invoke(target, null);
        return result;
    }
}