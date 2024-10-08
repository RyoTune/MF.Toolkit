using CNutSharp.Library.Models;
using CriFs.V2.Hook.Interfaces;
using MF.Toolkit.Reloaded.Common;

namespace MF.Toolkit.Reloaded.Squirrel.Scripts;

internal class ScriptsRegistry : IRegisterMod
{
    private readonly ICriFsRedirectorApi _criFs;
    private readonly GameFileProvider _fileProvider;
    private readonly string _bindDir;
    private readonly Dictionary<string, List<CNut>> _mergeNuts = new(StringComparer.OrdinalIgnoreCase);

    public ScriptsRegistry(
        ICriFsRedirectorApi criFs,
        GameFileProvider fileProvider,
        string modDir)
    {
        _criFs = criFs;
        _fileProvider = fileProvider;
        _bindDir = criFs.GenerateBindingDirectory(modDir);
    }

    public void RegisterFolder(string folder)
    {
        foreach (var modCnutFile in Directory.EnumerateFiles(folder, "*.cnut", SearchOption.AllDirectories))
        {
            var nutPath = Path.GetRelativePath(folder, modCnutFile);
            var modCnut = new CNut(modCnutFile);
            if (_mergeNuts.TryGetValue(nutPath, out var existing))
            {
                existing.Add(modCnut);
            }
            else
            {
                _mergeNuts[nutPath] = [modCnut];

                // Register bind here since CriFs won't allow it
                // after mods are initialized.
                var nutOutFile = Path.Join(_bindDir, nutPath);
                _criFs.AddBind(nutPath, nutOutFile);
            }

            Log.Information($"Registered Nut: {modCnutFile}");
        }
    }

    public void MergeNuts()
    {
        foreach (var item in _mergeNuts)
        {
            (var nutPath, var modNuts) = item;
            if (!_fileProvider.TryGetFile(nutPath, out var nutStream))
            {
                Log.Error($"Original \".cnut\" file not found.\nFile: {nutPath}");
                return;
            }

            var ogNut = new CNut(nutStream);
            foreach (var nut in modNuts)
            {
                ogNut.Merge(nut);
            }

            var nutOutFile = Path.Join(_bindDir, nutPath);
            Directory.CreateDirectory(Path.GetDirectoryName(nutOutFile)!);

            using var file = File.OpenWrite(nutOutFile);
            ogNut.Write(file);
        }
    }

    public void RegisterMod(string modId, string metaDir) => RegisterFolder(metaDir);
}