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

    public (nint pointer, int length) ToBinary()
    {
        var sb = new StringBuilder();
        foreach (var item in this)
        {
            var msg = item.Value;
            if (msg.Speaker != null)
            {
                sb.AppendLine($"#{msg.Speaker}");
            }

            sb.AppendLine($"@{msg.Identifier}");
            sb.AppendLine($"{{\n{msg.Content}\n}}");
        }

        var str = sb.ToString();
        return (Marshal.StringToHGlobalAnsi(str), str.Length);
    }
}
