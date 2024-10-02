using MF.Toolkit.Interfaces.Messages;
using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;

namespace MF.Toolkit.Reloaded.Messages;

internal unsafe class MessageService : IMessage
{
    private delegate int GetItemNameSerialId(ItemMsgSerial* serial);
    private IHook<GetItemNameSerialId>? getItemNameSerialIdHook;

    private delegate nint GetMsgBySerialId(int* param1, uint* param2, nint param3);
    private IHook<GetMsgBySerialId>? getMsgBySerialHook;

    private delegate MSG* CreateMsgFromString(nint str, MsgFlags flags, MsgConfig1* config1, MsgConfig2* config2);
    private CreateMsgFromString? createMsgFromString;

    private readonly MsgConfig1* msgConfig1;
    private readonly MsgConfig2* msgConfig2;
    private readonly Dictionary<int, nint> customSerialMsgs = [];
    private int nextCustomMsg = -1;

    public MessageService()
    {
        this.msgConfig1 = (MsgConfig1*)Marshal.AllocHGlobal(sizeof(MsgConfig1));
        this.msgConfig2 = (MsgConfig2*)Marshal.AllocHGlobal(sizeof(MsgConfig1));
        *this.msgConfig1 = new();
        *this.msgConfig2 = new();

        ScanHooks.Add(
            nameof(GetItemNameSerialId),
            "48 83 EC 38 4C 8B 05 ?? ?? ?? ?? 4D 85 C0 0F 84 ?? ?? ?? ?? 0F B6 01 48 8D 54 24 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 88 44 24 ?? 0F B6 41 ?? 49 8B C8 88 44 24",
            (hooks, result) => this.getItemNameSerialIdHook = hooks.CreateHook<GetItemNameSerialId>(this.GetItemNameSerialIdImpl, result).Activate());

        ScanHooks.Add(
            nameof(GetMsgBySerialId),
            "48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 41 56 41 57 48 8B EC 48 81 EC 80 00 00 00 49 8B F0",
            (hooks, result) => this.getMsgBySerialHook = hooks.CreateHook<GetMsgBySerialId>(this.GetMsgBySerialIdImpl, result).Activate());

        ScanHooks.Add(
            nameof(CreateMsgFromString),
            "40 53 48 83 EC 30 41 80 79 ?? 00",
            (hooks, result) => this.createMsgFromString = hooks.CreateWrapper<CreateMsgFromString>(result, out _));
    }

    private int GetItemNameSerialIdImpl(ItemMsgSerial* serial)
    {
        if (serial->IsCustomSerial())
        {
            Log.Debug($"Using Name Serial ID: {serial->NameSerialId}");
            return this.nextCustomMsg = serial->NameSerialId;
        }
        else
        {
            var result = this.getItemNameSerialIdHook!.OriginalFunction(serial);
            return result;
        }
    }

    private nint GetMsgBySerialIdImpl(int* serialId, uint* param2, nint param3)
    {
        if (*serialId == -1)
        {
            this.customSerialMsgs.TryGetValue(this.nextCustomMsg, out var newMsg);
            this.nextCustomMsg = -1;
            return newMsg;
        }
        else
        {
            return this.getMsgBySerialHook!.OriginalFunction(serialId, param2, param3);
        }
    }

    public int CreateMsgSerial(MSG* msg)
    {
        var nextId = this.customSerialMsgs.Count;
        this.customSerialMsgs[nextId] = (nint)msg;
        return nextId;
    }

    public int CreateMsgSerial(string str) => this.CreateMsgSerial(this.CreateMsg(str));

    public MSG* CreateMsg(string str)
    {
        var strPtr = Marshal.StringToHGlobalAnsi(str);
        var msg = this.createMsgFromString!(strPtr, MsgFlags.Flag1, this.msgConfig1, this.msgConfig2);
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
}
