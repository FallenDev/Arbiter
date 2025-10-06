using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbiter.App.Models;

public class ArbiterSettings : ICloneable
{
    public static readonly string DefaultPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "KRU", "Dark Ages", "Darkages.exe");

    public string ClientExecutablePath { get; set; } = DefaultPath;
    public bool SkipIntroVideo { get; set; } = true;
    public bool SuppressLoginNotice { get; set; } = true;

    public int LocalPort { get; set; } = 2610;

    public string RemoteServerAddress { get; set; } = "da0.kru.com";
    public int RemoteServerPort { get; set; } = 2610;

    public bool TraceOnStartup { get; set; }
    public bool TraceAutosave { get; set; }

    public DebugSettings Debug { get; set; } = new();

    public WindowRect? StartupLocation { get; set; }

    public InterfacePanelState? LeftPanel { get; set; }
    public InterfacePanelState? RightPanel { get; set; }
    public InterfacePanelState? BottomPanel { get; set; }

    public List<MessageFilter> MessageFilters { get; set; } = [];

    public object Clone() => new ArbiterSettings
    {
        ClientExecutablePath = ClientExecutablePath,
        SkipIntroVideo = SkipIntroVideo,
        SuppressLoginNotice = SuppressLoginNotice,
        LocalPort = LocalPort,
        RemoteServerAddress = RemoteServerAddress,
        RemoteServerPort = RemoteServerPort,
        TraceOnStartup = TraceOnStartup,
        TraceAutosave = TraceAutosave,
        Debug = Debug.Clone() as DebugSettings ?? new DebugSettings(),
        StartupLocation = StartupLocation?.Clone() as WindowRect,
        LeftPanel = LeftPanel?.Clone() as InterfacePanelState,
        RightPanel = RightPanel?.Clone() as InterfacePanelState,
        BottomPanel = BottomPanel?.Clone() as InterfacePanelState,
        MessageFilters = MessageFilters.Select(x => new MessageFilter { Pattern = x.Pattern }).ToList()
    };
}