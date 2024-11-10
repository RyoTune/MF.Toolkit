using MF.Toolkit.Interfaces.Library;
using MF.Toolkit.Interfaces.Metaphor.Models;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Metaphor.Models;
using Reloaded.Hooks.Definitions;

namespace MF.Toolkit.Reloaded.Metaphor.Services;

internal class BitService : IUseConfig
{
    private delegate int BitGet(int bit);
    private IHook<BitGet>? _bitGetHook;

    private delegate void BitSet(int bit, bool value);
    private IHook<BitSet>? _bitSetHook;

    private readonly IMetaphorLibrary _metaLib;
    private readonly Dictionary<int, bool> _bitOverrides = [];
    private readonly List<Action<GameDlc[]>> dlcChecks = [];
    private readonly Dictionary<int, GameDlc> _dlcBits = new()
    {
        [8918] = GameDlc.P1,
        [8919] = GameDlc.P2,
        [8920] = GameDlc.P3,
        [8921] = GameDlc.P4,
        [8922] = GameDlc.P5,
        [8923] = GameDlc.SMT4,
        [8924] = GameDlc.SMT5,
        [8925] = GameDlc.EO,
    };

    private GameVarLogMode _logBits;
    private bool _shouldEditData;

    public BitService(IMetaphorLibrary metaLib)
    {
        _metaLib = metaLib;

        ScanHooks.Add(
            nameof(BitGet),
            "8B C1 99 83 E2 1F 03 C2 8B C8 83 E0 1F 2B C2",
            (hooks, result) => _bitGetHook = hooks.CreateHook<BitGet>(BitGetImpl, result));

        ScanHooks.Add(
            nameof(BitSet),
            "40 53 48 83 EC 20 44 0F B6 CA",
            (hooks, result) => _bitSetHook = hooks.CreateHook<BitSet>(BitSetImpl, result));
    }

    internal void CheckDlc(Action<GameDlc[]> dlcResult) => dlcChecks.Add(dlcResult);

    public void SetBit(int bit, bool value)
    {
        _bitOverrides[bit] = value;
        Log.Information($"Set Bit: {bit} = {value}");
    }

    private void BitSetImpl(int bit, bool value)
    {
        _bitSetHook!.OriginalFunction(bit, value);
        if (_logBits == GameVarLogMode.SetOnly || _logBits == GameVarLogMode.Both)
        {
            Log.Information($"{nameof(BitSet)} || Bit: {bit} = {value}");
        }

        if (_dlcBits.TryGetValue(bit, out var dlc))
        {
            Log.Information($"DLC: {dlc} owned = {value}");
        }
    }

    private int BitGetImpl(int bit)
    {
        var ogValue = _bitGetHook!.OriginalFunction(bit) == 1;
        var isReplaced = _bitOverrides.TryGetValue(bit, out var newValue);
        if (_logBits == GameVarLogMode.GetOnly || _logBits == GameVarLogMode.Both)
        {
            if (isReplaced)
            {
                Log.Information($"{nameof(BitGet)} || Bit: {bit} = {newValue} (Original: {ogValue}");
            }
            else
            {
                Log.Information($"{nameof(BitGet)} || Bit: {bit} = {ogValue}");
            }
        }

        if (isReplaced && _shouldEditData)
        {
            if (newValue)
            {
                _metaLib.BitOn(bit, 1);
            }
            else
            {
                _metaLib.BitOff(bit);
            }
        }

        return isReplaced ? Convert.ToInt32(newValue) : Convert.ToInt32(ogValue);
    }

    public void ConfigChanged(Config config)
    {
        _logBits = config.ShowBits;
        _shouldEditData = config.ShouldEditData;
    }
}
