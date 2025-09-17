using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Arbiter.App.Extensions;
using Arbiter.App.Mappings;
using Arbiter.App.Models;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Client.Messages;
using Arbiter.Net.Server;
using Arbiter.Net.Server.Messages;

namespace Arbiter.App.ViewModels.Inspector;

public class InspectorViewModelFactory
{
    private readonly InspectorMappingRegistry _registry;

    public InspectorViewModelFactory(InspectorMappingRegistry registry)
    {
        _registry = registry;
    }

    public (InspectorPacketViewModel, Exception?) Create(NetworkPacket packet)
    {
        var displayName = packet switch
        {
            ClientPacket cp => cp.Command.ToString(),
            ServerPacket sp => sp.Command.ToString(),
            _ => string.Empty
        };

        var vm = new InspectorPacketViewModel
        {
            DisplayName = displayName,
            Direction = packet is ClientPacket ? PacketDirection.Client : PacketDirection.Server,
            Command = packet.Command
        };

        Exception? exception = null;
        try
        {
            object? message = null;
            if (packet is ClientPacket clientPacket)
            {
                message = ClientMessageFactory.Default.Create(clientPacket);
                vm.Value = message;
            }

            if (packet is ServerPacket serverPacket)
            {
                message = ServerMessageFactory.Default.Create(serverPacket);
                vm.Value = message;
            }

            if (message is not null)
            {
                foreach (var section in GetSectionsForMessage(message))
                {
                    vm.Sections.Add(section);
                }
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        return (vm, exception);
    }

    private IEnumerable<InspectorSectionViewModel> GetSectionsForMessage(object message)
    {
        var messageType = message.GetType();
        if (!_registry.TryGetMapping(messageType, out var mapping))
        {
            yield break;
        }

        foreach (var section in mapping.Sections)
        {
            var sectionVm = new InspectorSectionViewModel
                { Header = section.Header, IsExpanded = section.IsExpanded(message) };

            foreach (var prop in section.Properties)
            {
                // Lookup overrides for this property on the containing message type
                var overrideKey = prop.Member?.Name ?? prop.Name;
                _registry.TryGetPropertyOverride(messageType, overrideKey, out var o);

                var value = prop.Getter(message);
                var formatter = o?.Formatter ?? prop.Formatter;
                var displayName = o?.DisplayName ?? prop.Name;
                var showMultiline = o?.ShowMultiline ?? prop.ShowMultiline;
                var toolTip = o?.ToolTip ?? prop.ToolTip;

                InspectorItemViewModel itemVm;
                if (formatter is not null)
                {
                    var formatted = value is not null ? formatter(value) : string.Empty;
                    itemVm = new InspectorValueViewModel
                    {
                        Name = displayName,
                        Value = formatted,
                        ShowHex = false,
                        IsMultiline = showMultiline,
                        ToolTip = toolTip
                    };
                }
                else
                {
                    itemVm = CreateItemViewModel(value, prop, value?.GetType(), messageType);
                    // Ensure name reflects display override for container/scalar created below
                    if (itemVm is InspectorValueViewModel v)
                    {
                        v.Name = displayName;
                        v.IsMultiline = showMultiline;
                        v.ToolTip = toolTip;
                    }
                    else
                    {
                        itemVm.Name = displayName;
                    }
                }

                sectionVm.Items.Add(itemVm);
            }

            yield return sectionVm;
        }
    }

    private InspectorItemViewModel CreateItemViewModel(object? value, InspectorPropertyMapping propMapping,
        Type? valueType = null, Type? containingType = null)
    {
        if (value is null)
        {
            return CreateScalarViewModel(value, propMapping);
        }

        if (IsByteEnumerable(value))
        {
            // Merge overrides for byte sequences based on containing type
            var showHex = MergeBool(containingType, propMapping, static (o, p) => o?.ShowHex ?? p.ShowHex);
            var showMultiline = MergeBool(containingType, propMapping, static (o, p) => o?.ShowMultiline ?? p.ShowMultiline);
            var toolTip = MergeValue(containingType, propMapping, static (o, p) => o?.ToolTip ?? p.ToolTip);

            return new InspectorValueViewModel
            {
                Name = MergeDisplayName(containingType, propMapping),
                Value = value,
                ShowHex = showHex,
                IsMultiline = showMultiline,
                ToolTip = toolTip
            };
        }

        if (value is IDictionary dict)
        {
            return CreateDictionaryViewModel(dict, propMapping, value.GetType(), containingType);
        }

        if (value is IEnumerable list and not string and not char[])
        {
            return CreateListViewModel(list, propMapping, value.GetType(), containingType);
        }

        if (valueType is not null && IsCustomType(valueType))
        {
            return CreateClassViewModel(value, propMapping, valueType, containingType);
        }

        return CreateScalarViewModel(value, propMapping, value.GetType(), containingType);
    }

    private InspectorListViewModel CreateListViewModel(IEnumerable list, InspectorPropertyMapping propMapping,
        Type? listType = null, Type? containingType = null)
    {
        var isExpanded = MergeBool(containingType, propMapping, static (o, p) => o?.IsExpanded ?? true);
        
        var vm = new InspectorListViewModel
        {
            Name = MergeDisplayName(containingType, propMapping),
        };

        var index = 0;
        foreach (var item in list)
        {
            var itemType = item?.GetType();
            var child = CreateItemViewModel(item, propMapping, itemType, containingType);

            string? nameOverride = null;
            if (itemType is not null && _registry.TryGetOverrides(itemType, out var typeOverrides))
            {
                var selector = typeOverrides.DisplayNameSelector;
                if (selector is not null)
                {
                    try
                    {
                        nameOverride = selector(item!);
                    }
                    catch
                    {
                        // ignore selector failures and fallback to default naming
                    }
                }
            }

            child.Name = nameOverride ?? $"Element {index}";

            vm.Items.Add(child);
            index++;
        }

        return vm;
    }

    private InspectorDictionaryViewModel CreateDictionaryViewModel(IDictionary dict,
        InspectorPropertyMapping propMapping, Type? dictType = null, Type? containingType = null)
    {
        var isExpanded = MergeBool(containingType, propMapping, static (o, p) => o?.IsExpanded ?? true);
        
        var vm = new InspectorDictionaryViewModel
        {
            Name = MergeDisplayName(dictType, propMapping),
            TypeName = propMapping.PropertyType.Name,
            IsExpanded = isExpanded,
        };

        foreach (DictionaryEntry kv in dict)
        {
            var key = kv.Key.ToString()?.ToNaturalWording() ?? "<null>";
            var child = CreateItemViewModel(kv.Value, propMapping, kv.Value?.GetType(), containingType);

            if (child is InspectorValueViewModel valueVm)
            {
                valueVm.Name = key;
                vm.Values.Add(valueVm);
            }
        }

        return vm;
    }

    private InspectorDictionaryViewModel CreateClassViewModel(object value, InspectorPropertyMapping propMapping,
        Type classType, Type? containingType = null)
    {
        var isExpanded = MergeBool(containingType, propMapping, static (o, p) => o?.IsExpanded ?? true);
        
        var vm = new InspectorDictionaryViewModel
        {
            Name = MergeDisplayName(classType, propMapping),
            TypeName = classType.Name,
            IsExpanded = isExpanded,
        };

        if (_registry.TryGetMapping(classType, out var typeMapping))
        {
            foreach (var section in typeMapping.Sections)
            {
                foreach (var nestedProp in section.Properties)
                {
                    var nestedValue = nestedProp.Getter(value);
                    var child = CreateItemViewModel(nestedValue, nestedProp, nestedValue?.GetType(), classType);

                    if (child is InspectorValueViewModel valueVm)
                    {
                        vm.Values.Add(valueVm);
                    }
                }
            }
        }
        else
        {
            var properties = classType.GetPropertiesInDerivedOrder()
                .Where(p => p.GetMethod is not null);

            foreach (var property in properties)
            {
                var tempMapping = CreateMappingFromPropertyInfo(property);
                var child = CreateItemViewModel(property.GetValue(value), tempMapping, property.PropertyType, classType);

                if (child is InspectorValueViewModel valueVm)
                {
                    vm.Values.Add(valueVm);
                }
            }
        }

        return vm;
    }

    private InspectorValueViewModel CreateScalarViewModel(object? value, InspectorPropertyMapping propMapping,
        Type? valueType = null, Type? containingType = null)
    {
        var showHex = MergeBool(containingType, propMapping, static (o, p) => o?.ShowHex ?? p.ShowHex);
        var showMultiline = MergeBool(containingType, propMapping, static (o, p) => o?.ShowMultiline ?? p.ShowMultiline);
        var isMasked = MergeBool(containingType, propMapping, static (o, p) => o?.IsMasked ?? p.IsMasked);
        var maskChar = MergeValue(containingType, propMapping, static (o, p) => o?.MaskCharacter ?? p.MaskCharacter);
        var toolTip = MergeValue(containingType, propMapping, static (o, p) => o?.ToolTip ?? p.ToolTip);
        var name = MergeDisplayName(containingType, propMapping);
        
        return new InspectorValueViewModel
        {
            Name = name,
            Value = value,
            ShowHex = showHex,
            IsMultiline = showMultiline,
            IsRevealed = !isMasked,
            MaskCharacter = maskChar,
            ToolTip = toolTip,
        };
    }

    private static bool IsByteEnumerable(object value) => value switch
    {
        byte[] => true,
        IEnumerable<byte> => true,
        _ => false
    };

    private static bool IsCustomType(Type type) => type.IsClass && type != typeof(string) && type != typeof(IPAddress);


    private static InspectorPropertyMapping CreateMappingFromPropertyInfo(PropertyInfo property)
    {
        // Create a simple getter
        object? Getter(object instance)
        {
            if (instance is null)
            {
                return null;
            }

            return property.GetValue(instance);
        }

        return new InspectorPropertyMapping(property.Name.ToNaturalWording(), property, property.PropertyType, Getter)
        {
            ShowHex = false,
            ShowMultiline = false,
            IsMasked = false,
            MaskCharacter = null,
            Formatter = null,
            ToolTip = null
        };
    }

    // Helper methods to merge override values with base mapping without cloning
    private bool MergeBool(Type? containingType, InspectorPropertyMapping prop,
        Func<InspectorPropertyOverrides?, InspectorPropertyMapping, bool> selector)
    {
        if (containingType is not null)
        {
            var overrideKey = prop.Member?.Name ?? prop.Name;
            if (_registry.TryGetPropertyOverride(containingType, overrideKey, out var o))
            {
                return selector(o, prop);
            }
        }
        return selector(null, prop);
    }

    private T? MergeValue<T>(Type? containingType, InspectorPropertyMapping prop,
        Func<InspectorPropertyOverrides?, InspectorPropertyMapping, T?> selector)
    {
        if (containingType is not null)
        {
            var overrideKey = prop.Member?.Name ?? prop.Name;
            if (_registry.TryGetPropertyOverride(containingType, overrideKey, out var o))
            {
                return selector(o, prop);
            }
        }
        return selector(null, prop);
    }

    private string MergeDisplayName(Type? containingType, InspectorPropertyMapping prop)
    {
        var name = MergeValue(containingType, prop, static (o, p) => o?.DisplayName ?? p.Name);
        return name ?? prop.Name;
    }
}