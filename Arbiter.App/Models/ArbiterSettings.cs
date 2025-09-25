using System;

namespace Arbiter.App.Models;

public class ArbiterSettings : ICloneable
{
    public static readonly string DefaultPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "KRU", "Dark Ages", "Darkages.exe");

    public string ClientExecutablePath { get; set; } = DefaultPath;
    public int LocalPort { get; set; } = 2610;

    public string RemoteServerAddress { get; set; } = "da0.kru.com";
    public int RemoteServerPort { get; set; } = 2610;
    
    public bool TraceOnStartup { get; set; }
    public bool TraceAutosave { get; set; }
    
    public WindowRect? StartupLocation { get; set; }

    public InterfacePanelState? LeftPanel { get; set; }
    public InterfacePanelState? RightPanel { get; set; }
    public InterfacePanelState? BottomPanel { get; set; }

    public object Clone() => new ArbiterSettings
    {
        ClientExecutablePath = ClientExecutablePath,
        LocalPort = LocalPort,
        RemoteServerAddress = RemoteServerAddress,
        RemoteServerPort = RemoteServerPort,
        TraceOnStartup = TraceOnStartup,
        TraceAutosave = TraceAutosave,
        StartupLocation = StartupLocation?.Clone() as WindowRect,
        LeftPanel = LeftPanel?.Clone() as InterfacePanelState,
        RightPanel = RightPanel?.Clone() as InterfacePanelState,
        BottomPanel = BottomPanel?.Clone() as InterfacePanelState,
    };

}