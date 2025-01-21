using MF.Toolkit.Reloaded.Messages.Parser;
using System.Runtime.InteropServices;

namespace MF.Toolkit.Reloaded.Messages.Models.MessageLists;

internal class GameMessageList : MessageList
{
    public GameMessageList(nint src, int srcLength)
    {
        Merge(MessageParser.Parse(Marshal.PtrToStringUTF8(src, srcLength)!));
    }

    public int GetLabelId(string label) => FindIndex(x => x.Label == label);
}
