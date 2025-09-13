using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Arbiter.App.Mappings;

public class InspectorMappingRegistry
{
    private readonly ConcurrentDictionary<Type, InspectorTypeMapping> _mappings = new();
    private readonly ConcurrentDictionary<Type, InspectorTypeOverrideMapping> _overrides = new();

    public InspectorMappingRegistry()
    {
        RegisterFromAssemblyProviders();
    }

    public void Register<T>(Action<InspectorTypeBuilder<T>> configure)
    {
        var builder = new InspectorTypeBuilder<T>();
        configure.Invoke(builder);

        _mappings.TryAdd(typeof(T), builder.Build());
    }

    public void RegisterOverrides<T>(Action<InspectorTypeOverrideBuilder<T>> configure)
    {
        var builder = new InspectorTypeOverrideBuilder<T>();
        configure.Invoke(builder);
        _overrides.AddOrUpdate(typeof(T), _ => builder.Build(), (_, __) => builder.Build());
    }

    public bool TryGetMapping(Type type, [NotNullWhen(true)] out InspectorTypeMapping? mapping) =>
        _mappings.TryGetValue(type, out mapping);

    public bool TryGetOverrides(Type type, [NotNullWhen(true)] out InspectorTypeOverrideMapping? overrides) =>
        _overrides.TryGetValue(type, out overrides);

    public bool TryGetPropertyOverride(Type type, string propertyName, out InspectorPropertyOverrides? propertyOverride)
    {
        propertyOverride = null;
        if (!_overrides.TryGetValue(type, out var typeOverrides))
        {
            return false;
        }

        if (!typeOverrides.TryGetProperty(propertyName, out var found))
        {
            return false;
        }
        
        propertyOverride = found;
        return true;
    }

    private void RegisterFromAssemblyProviders()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var providerType = typeof(IInspectorMappingProvider);

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => !t.IsAbstract && providerType.IsAssignableFrom(t));

            foreach (var type in types)
            {
                var instance = (IInspectorMappingProvider)Activator.CreateInstance(type)!;
                instance.RegisterMappings(this);
            }
        }
    }
}