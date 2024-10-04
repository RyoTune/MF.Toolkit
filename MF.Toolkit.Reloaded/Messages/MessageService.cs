using MF.Toolkit.Interfaces.Messages;
using MF.Toolkit.Interfaces.Messages.Models;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Messages.Parser;
using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;

namespace MF.Toolkit.Reloaded.Messages;

internal unsafe class MessageService : IMessage, IUseConfig
{
    private delegate MSG* GetItemMsg(int* itemId, MsgFlags* msgFlags);
    private IHook<GetItemMsg>? getItemNameHook;
    private IHook<GetItemMsg>? getItemDescHook;
    private IHook<GetItemMsg>? getItemEffectHook;

    private delegate MSG* CreateMsgFromString(nint str, MsgFlags flags, MsgConfig1* config1, MsgConfig2* config2);
    private CreateMsgFromString? createMsgFromString;

    private delegate nint CompileMsg(nint buffer, nint sourceStr, int numItems);
    private delegate uint CompileMsgFile(nint msgSrc, int msgSrcLength, nint msgFile, int param4, byte param5);
    private IHook<CompileMsgFile>? compileMsgFileHook;

    private readonly MsgConfig1* msgConfig1;
    private readonly MsgConfig2* msgConfig2;
    private readonly Dictionary<int, string> itemNames = [];
    private readonly Dictionary<int, string> itemDescs = [];
    private readonly Dictionary<int, string> itemEffects = [];
    private readonly MessageRegistry registry;
    private bool devMode;

    public MessageService(MessageRegistry registry)
    {
        this.registry = registry;
        this.msgConfig1 = (MsgConfig1*)Marshal.AllocHGlobal(sizeof(MsgConfig1));
        this.msgConfig2 = (MsgConfig2*)Marshal.AllocHGlobal(sizeof(MsgConfig1));
        *this.msgConfig1 = new();
        *this.msgConfig2 = new();

        ScanHooks.Add(
            nameof(CreateMsgFromString),
            "40 53 48 83 EC 30 41 80 79 ?? 00",
            (hooks, result) => this.createMsgFromString = hooks.CreateWrapper<CreateMsgFromString>(result, out _));

        ScanHooks.Add(
            "GetItemNameMsg from FUN_140a545e0",
            "48 83 EC 28 8B 01 FF C8",
            (hooks, result) =>
            {
                var offset = (int*)(result + 0x2d + 1);
                var offsetValue = *offset;
                var funcAddress = offsetValue + (nint)offset + 4;
                this.getItemNameHook = hooks.CreateHook<GetItemMsg>((a, b) => this.GetItemMsgImpl(a, b, ItemText.Name), funcAddress).Activate();
            });

        ScanHooks.Add(
            "GetItemEffectMsg from FUN_140b3cc30",
            "44 89 44 24 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 38 4C 8B F9",
            (hooks, result) =>
            {
                var offset = (int*)(result + 0x8e + 1);
                var offsetValue = *offset;
                var funcAddress = offsetValue + (nint)offset + 4;
                this.getItemEffectHook = hooks.CreateHook<GetItemMsg>((a, b) => this.GetItemMsgImpl(a, b, ItemText.Effect), funcAddress).Activate();
            });

        ScanHooks.Add(
            "GetItemDescriptionMsg",
            "48 89 5C 24 ?? 57 48 83 EC 20 48 8B D9 48 8B FA 8B 09 E8 ?? ?? ?? ?? 83 F8 4D",
            (hooks, result) => this.getItemDescHook = hooks.CreateHook<GetItemMsg>((a, b) => this.GetItemMsgImpl(a, b, ItemText.Description), result).Activate());

        ScanHooks.Add(
            nameof(CompileMsg),
            "48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 56 48 83 EC 60 41 8B F8",
            (hooks, result) => { });

        ScanHooks.Add(
            nameof(CompileMsgFile),
            "48 89 5C 24 ?? 48 89 4C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 ?? 48 81 EC E0 00 00 00 45 8B E1",
            (hooks, result) => this.compileMsgFileHook = hooks.CreateHook<CompileMsgFile>(this.CompileMsgFileImpl, result).Activate());
    }

    private uint CompileMsgFileImpl(nint msgSrc, int msgSrcLength, nint msgFile, int param4, byte param5)
    {
        var msgPath = Marshal.PtrToStringAnsi(msgFile)!;
        if (this.registry.TryGetModDict(msgPath, out var newMsgs) && newMsgs.Count > 0)
        {
            var msgDict = MessageParser.Parse(Marshal.PtrToStringAnsi(msgSrc, msgSrcLength)!);
            msgDict.Merge(newMsgs);
            (var pointer, var length) = msgDict.ToBinary();

            if (this.devMode)
            {
                Log.Information($"Merged MSG: {msgPath} || Param4: {param4} || Param5: {param5}");
            }
            else
            {
                Log.Debug($"Merged MSG: {msgPath} || Param4: {param4} || Param5: {param5}");
            }

            var result = this.compileMsgFileHook!.OriginalFunction(pointer, length, msgFile, param4, param5);
            Marshal.FreeHGlobal(pointer); // Hopefully doesn't cause a crash lol.
            return result;
        }

        if (this.devMode)
        {
            Log.Information($"MSG: {msgPath} || Param4: {param4} || Param5: {param5}");
        }

        return this.compileMsgFileHook!.OriginalFunction(msgSrc, msgSrcLength, msgFile, param4, param5);
    }

    public void SetItemText(int itemId, ItemText type, string text)
    {
        switch (type)
        {
            case ItemText.Name:
                this.itemNames[itemId] = text;
                break;
            case ItemText.Description:
                this.itemDescs[itemId] = text;
                break;
            case ItemText.Effect:
                this.itemEffects[itemId] = text;
                break;
        }
    }
    private MSG* GetItemMsgImpl(int* itemId, MsgFlags* msgFlags, ItemText type)
    {
        if (type == ItemText.Description)
        {
            if (this.itemDescs.TryGetValue(*itemId, out var desc))
            {
                var strPtr = Marshal.StringToHGlobalAnsi(desc);
                var msg = this.createMsgFromString!(strPtr, *msgFlags, this.msgConfig1, this.msgConfig2);
                Marshal.FreeHGlobal(strPtr);
                return msg;
            }

            return this.getItemDescHook!.OriginalFunction(itemId, msgFlags);
        }

        if (type == ItemText.Name)
        {
            if (this.itemNames.TryGetValue(*itemId, out var name))
            {
                var strPtr = Marshal.StringToHGlobalAnsi(name);
                var msg = this.createMsgFromString!(strPtr, *msgFlags, this.msgConfig1, this.msgConfig2);
                Marshal.FreeHGlobal(strPtr);
                return msg;
            }

            return this.getItemNameHook!.OriginalFunction(itemId, msgFlags);
        }

        if (type == ItemText.Effect)
        {
            if (this.itemEffects.TryGetValue(*itemId, out var effect))
            {
                var strPtr = Marshal.StringToHGlobalAnsi(effect);
                var msg = this.createMsgFromString!(strPtr, *msgFlags, this.msgConfig1, this.msgConfig2);
                Marshal.FreeHGlobal(strPtr);
                return msg;
            }

            return this.getItemEffectHook!.OriginalFunction(itemId, msgFlags);
        }

        return this.CreateMsg("Unknown Item MSG");
    }

    public MSG* CreateMsg(string str)
    {
        var strPtr = Marshal.StringToHGlobalAnsi(str);
        var msg = this.createMsgFromString!(strPtr, MsgFlags.Flag2, this.msgConfig1, this.msgConfig2);
        Marshal.FreeHGlobal(strPtr);
        return msg;
    }

    public MSG* CreateMsg(string str, MsgFlags flags, MsgConfig1 config1, MsgConfig2 config2)
    {
        var strPtr = Marshal.StringToHGlobalAnsi(str);
        var c1 = (MsgConfig1*)Marshal.AllocHGlobal(sizeof(MsgConfig1));
        var c2 = (MsgConfig2*)Marshal.AllocHGlobal(sizeof(MsgConfig2));
        *c1 = config1;
        *c2 = config2;

        var msg = this.createMsgFromString!(strPtr, flags, c1, c2);
        Marshal.FreeHGlobal(strPtr);
        Marshal.FreeHGlobal((nint)c1);
        Marshal.FreeHGlobal((nint)c2);
        return msg;
    }

    public void ConfigChanged(Config config) => this.devMode = config.DevMode;

    public void EditMsgFile(string msgFilePath, IEnumerable<Message> msgs) => this.registry.RegisterMessages(msgFilePath, msgs);
}
