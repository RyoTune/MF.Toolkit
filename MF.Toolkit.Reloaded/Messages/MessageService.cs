using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Interfaces.Messages;
using MF.Toolkit.Interfaces.Messages.Models;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Messages.Models;
using MF.Toolkit.Reloaded.Messages.Models.ItemMessages;
using MF.Toolkit.Reloaded.Messages.Models.MessageLists;
using MF.Toolkit.Reloaded.Messages.Models.MessageProviders;
using Reloaded.Hooks.Definitions;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MF.Toolkit.Reloaded.Messages;

internal unsafe class MessageService : IMessage, IUseConfig
{
    private static readonly Message SYSTEM_ERROR = new()
    {
        Label = nameof(SYSTEM_ERROR),
        Content = "ERROR",
    };

    private delegate MSG* GetItemMsg(int* itemId, MsgFlag* msgFlags);
    private IHook<GetItemMsg>? getItemNameHook;
    private IHook<GetItemMsg>? getItemDescHook;
    private IHook<GetItemMsg>? getItemEffectHook;

    private delegate MSG* CreateMsgFromString(nint str, MsgFlag flags, MsgConfig1* config1, MsgConfig2* config2);
    private CreateMsgFromString? createMsgFromString;

    private delegate MSG* GetMessageByLabelIndex(int* msgId, int labelIdx, MsgFlag* flags, MsgConfigs* configs);

    private delegate nint GetMsgBySerialId(int* param1, int* param2, nint param3);

    private delegate nint CompileMsg(nint buffer, nint sourceStr, int length);
    private delegate uint CompileMsgFile(nint msgSrc, int msgSrcLength, nint msgFile, int param4, byte param5);
    private IHook<CompileMsgFile>? compileMsgFileHook;
    private GetMessageByLabelIndex? getMsgByLabel;

    private record MsgTableEntry(int Id, GameMessageList Msg);
    private readonly ConcurrentDictionary<string, MsgTableEntry> msgTable = new(StringComparer.OrdinalIgnoreCase);

    private readonly MsgConfig1* msgConfig1;
    private readonly MsgConfig2* msgConfig2;
    private readonly Dictionary<int, string> itemNames = [];
    private readonly Dictionary<int, string> itemDescs = [];
    private readonly Dictionary<int, string> itemEffects = [];
    private readonly MessageRegistry _registry;
    private bool devMode;

    public MessageService(MessageRegistry registry)
    {
        _registry = registry;
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
                this.getItemNameHook = hooks.CreateHook<GetItemMsg>((a, b) => this.GetItemMsgImpl(a, b, ItemMsg.Name), funcAddress).Activate();
            });

        ScanHooks.Add(
            "GetItemEffectMsg from FUN_140b3cc30",
            "44 89 44 24 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 38 4C 8B F9",
            (hooks, result) =>
            {
                var offset = (int*)(result + 0x8e + 1);
                var offsetValue = *offset;
                var funcAddress = offsetValue + (nint)offset + 4;
                this.getItemEffectHook = hooks.CreateHook<GetItemMsg>((a, b) => this.GetItemMsgImpl(a, b, ItemMsg.Effect), funcAddress).Activate();
            });

        ScanHooks.Add(
            "GetItemDescriptionMsg",
            "48 89 5C 24 ?? 57 48 83 EC 20 48 89 CB 48 89 D7 8B 09 E8",
            (hooks, result) => this.getItemDescHook = hooks.CreateHook<GetItemMsg>((a, b) => this.GetItemMsgImpl(a, b, ItemMsg.Description), result).Activate());

        ScanHooks.Add(
            nameof(CompileMsgFile),
            "48 89 5C 24 ?? 48 89 4C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 ?? 48 81 EC E0 00 00 00 45 8B E1",
            (hooks, result) => this.compileMsgFileHook = hooks.CreateHook<CompileMsgFile>(this.CompileMsgFileImpl, result).Activate());

        ScanHooks.Add(
            nameof(GetMessageByLabelIndex),
            "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 56 41 57 48 8B EC 48 81 EC 80 00 00 00 49 8B F1",
            (hooks, result) => this.getMsgByLabel = hooks.CreateWrapper<GetMessageByLabelIndex>(result, out _));
    }

    public MSG* GetMessageByLabelIdImpl(int msgId, int labelIdx, MsgFlag flags, MsgConfigs* configs)
    {
        var msgIdPtr = (int*)Marshal.AllocHGlobal(sizeof(int));
        var flagsPtr = (MsgFlag*)Marshal.AllocHGlobal(sizeof(int));

        *msgIdPtr = msgId;
        *flagsPtr = flags;
        return this.getMsgByLabel!(msgIdPtr, labelIdx, flagsPtr, configs);
    }

    private uint CompileMsgFileImpl(nint msgSrc, int msgSrcLength, nint msgFile, int id, byte param5)
    {
        var msgPath = Marshal.PtrToStringAnsi(msgFile)!;
        var gameMessages = new GameMessageList(msgSrc, msgSrcLength);
        this.msgTable[MsgUtils.ToLangAgnostic(msgPath)] = new(id - 1, gameMessages);

        if (this._registry.TryGetModMessages(msgPath, out var messages))
        {
            gameMessages.Merge(messages);
            var newMsgSrc = gameMessages.ToMemory();
            if (this.devMode)
            {
                Log.Information($"Merged MSG: {msgPath} || ID: {id} || Param5: {param5}");
            }
            else
            {
                Log.Debug($"Merged MSG: {msgPath} || ID: {id} || Param5: {param5}");
            }

            var result = this.compileMsgFileHook!.OriginalFunction(newMsgSrc.Pointer, newMsgSrc.Length, msgFile, id, param5);
            Marshal.FreeHGlobal(newMsgSrc.Pointer); // Hopefully doesn't cause a crash lol.
            return result;
        }

        if (this.devMode)
        {
            Log.Information($"MSG: {msgPath} || ID: {id} || Param5: {param5}");
        }

        return this.compileMsgFileHook!.OriginalFunction(msgSrc, msgSrcLength, msgFile, id, param5);
    }

    public void SetItemMessage(int itemId, ItemMsg type, string label)
    {
        switch (type)
        {
            case ItemMsg.Name:
                this.itemNames[itemId] = label;
                break;
            case ItemMsg.Description:
                this.itemDescs[itemId] = label;
                break;
            case ItemMsg.Effect:
                this.itemEffects[itemId] = label;
                break;
        }
    }

    public MSG* GetMessage(string msgPath, string label, MsgFlag msgFlags)
    {
        var msgPathAnostic = MsgUtils.ToLangAgnostic(msgPath);
        if (this.msgTable.TryGetValue(msgPathAnostic, out var msg))
        {
            var id = msg.Msg.GetLabelId(label);
            if (id != -1)
            {
                return this.GetMessageByLabelIdImpl(msg.Id, id, msgFlags, (MsgConfigs*)0);
            }

            Log.Warning($"Label not found. || MSG: {msgPathAnostic} || Label: {label}");
        }
        else
        {
            Log.Warning($"MSG not found. || MSG: {msgPathAnostic}");
        }

        Log.Warning($"Failed to get message.");
        return null;
    }

    public MSG* GetMessage(string msgPath, int labelId, MsgFlag msgFlags)
    {
        if (this.msgTable.TryGetValue(msgPath, out var msg))
        {
            return this.GetMessageByLabelIdImpl(msg.Id, labelId, msgFlags, (MsgConfigs*)0);
        }

        Log.Warning($"Failed to get message. || MSG: {msgPath} || Label ID: {labelId}");
        return null;
    }

    private MSG* GetItemMsgImpl(int* itemId, MsgFlag* msgFlags, ItemMsg type)
    {
        var msgPath = MsgUtils.GetItemMsgPath(Language.Any, type);
        if (type == ItemMsg.Name)
        {
            if (this.itemNames.TryGetValue(*itemId, out var nameLabel))
            {
                var nameMsg = this.GetMessage(msgPath, nameLabel, *msgFlags);
                if (nameMsg != null)
                {
                    return nameMsg;
                }
            }

            return this.getItemNameHook!.OriginalFunction(itemId, msgFlags);
        }

        if (type == ItemMsg.Description)
        {
            if (this.itemDescs.TryGetValue(*itemId, out var descLabel))
            {
                var descMsg = this.GetMessage(msgPath, descLabel, *msgFlags);
                if (descMsg != null)
                {
                    return descMsg;
                }
            }

            return this.getItemDescHook!.OriginalFunction(itemId, msgFlags);
        }

        if (type == ItemMsg.Effect)
        {
            if (this.itemEffects.TryGetValue(*itemId, out var effectLabel))
            {
                var effectMsg = this.GetMessage(msgPath, effectLabel, *msgFlags);
                if (effectMsg != null)
                {
                    return effectMsg;
                }
            }

            return this.getItemEffectHook!.OriginalFunction(itemId, msgFlags);
        }

        return this.CreateMsg("Unknown Item MSG");
    }

    public MSG* CreateMsg(string str)
    {
        var strPtr = Marshal.StringToHGlobalAnsi(str);
        var msg = this.createMsgFromString!(strPtr, MsgFlag.Flag2, this.msgConfig1, this.msgConfig2);
        Marshal.FreeHGlobal(strPtr);
        return msg;
    }

    public MSG* CreateMsg(string str, MsgFlag flags, MsgConfig1 config1, MsgConfig2 config2)
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

    public void EditMsg(string msgPath, IEnumerable<Message> messages)
    {
        var lang = MsgUtils.GetMsgLanguage(msgPath);
        _registry.RegisterMsg(lang, new Msg(msgPath, messages.Select(x => new SingleMessage(x)).ToArray()));
    }

    public ILangItemMessages CreateItemMessages()
    {
        var itemMessages = new LangItemMessages();
        foreach (var lang in Enum.GetValues<Language>())
        {
            if (lang == Language.Any) continue;

            var (name, desc, effect) = itemMessages.GetLanguage(lang).GetMessages();
            var itemMsg = new Msg(MsgUtils.GetItemMsgPath(lang, ItemMsg.Name), new SingleMessage(name));
            var descMsg = new Msg(MsgUtils.GetItemMsgPath(lang, ItemMsg.Description), new SingleMessage(desc));
            var effectMsg = new Msg(MsgUtils.GetItemMsgPath(lang, ItemMsg.Effect), new SingleMessage(effect));

            _registry.RegisterMsg(lang, itemMsg);
            _registry.RegisterMsg(lang, descMsg);
            _registry.RegisterMsg(lang, effectMsg);
        }

        return itemMessages;
    }
}


[StructLayout(LayoutKind.Sequential)]
public unsafe struct MsgConfigs // Probably, maybe accruate...
{
    public MsgConfig2 Config2;
    public MsgConfig1 Config1;
}