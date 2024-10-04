using MF.Toolkit.Interfaces.Messages.Models;
using System.Runtime.InteropServices;
using System.Text;

namespace MF.Toolkit.Reloaded.Messages.Models;

public class MessageDictionary : Dictionary<string, Message>
{
    public MessageDictionary()
    {
    }

    public MessageDictionary(IEnumerable<Message> messages)
        : base(messages.ToDictionary(x => x.Identifier, x => x))
    {
    }

    public void Merge(MessageDictionary mergingDict)
    {
        foreach (var item in mergingDict)
        {
            this[item.Key] = item.Value;
        }
    }

    public (nint Pointer, int Length) ToMemory()
    {
        var str = string.Join('\n', this.Values.Select(x => x.ToString()));
        return (Marshal.StringToHGlobalAnsi(str), str.Length);
    }
}
