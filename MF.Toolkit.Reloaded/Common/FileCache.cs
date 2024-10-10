using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace MF.Toolkit.Reloaded.Common;

internal class FileCache
{
    private readonly string _cacheDir;
    private readonly string _cacheInfoFile;
    private readonly ConcurrentDictionary<string, FileCacheInfo> _cache;

    public FileCache(string modDir)
    {
        _cacheDir = Path.Join(modDir, "cache");
        Directory.CreateDirectory(_cacheDir);

        _cacheInfoFile = Path.Join(_cacheDir, "cache.json");
        _cache = GetCache();
    }

    /// <summary>
    /// Gets the cached file path for the given input file.
    /// </summary>
    /// <param name="inputFile">Input file.</param>
    /// <returns>Cached file path for input.</returns>
    public string PathFor(string inputFile)
    {
        var hash = MD5.HashData(Encoding.ASCII.GetBytes(inputFile));
        var hashStr = BitConverter.ToString(hash).Replace("-", string.Empty);
        return Path.Join(_cacheDir, hashStr);
    }

    /// <summary>
    /// Checks if the cached file for the given input file
    /// is still valid.
    /// </summary>
    /// <param name="inputFile">Input file.</param>
    /// <returns>Whether the cached file is valid.</returns>
    public bool IsValid(string inputFile)
    {
        var cachedFile = PathFor(inputFile);
        if (!File.Exists(cachedFile))
        {
            return false;
        }

        if (_cache.TryGetValue(inputFile, out var cacheInfo))
        {
            var currInfo = new FileInfo(inputFile);
            return currInfo.LastWriteTime == cacheInfo.LastWriteTime && currInfo.Length == cacheInfo.Length;
        }

        return false;
    }

    public void UpdateFile(string inputFile)
    {
        var fileInfo = new FileInfo(inputFile);
        _cache[inputFile] = new(fileInfo.LastWriteTime, fileInfo.Length);
    }

    public void SaveCache() => JsonFileSerializer.Serialize(_cacheInfoFile, _cache);

    private ConcurrentDictionary<string, FileCacheInfo> GetCache()
    {
        if (File.Exists(_cacheInfoFile))
        {
            return JsonFileSerializer.Deserialize<ConcurrentDictionary<string, FileCacheInfo>>(_cacheInfoFile);
        }

        return [];
    }

    private record FileCacheInfo(DateTime LastWriteTime, long Length);
}
