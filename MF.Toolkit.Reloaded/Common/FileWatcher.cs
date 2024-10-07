namespace MF.Toolkit.Reloaded.Common;

internal static class FileWatcher
{
    private static List<AltFileSystemWatcher> _watchers = [];

    public static AltFileSystemWatcher CreateWatcher(string file, Action onChanged)
    {
        var watcher = new AltFileSystemWatcher(Path.GetDirectoryName(file)!, Path.GetFileName(file), onChanged)
        {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
        };

        _watchers.Add(watcher); // keep reference for GC
        return watcher;
    }
}

internal class AltFileSystemWatcher : FileSystemWatcher
{
    private readonly System.Timers.Timer _timer = new(500) { AutoReset = false };

    public AltFileSystemWatcher(string directory, string filter, Action onChanged)
        : base(directory, filter)
    {
        this.Changed += (sender, e) => _timer.Restart();
        _timer.Elapsed += (_, _) => onChanged();
    }
}
