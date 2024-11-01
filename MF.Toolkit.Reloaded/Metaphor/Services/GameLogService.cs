using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using SharedScans.Interfaces;
using System.Runtime.InteropServices;
using static MF.Toolkit.Interfaces.Library.MetaphorDefinitions;

namespace MF.Toolkit.Reloaded.Metaphor.Services;

internal class GameLogService : IUseConfig
{
    private bool _isDevMode;

    public GameLogService(ISharedScans scans)
    {
        scans.CreateHook<PrintLog>((a) => Print(a, LogType.Log), Mod.NAME);
        scans.CreateHook<PrintLogSub>((a) => Print(a, LogType.LogSub), Mod.NAME);
        scans.CreateHook<AssertPrint>(AssertPrintImpl, Mod.NAME);
    }

    private void AssertPrintImpl(int param1, nint param2)
    {
        if (_isDevMode)
        {
            Log.Information($"AssertPrint: {Marshal.PtrToStringAnsi(param2)} || {param1}");
        }
    }

    private void Print(nint param1, LogType type)
    {
        if (_isDevMode)
        {
            Log.Information($"{type}: {Marshal.PtrToStringAnsi(param1)}");
        }
    }

    public void ConfigChanged(Config config) => _isDevMode = config.DevMode;

    private enum LogType
    {
        Log,
        LogSub,
    }
}
