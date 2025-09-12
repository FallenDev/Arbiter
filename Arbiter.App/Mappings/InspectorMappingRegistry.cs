using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Arbiter.App.Mappings;

public class InspectorMappingRegistry
{
    private readonly ConcurrentDictionary<Type, InspectorTypeMapping> _mappings = new();

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

    public bool TryGetMapping(Type type, [NotNullWhen(true)] out InspectorTypeMapping? mapping) =>
        _mappings.TryGetValue(type, out mapping);

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