using MF.Toolkit.Reloaded.Messages.Parser;
using System.Runtime.InteropServices;

namespace MF.Toolkit.Reloaded.Messages.Models;

internal class GameMsg : MessageList
{
    public GameMsg(nint src, int srcLength)
    {
        this.Merge(MessageParser.Parse(Marshal.PtrToStringAnsi(src, srcLength)!));
    }

    public int GetLabelId(string label) => this.FindIndex(x => x.Label == label);
}
