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

    public InspectorPacketViewModel Create(NetworkPacket packet)
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

        object? message = null;
        if (packet is ClientPacket clientPacket &&
            ClientMessageFactory.Default.TryCreate(clientPacket, out var clientMessage))
        {
            message = clientMessage;
        }

        if (packet is ServerPacket serverPacket &&
            ServerMessageFactory.Default.TryCreate(serverPacket, out var serverMessage))
        {
            message = serverMessage;
        }

        if (message is null)
        {
            return vm;
        }

        foreach (var section in GetSectionsForMessage(message))
        {
            vm.Sections.Add(section);
        }

        return vm;
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
                InspectorItemViewModel itemVm;

                var value = prop.Getter(message);

                if (prop.Formatter is not null)
                {
                    var formatted = value is not null ? prop.Formatter(value) : string.Empty;

                    itemVm = new InspectorValueViewModel
                    {
                        Name = prop.Name,
                        Value = formatted,
                        ShowHex = false,
                        IsMultiline = prop.ShowMultiline,
                        ToolTip = prop.ToolTip
                    };
                }
                else
                {
                    itemVm = CreateItemViewModel(value, prop);
                }

                sectionVm.Items.Add(itemVm);
            }

            yield return sectionVm;
        }
    }

    private static InspectorItemViewModel CreateItemViewModel(object? value, InspectorPropertyMapping propMapping,
        Type? valueType = null)
    {
        if (value is null)
        {
            return CreateScalarViewModel(value, propMapping);
        }

        if (IsByteEnumerable(value))
        {
            return new InspectorValueViewModel
            {
                Name = propMapping.Name,
                Value = value,
                ShowHex = propMapping.ShowHex,
                IsMultiline = propMapping.ShowMultiline,
                ToolTip = propMapping.ToolTip
            };
        }

        if (value is IDictionary dict)
        {
            return CreateDictionaryViewModel(dict, propMapping, value.GetType());
        }

        if (value is IEnumerable list and not string and not char[])
        {
            return CreateListViewModel(list, propMapping, value.GetType());
        }

        if (valueType is not null && IsCustomType(valueType))
        {
            return CreateClassViewModel(value, propMapping, valueType);
        }

        return CreateScalarViewModel(value, propMapping, value.GetType());
    }

    private static InspectorListViewModel CreateListViewModel(IEnumerable list, InspectorPropertyMapping propMapping,
        Type? listType = null)
    {
        var vm = new InspectorListViewModel
        {
            Name = propMapping.Name,
        };

        var index = 0;
        foreach (var item in list)
        {
            var child = CreateItemViewModel(item, propMapping, item?.GetType());
            child.Name = $"Element {index}";

            vm.Items.Add(child);
            index++;
        }

        return vm;
    }

    private static InspectorDictionaryViewModel CreateDictionaryViewModel(IDictionary dict,
        InspectorPropertyMapping propMapping, Type? dictType = null)
    {
        var vm = new InspectorDictionaryViewModel
        {
            Name = propMapping.Name,
            TypeName = propMapping.PropertyType.Name
        };

        foreach (DictionaryEntry kv in dict)
        {
            var key = kv.Key.ToString()?.ToNaturalWording() ?? "<null>";
            var child = CreateItemViewModel(kv.Value, propMapping, kv.Value?.GetType());

            if (child is InspectorValueViewModel valueVm)
            {
                valueVm.Name = key;
                vm.Values.Add(valueVm);
            }
        }

        return vm;
    }

    private static InspectorDictionaryViewModel CreateClassViewModel(object value, InspectorPropertyMapping propMapping,
        Type classType)
    {
        var vm = new InspectorDictionaryViewModel
        {
            Name = propMapping.Name,
            TypeName = classType.Name
        };

        var properties = classType.GetPropertiesInDerivedOrder()
            .Where(p => p.GetMethod is not null);

        foreach (var property in properties)
        {
            var child = CreateItemViewModel(property.GetValue(value), propMapping, property.PropertyType);

            if (child is InspectorValueViewModel valueVm)
            {
                valueVm.Name = property.Name.ToNaturalWording();
                vm.Values.Add(valueVm);
            }
        }

        return vm;
    }

    private static InspectorValueViewModel CreateScalarViewModel(object? value, InspectorPropertyMapping propMapping,
        Type? valueType = null)
    {
        return new InspectorValueViewModel
        {
            Name = propMapping.Name,
            Value = value,
            ShowHex = propMapping.ShowHex,
            IsMultiline = propMapping.ShowMultiline,
            ToolTip = propMapping.ToolTip
        };
    }

    private static bool IsByteEnumerable(object value) => value switch
    {
        byte[] => true,
        IEnumerable<byte> => true,
        _ => false
    };

    private static bool IsCustomType(Type type) => type.IsClass && type != typeof(string) && type != typeof(IPAddress);
}