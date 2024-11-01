using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;

namespace MF.Toolkit.Reloaded.Metaphor.Services;

internal unsafe class CriFileService
{
    private delegate int criFsLoader_Load(nint loader, nint binder, nint path, nint offset, nint load_size, nint buffer, nint buffer_size);
    private IHook<criFsLoader_Load>? _loadHook;

    private delegate int criFsBinder_Find(nint binderHn, nint filePath, nint fileInfo, nint exist);
    private IHook<criFsBinder_Find>? _findHook;

    private readonly Dictionary<string, nint> _fileRedirects = new(StringComparer.OrdinalIgnoreCase);

    public CriFileService()
    {
        ScanHooks.Add(
            nameof(criFsLoader_Load),
            "48 89 5C 24 ?? 57 48 83 EC 30 49 8B F9 48 8B D9 48 85 C9 74 ?? 4D 85 C0",
            (hooks, result) => _loadHook = hooks.CreateHook<criFsLoader_Load>(criFsLoader_LoadImpl, result).Activate());

        ScanHooks.Add(
            nameof(criFsBinder_Find),
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 40 49 8B F9 49 8B D8 48 8B F2",
            (hooks, result) => _findHook = hooks.CreateHook<criFsBinder_Find>(criFsBinder_FindImpl, result).Activate());
    }

    private int criFsBinder_FindImpl(nint binderHn, nint filePath, nint fileInfo, nint exist)
    {
        var ogPath = Marshal.PtrToStringAnsi(filePath)!;
        if (_fileRedirects.TryGetValue(ogPath, out var newPath))
        {
            return _findHook!.OriginalFunction(binderHn, newPath, fileInfo, exist);
        }

        return _findHook!.OriginalFunction(binderHn, filePath, fileInfo, exist);
    }

    private int criFsLoader_LoadImpl(nint loader, nint binder, nint path, nint offset, nint load_size, nint buffer, nint buffer_size)
    {
        var ogPath = Marshal.PtrToStringAnsi(path)!;
        if (_fileRedirects.TryGetValue(ogPath, out var newPath))
        {
            return _loadHook!.OriginalFunction(loader, binder, newPath, offset, load_size, buffer, buffer_size);
        }

        return _loadHook!.OriginalFunction(loader, binder, path, offset, load_size, buffer, buffer_size);
    }

    public void RedirectFile(string ogPath, string newPath)
    {
        Log.Information($"File: {ogPath}\nRedirect: {newPath}");
        if (_fileRedirects.TryGetValue(ogPath, out var prevPathPtr))
        {
            Marshal.FreeHGlobal(prevPathPtr);
        }

        _fileRedirects[ogPath] = Marshal.StringToHGlobalAnsi(newPath);
    }
}
