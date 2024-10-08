using CriFs.V2.Hook.Interfaces;
using CriFs.V2.Hook.Interfaces.Structs;
using CriFsV2Lib.Definitions;
using System.Diagnostics.CodeAnalysis;

namespace MF.Toolkit.Reloaded.Common;

internal class GameFileProvider : IDisposable
{
    private readonly ICpkReader _reader;
    private readonly CpkCacheEntry _cachedFiles;

    public GameFileProvider(ICriFsRedirectorApi criFs)
    {
        var cpkFile = criFs.GetCpkFilesInGameDir().First();
        var cpkStream = new FileStream(cpkFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        _reader = criFs.GetCriFsLib().CreateCpkReader(cpkStream, false);
        _cachedFiles = criFs.GetCpkFilesCached(cpkFile);
    }

    public bool TryGetFile(string file, [NotNullWhen(true)] out MemoryStream? mStream)
    {
        mStream = null;
        if (_cachedFiles.FilesByPath.TryGetValue(file, out var idx))
        {
            var fileEntry = _cachedFiles.Files[idx];
            using var fileExtract = _reader.ExtractFile(fileEntry.File);
            mStream = new MemoryStream(fileExtract.Span.ToArray());
        }

        return mStream != null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _reader.Dispose();
    }
}
