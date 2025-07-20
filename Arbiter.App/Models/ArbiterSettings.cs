using System;

namespace Arbiter.App.Models;

public class ArbiterSettings
{
    public static readonly string DefaultPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "KRU", "Dark Ages", "Darkages.exe");

    public string ClientExecutablePath { get; set; } = DefaultPath;
}