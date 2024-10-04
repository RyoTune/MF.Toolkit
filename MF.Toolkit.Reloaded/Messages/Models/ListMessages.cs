using MF.Toolkit.Interfaces.Messages.Models;

namespace MF.Toolkit.Reloaded.Messages.Models;

internal class ListMessages(IEnumerable<Message> messages) : IMessagesProvider
{
    public MessageDictionary Messages => new(messages);
}
