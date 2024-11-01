using MF.Toolkit.Interfaces.Library;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Metaphor.Models;
using Reloaded.Hooks.Definitions;

namespace MF.Toolkit.Reloaded.Metaphor;

internal class CounterService : IUseConfig
{
    private delegate int CounterGet(int counter);
    private IHook<CounterGet>? _counterGetHook;

    private delegate void CounterSet(int counter, int value);
    private IHook<CounterSet>? _counterSetHook;

    private readonly IMetaphorLibrary _metaLib;
    private readonly Dictionary<int, int> _counterOverrides = [];
    private GameVarLogMode _logCounters;
    private bool _shouldEditData;

    public CounterService(IMetaphorLibrary metaLib)
    {
        _metaLib = metaLib;

        ScanHooks.Add(
            nameof(CounterGet),
            "48 8B 05 ?? ?? ?? ?? 48 63 D1 8B 84 ?? f4 33 02 00",
            (hooks, result) => _counterGetHook = hooks.CreateHook<CounterGet>(CounterGetImpl, result).Activate());

        ScanHooks.Add(
            nameof(CounterSet),
            "48 8B 05 ?? ?? ?? ?? 4C 63 C1 42 89 94 ?? F4 33 02 00",
            (hooks, result) => _counterSetHook = hooks.CreateHook<CounterSet>(CounterSetImpl, result).Activate());
    }

    public void SetCounter(int counter, int value) => _counterOverrides[counter] = value;

    private void CounterSetImpl(int counter, int value)
    {
        _counterSetHook!.OriginalFunction(counter, value);
        if (_logCounters == GameVarLogMode.SetOnly || _logCounters == GameVarLogMode.Both)
        {
            Log.Information($"{nameof(CounterSet)} || Counter: {counter} = {value}");
        }
    }

    private int CounterGetImpl(int counter)
    {
        var ogValue = _counterGetHook!.OriginalFunction(counter);
        var isReplaced = _counterOverrides.TryGetValue(counter, out var newValue);
        if (_logCounters == GameVarLogMode.GetOnly || _logCounters == GameVarLogMode.Both)
        {
            if (isReplaced)
            {
                Log.Information($"{nameof(CounterGet)} || Counter: {counter} = {newValue} (Original: {ogValue}");
            }
            else
            {
                Log.Information($"{nameof(CounterGet)} || Counter: {counter} = {ogValue}");
            }
        }

        if (isReplaced && _shouldEditData)
        {
            _metaLib.CounterSet(counter, newValue);
        }

        return isReplaced ? newValue : ogValue;
    }

    public void ConfigChanged(Config config)
    {
        _logCounters = config.ShowCounters;
        _shouldEditData = config.ShouldEditData;
    }
}
