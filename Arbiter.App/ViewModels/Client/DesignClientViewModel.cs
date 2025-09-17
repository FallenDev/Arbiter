using System.Net.Sockets;
using Arbiter.Net;

namespace Arbiter.App.ViewModels.Client;

// This is only used at design-time to make it easier to adjust XAML view layout.
public class DesignClientViewModel : ClientViewModel
{
    public DesignClientViewModel()
        : base(new ProxyConnection(0, new TcpClient()))
    {
        EntityId = 0xFEEDBEEF;
        Name = "VeryLongName";
        Class = "Summoner";
        MapName = "Black Dragon Vestibule";
        Level = 99;
        AbilityLevel = 99;
        MapId = 99999;
        MapX = 100;
        MapY = 100;
        CurrentHealth = 123_456;
        MaxHealth = 234_789;
        CurrentMana = 123_456;
        MaxMana = 234_789;
    }
}