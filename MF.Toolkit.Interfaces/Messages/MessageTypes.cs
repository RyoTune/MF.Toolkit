using System.Runtime.InteropServices;
using System.Text;

namespace MF.Toolkit.Interfaces.Messages;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Flags]
public enum MsgFlag : uint
{
    // Descriptions use Flags 1, 2, 3.
    // Names use Flag 4.
    None = 0,
    Flag1 = 1, // Bold / Resize?
    Flag2 = 1 << 1, // Normal (at least in inv) / Resize
    Flag3 = 1 << 2, // Normal + big, same as None?
    Flag4 = 1 << 3, // Used by names.
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public unsafe struct ItemMsgSerial
{
    private fixed byte magic[4];
    public int NameSerialId;
    public int DescSerialId;
    public int EffectSerialId;

    public readonly bool IsCustomSerial() =>
        magic[0] == 'M' &&
        magic[1] == 'F' &&
        magic[2] == 'T' &&
        magic[3] == 'K';

    public void UseCustomSerial()
    {
        var toolkitMagic = Encoding.ASCII.GetBytes("MFTK");
        magic[0] = toolkitMagic[0];
        magic[1] = toolkitMagic[1];
        magic[2] = toolkitMagic[2];
        magic[3] = toolkitMagic[3];
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MsgConfig1
{
    public byte field1; // Style flags? 1 = bold
    public byte field2; // unused
    public byte field3; // unused
    public byte field4; // unused
    public int field5; // passed as arg but never used???
    public bool field6; // whether to use 
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MsgConfig2
{
    public nint field1; // probably a pointer
    public bool field2; // whether to use pointer
}

public unsafe struct MSG
{
}

public enum ItemMsg
{
    Name,
    Description,
    Effect,
}
