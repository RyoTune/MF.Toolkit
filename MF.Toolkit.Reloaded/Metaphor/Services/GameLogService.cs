using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using SharedScans.Interfaces;
using System.Runtime.InteropServices;
using static MF.Toolkit.Interfaces.Library.MetaphorDefinitions;

namespace MF.Toolkit.Reloaded.Metaphor.Services;

internal class GameLogService : IUseConfig
{
    private bool _isDevMode;
    private bool _showGameLogs;

    public GameLogService(ISharedScans scans)
    {
        scans.CreateHook<PrintLog>((a) => Print(a, LogType.Log), Mod.NAME);
        scans.CreateHook<PrintLogSub>((a) => Print(a, LogType.LogSub), Mod.NAME);
        scans.CreateHook<AssertPrint>(AssertPrintImpl, Mod.NAME);
    }

    private void AssertPrintImpl(int param1, nint param2)
    {
        if (_isDevMode && _showGameLogs)
        {
            Log.Information($"AssertPrint: {Marshal.PtrToStringAnsi(param2)} || {param1}");
        }
    }

    private void Print(nint param1, LogType type)
    {
        if (_isDevMode && _showGameLogs)
        {
            Log.Information($"{type}: {Marshal.PtrToStringAnsi(param1)}");
        }
    }

    public void ConfigChanged(Config config)
    {
        _isDevMode = config.DevMode;
        _showGameLogs = config.ShowGameLogs;
    }

    private enum LogType
    {
        Log,
        LogSub,
    }
}
