using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Metaphor.Models;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using System.Runtime.InteropServices;
using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

namespace MF.Toolkit.Reloaded.Metaphor.Services;

internal unsafe class SpriteService : IUseConfig
{
    [Function(Register.rcx, Register.rax, true)]
    private delegate void SetSpriteSeq(nint spriteSeq);
    private IReverseWrapper<SetSpriteSeq>? _setSpriteWrapper;
    private IAsmHook? _setSeqHook;

    private delegate void PlaySpriteSeq(nint param1, nint spriteStr, nint seqStr, nint param4, nint param5, nint param6, nint param7);
    private IHook<PlaySpriteSeq>? _playSprSeq;

    private readonly List<ReplacementSequence> _replacedSeqs = [];

    private readonly string _modDir;
    private readonly Dictionary<string, List<string>> _sequences = [];
    private bool _isDevMode;
    private bool _shouldDump;
    private bool _hasDump;

    public SpriteService(string modDir)
    {
        _modDir = modDir;
        ScanHooks.Add(
            nameof(SetSpriteSeq),
            "48 89 30 48 89 78 ?? 81 FB CB 12 00 00",
            (hooks, result) =>
            {
                var patch = new string[]
                {
                    "use64",
                    Utilities.PushCallerRegisters,
                    "mov rcx,rax",
                    hooks.Utilities.GetAbsoluteCallMnemonics(SetSpriteSeqImpl, out _setSpriteWrapper),
                    Utilities.PopCallerRegisters,
                };

                _setSeqHook = hooks.CreateAsmHook(patch, result, AsmHookBehaviour.ExecuteAfter).Activate();
            });

        ScanHooks.Add(
            nameof(PlaySpriteSeq),
            "40 53 55 56 57 48 83 EC 68 48 8B 05 ?? ?? ?? ?? 48 31 E0",
            (hooks, result) => _playSprSeq = hooks.CreateHook<PlaySpriteSeq>(PlaySpriteSeqImpl, result).Activate());
    }

    public void ReplaceSpriteSeq(string ogSeq, string newSeqSprite, string newSeq) => _replacedSeqs.Add(new(ogSeq, newSeqSprite, newSeq));

    private void SetSpriteSeqImpl(nint spriteSeq)
    {
        var seqStr = (nint*)spriteSeq;
        var seqStrLen = (nint*)(spriteSeq + 8);

        var seq = Marshal.PtrToStringAnsi(*seqStr)!;
        var seqParts = seq.Split('/');

        var seqName = seqParts[0];

        if (_sequences.TryGetValue(seqName, out var seqs))
        {
            if (!seqs.Contains(seq))
            {
                seqs.Add(seq);
            }
        }
        else
        {
            _sequences[seqName] = [seq];
        }

        var replacementSeq = _replacedSeqs.FirstOrDefault(x => x.OriginalSeq == seq);
        if (replacementSeq != null && replacementSeq.NewSeq != "NONE")
        {
            *seqStr = StringsCache.GetStringPtr(replacementSeq.NewSeq);
            *seqStrLen = replacementSeq.NewSeq.Length;
        }
    }

    private void PlaySpriteSeqImpl(nint param1, nint spriteStr, nint seqStr, nint param4, nint param5, nint param6, nint param7)
    {
        var ogSpr = Marshal.PtrToStringAnsi(spriteStr)!;
        var ogSeq = Marshal.PtrToStringAnsi(seqStr)!;

        if (_isDevMode)
        {
            Log.Information($"Playing Sprite: {ogSpr} || {ogSeq}");
        }

        if (_shouldDump && !_hasDump)
        {
            JsonFileSerializer.Serialize(Path.Join(_modDir, "dump", "sprites.json"), _sequences);
            _hasDump = true;
        }

        // Use known sprite name from replacement seq
        // to fix sprite/seq mismatches.
        var replacementSeq = _replacedSeqs.FirstOrDefault(x => x.NewSeq == ogSeq);
        if (replacementSeq != null)
        {
            var newSprStr = StringsCache.GetStringPtr(replacementSeq.NewSeqSprite);
            var newSeqStr = StringsCache.GetStringPtr(replacementSeq.NewSeq);

            Log.Debug($"SpriteSequence: {ogSpr} -> {replacementSeq.NewSeqSprite} || {ogSeq} -> {replacementSeq.NewSeq}");
            _playSprSeq!.OriginalFunction(param1, newSprStr, newSeqStr, param4, param5, param6, param7);
            return;
        }

        var isSeqDisabled = _replacedSeqs.FindIndex(x => x.OriginalSeq == ogSeq && x.NewSeq == "NONE") != -1;
        if (isSeqDisabled)
        {
            return;
        }


        _playSprSeq!.OriginalFunction(param1, spriteStr, seqStr, param4, param5, param6, param7);
    }

    public void ConfigChanged(Config config)
    {
        _isDevMode = config.DevMode;
        _shouldDump = config.DumpData;
    }
}
