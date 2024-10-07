using MF.Toolkit.Interfaces.Messages.Models;
using System.Runtime.InteropServices;

namespace MF.Toolkit.Reloaded.Messages.Models.MessageLists;

public class MessageList : List<Message>
{
    public MessageList()
    {
    }

    public MessageList(IEnumerable<Message> messages) : base(messages)
    {
    }

    public void Merge(MessageList newMessages)
    {
        foreach (var message in newMessages)
        {
            var existingIdx = FindIndex(x => x.Label == message.Label);
            if (existingIdx != -1)
            {
                this[existingIdx] = message;
            }
            else
            {
                Add(message);
            }
        }
    }

    public (nint Pointer, int Length) ToMemory()
    {
        var str = string.Join('\n', this.Select(x => x.ToString()));
        return (Marshal.StringToHGlobalAnsi(str), str.Length);
    }
}
