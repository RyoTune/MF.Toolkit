using CNutSharp.Library.Models;
using CNutSharp.Library.Squirrel;
using CriFs.V2.Hook.Interfaces;
using MF.Toolkit.Reloaded.Common;

namespace MF.Toolkit.Reloaded.Squirrel.Scripts;

internal class ScriptsRegistry : IRegisterMod
{
    private readonly MLogger _mlog;
    private readonly FileCache _cache;
    private readonly ICriFsRedirectorApi _criFs;
    private readonly GameFileProvider _fileProvider;
    private readonly SqCompiler _sqCompiler;
    private readonly string _bindDir;

    private readonly List<Task> _nutCompileTasks = [];
    private readonly Dictionary<string, List<string>> _mergeCnuts = new(StringComparer.OrdinalIgnoreCase);

    public ScriptsRegistry(
        ICriFsRedirectorApi criFs,
        GameFileProvider fileProvider,
        SqCompiler sqCompiler,
        string modDir)
    {
        _mlog = new MLogger();
        _cache = new FileCache(modDir);
        _criFs = criFs;
        _fileProvider = fileProvider;
        _sqCompiler = sqCompiler;
        _bindDir = criFs.GenerateBindingDirectory(modDir);
    }

    public void RegisterFolder(string folder)
    {
        foreach (var nutFile in Directory.EnumerateFiles(folder, "*.nut", SearchOption.AllDirectories))
        {
            var cnutPath = Path.GetRelativePath(folder, ToCnut(nutFile));
            var cacheCnutFile = _cache.PathFor(nutFile);

            if (_mergeCnuts.TryGetValue(cnutPath, out var modCnutFiles))
            {
                modCnutFiles.Add(cacheCnutFile);
            }
            else
            {
                _mergeCnuts[cnutPath] = [cacheCnutFile];

                // Register bind here since CriFs won't allow it
                // after mods are initialized.
                var cnutBindFile = GetBindCnutFile(cnutPath);
                _criFs.AddBind(cnutPath, cnutBindFile);
            }

            // Compile NUT file if cache is invalid.
            if (!_cache.IsValid(nutFile))
            {
                var compileTask = Task.Run(async () =>
                {
                    var success = await _sqCompiler.TryCompile(nutFile, cacheCnutFile);
                    if (success)
                    {
                        _cache.UpdateFile(nutFile);
                        Log.Information($"Compiled NUT.\nFile: {nutFile}");
                    }
                });

                _nutCompileTasks.Add(compileTask);
            }
            else
            {
                Log.Information($"Cached NUT\nFile: {nutFile}");
            }
        }
    }

    public void MergeNuts()
    {
        Task.WhenAll(_nutCompileTasks).Wait();
        foreach (var item in _mergeCnuts)
        {
            (var cnutPath, var modCnutFiles) = item;
            if (!_fileProvider.TryGetFile(cnutPath, out var mStream))
            {
                Log.Error($"Original \".cnut\" file not found.\nFile: {cnutPath}");
                return;
            }

            var ogCnut = new CNut(mStream, _mlog);
            foreach (var cnutFile in modCnutFiles)
            {
                if (File.Exists(cnutFile))
                {
                    var modCnut = new CNut(cnutFile);
                    ogCnut.Merge(modCnut);
                }
            }

            var cnutOutFile = GetBindCnutFile(cnutPath);
            Directory.CreateDirectory(Path.GetDirectoryName(cnutOutFile)!);

            using var file = File.OpenWrite(cnutOutFile);
            ogCnut.Write(file);
        }

        _cache.SaveCache();
    }

    public void RegisterMod(string modId, string metaDir) => RegisterFolder(metaDir);

    private string GetBindCnutFile(string cnutPath) => Path.Join(_bindDir, cnutPath);

    private static string ToCnut(string nutPath) => Path.ChangeExtension(nutPath, ".cnut");
}