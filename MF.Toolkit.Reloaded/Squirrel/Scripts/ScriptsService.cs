using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;

namespace MF.Toolkit.Reloaded.Squirrel.Scripts;

internal class ScriptsService
{
    private delegate nint criFsLoader_Load(nint loader, nint binder, nint path, nint offset, nint load_size, nint buffer, nint buffer_size);
    private IHook<criFsLoader_Load>? _loadHook;
    private readonly ScriptsRegistry _registry;

    public ScriptsService(ScriptsRegistry registry)
    {
        _registry = registry;

        ScanHooks.Add(
            nameof(criFsLoader_Load),
            "48 89 5C 24 ?? 57 48 83 EC 30 49 8B F9 48 8B D9 48 85 C9 74 ?? 4D 85 C0",
            (hooks, result) => _loadHook = hooks.CreateHook<criFsLoader_Load>(Load, result));
    }

    private nint Load(nint loader, nint binder, nint path, nint offset, nint load_size, nint buffer, nint buffer_size)
    {
        var pathStr = Marshal.PtrToStringAnsi(path)!;
        if (Path.GetExtension(pathStr).Equals(".cnut"))
        {
            Log.Information($"{pathStr}: 0x{buffer:X}");
        }

        return _loadHook!.OriginalFunction(loader, binder, path, offset, load_size, buffer, buffer_size);
    }
}
