# Arbiter

Network Analyzer Tool for Dark Ages

Written in .NET + [Avalonia](https://docs.avaloniaui.net/docs/welcome), using [MVVM](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) patterns.
Custom UI styling based on [Godot's](https://godotengine.org/) UI look and feel.

---

<img src="docs/src/screenshots/Arbiter.png"/>

## Requirements ✅

- [Dark Ages](https://www.darkages.com) Client 7.41 (current latest)
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- macOS[^1], Linux[^1], Windows

[^1]: Launching a game client is only supported on Windows.
  You can still analyze network packet dumps or redirect clients from other platforms or VMs.

## Installation 💾

1. Download the [latest release](https://github.com/ewrogers/Arbiter/releases/)
2. Extract all files to `C:\Arbiter` (or your choosing)
3. Open `Arbiter.exe`
4. Configure your DA installation path in `Settings` (if different)

## Documentation 📚

TBD

## Contributing 👨🏻‍💻

I welcome contributions to this project! Please open an issue or pull request if you have any suggestions, fixes, or improvements.

I personally use JetBrains Rider for development, but any editor should work as long as you install the appropriate Avalonia plugins.

## Packaging 📦

To package and deploy the application binary as a single-file executable, use the following command:

```powershell
cd Arbiter.App
dotnet publish -r win-x64 -c Release -p:PublishSingleFile=true --self-contained false
```

> [!IMPORTANT]
> You must include the published `.dll` files with the executable.

## Attribution 🙏🏻

Special thanks to [Chaos Server](https://github.com/Sichii/Chaos-Server) and [Hybrasyl](https://github.com/hybrasyl/server) repositories for many of the packet structures.
