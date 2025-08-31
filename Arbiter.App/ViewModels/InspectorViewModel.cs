using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arbiter.App.Annotations;
using Arbiter.App.Extensions;
using Arbiter.App.Models;
using Arbiter.App.Models.Packets;
using Arbiter.Net;
using Arbiter.Net.Client;
using Arbiter.Net.Server;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arbiter.App.ViewModels;

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

            // Apply tooltip if specified
            var toolTipAttribute = property.GetCustomAttribute<InspectToolTipAttribute>();
            if (toolTipAttribute is not null && !string.IsNullOrWhiteSpace(toolTipAttribute.ToolTip))
            {
                itemViewModel.ToolTip = toolTipAttribute.ToolTip;
            }
            
            // Do not reveal by default if masked
            var maskedAttribute = property.GetCustomAttribute<InspectMaskedAttribute>();
            if (maskedAttribute is not null)
            {
                itemViewModel.MaskCharacter = maskedAttribute.MaskCharacter;
                itemViewModel.IsRevealed = false;
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

        var value = property.GetValue(message);

        return new InspectorValueViewModel
        {
            Name = attr.Name ?? property.Name.ToNaturalWording(),
            Value = value,
            StringFormat = attr.StringFormat,
            ShowHex = attr.ShowHex
        };
    }

}