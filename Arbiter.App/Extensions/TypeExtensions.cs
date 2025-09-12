using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arbiter.App.Extensions;

public static class TypeExtensions
{
    public static IEnumerable<PropertyInfo> GetPropertiesInDerivedOrder(this Type type,
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
    {
        var types = new List<Type>();
        for (var t = type; t != null; t = t.BaseType)
        {
            types.Add(t);
        }

        types.Reverse();

        foreach (var property in types.SelectMany(t => t.GetProperties(flags | BindingFlags.DeclaredOnly)))
        {
            yield return property;
        }
    }
}