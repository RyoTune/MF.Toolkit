using MF.Toolkit.Interfaces.Messages.Models;
using System.Runtime.InteropServices;
using System.Text;

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
        var bytes = Encoding.UTF8.GetBytes(str);
        var ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return (ptr, bytes.Length);
    }
}
