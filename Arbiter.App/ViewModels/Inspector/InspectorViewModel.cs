using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arbiter.App.Annotations;
using Arbiter.App.Models;
using Arbiter.App.Models.Packets;
using Arbiter.Common.Extensions;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels.Inspector;

public partial class InspectorViewModel : ViewModelBase
{
    private readonly PacketMessageFactory _packetMessageFactory = new();

    private NetworkPacket? _selectedPacket;

    [ObservableProperty] private InspectorPacketViewModel? _inspectedPacket;

    public NetworkPacket? SelectedPacket
    {
        get => _selectedPacket;
        set
        {
            if (SetProperty(ref _selectedPacket, value))
            {
                OnPacketSelected(value);
            }
        }
    }

    public InspectorViewModel()
    {
        _packetMessageFactory.RegisterFromAssembly();
    }

    private void OnPacketSelected(NetworkPacket? packet)
    {
        if (packet is null)
        {
            InspectedPacket = null;
            return;
        }

        var fallbackName = packet switch
        {
            ClientPacket clientPacket => clientPacket.Command.ToString(),
            ServerPacket serverPacket => serverPacket.Command.ToString(),
            _ => "Unknown"
        };

        var vm = new InspectorPacketViewModel
        {
            DisplayName = fallbackName,
            Direction = packet is ClientPacket ? PacketDirection.Client : PacketDirection.Server,
            Command = packet.Command
        };

        if (_packetMessageFactory.CanParse(packet))
        {
            if (!_packetMessageFactory.TryParsePacket(packet, out var message, out var exception))
            {
                vm.Exception = exception;
            }
            else
            {
                vm.DisplayName = GetPacketDisplayName(message) ?? fallbackName;

                foreach (var section in GetSections(message).Values.OrderBy(s => s.Order))
                {
                    if (section.Items.Count > 0)
                    {
                        vm.Sections.Add(section);
                    }
                }
            }
        }

        InspectedPacket = vm;
    }

    private static string? GetPacketDisplayName(IPacketMessage message)
        => message.GetType().GetCustomAttribute<InspectPacketAttribute>()?.Name;

    private static Dictionary<string, InspectorSectionViewModel> GetSections(IPacketMessage message)
    {
        var sections = new Dictionary<string, InspectorSectionViewModel>(StringComparer.OrdinalIgnoreCase);

        // Default to the uncategorized section
        var defaultSection = new InspectorSectionViewModel { Header = "Uncategorized" };
        var currentSection = defaultSection;

        foreach (var property in message.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var sectionAttribute = property.GetCustomAttribute<InspectSectionAttribute>();

            // Switch to a different section
            if (sectionAttribute is not null)
            {
                if (!sections.TryGetValue(sectionAttribute.Header, out currentSection))
                {
                    currentSection = new InspectorSectionViewModel
                    {
                        Header = sectionAttribute.Header,
                        Order = sectionAttribute.Order
                    };

                    sections.Add(sectionAttribute.Header, currentSection);
                }
            }

            var itemViewModel = GetItemViewModel(property, message);
            if (itemViewModel is null)
            {
                continue;
            }
            
            currentSection.Items.Add(itemViewModel);
        }

        // Ensure the default section is always last, if it has items
        if (defaultSection.Items.Count > 0)
        {
            sections.Add(string.Empty, defaultSection);
        }

        return sections;
    }

    private static InspectorItemViewModel? GetItemViewModel(PropertyInfo property, IPacketMessage message)
    {
        // If the property is not decorated, skip it
        var attr = property.GetCustomAttribute<InspectPropertyAttribute>();
        if (attr is null)
        {
            return null;
        }

        var name = attr.Name ?? property.Name.ToNaturalWording();
        var stringFormat = attr.StringFormat;
        var showHex = attr.ShowHex;

        var value = property.GetValue(message);

        // If the property is a list, build a list model instead
        if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<object>)))
        {
            return BuildListModel(name, value, attr.Order, stringFormat, showHex);
        }

        // If the property is an object, build a dictionary model instead
        if (property.PropertyType.IsClass && value is not null)
        {
            return BuildDictionaryModel(name, value, attr.Order);
        }

        // Apply tooltip if specified
        var toolTipAttribute = property.GetCustomAttribute<InspectToolTipAttribute>();
        
        // Do not reveal by default if masked
        var maskedAttribute = property.GetCustomAttribute<InspectMaskedAttribute>();

        return new InspectorValueViewModel
        {
            Name = name,
            Order = attr.Order,
            Value = value,
            StringFormat = stringFormat,
            ShowHex = showHex,
            ToolTip = toolTipAttribute?.ToolTip,
            MaskCharacter = maskedAttribute?.MaskCharacter,
            IsRevealed = maskedAttribute is null
        };
    }

    private static InspectorListViewModel BuildListModel(string name, object? value, int order = int.MaxValue,
        string? stringFormat = null, bool showHex = false)
    {
        var listViewModel = new InspectorListViewModel
        {
            Name = name,
            Order = order
        };

        var objectCollection = value as IEnumerable<object> ?? [];
        var index = 0;
        foreach (var item in objectCollection)
        {
            var objType = item.GetType();

            if (objType.IsClass)
            {
                var displayName = $"Element {index++}";
                listViewModel.Items.Add(BuildDictionaryModel(displayName, item));
                continue;
            }

            listViewModel.Items.Add(new InspectorValueViewModel
            {
                Name = $"Element {index++}",
                Value = item,
                StringFormat = stringFormat,
                ShowHex = showHex
            });
        }

        return listViewModel;
    }

    private static InspectorDictionaryViewModel BuildDictionaryModel(string name, object objValue,
        int order = int.MaxValue)
    {
        var dictViewModel = new InspectorDictionaryViewModel
        {
            Name = name,
            Order = order
        };

        List<InspectorValueViewModel> keyValues = [];
        foreach (var property in objValue.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = property.GetCustomAttribute<InspectPropertyAttribute>();
            if (attr is null)
            {
                continue;
            }

            var key = attr.Name ?? property.Name.ToNaturalWording();
            var value = property.GetValue(objValue);
            
            var keyValueViewModel = new InspectorValueViewModel
            {
                Name = key,
                Order = attr.Order,
                Value = value,
                StringFormat = attr.StringFormat,
                ShowHex = attr.ShowHex
            };
            
            keyValues.Add(keyValueViewModel);
        }

        // Ensure they are in their right order, ascending
        foreach (var viewModel in keyValues.OrderBy(k => k.Order))
        {
            dictViewModel.Values.Add(viewModel);
        }

        return dictViewModel;
    }
}