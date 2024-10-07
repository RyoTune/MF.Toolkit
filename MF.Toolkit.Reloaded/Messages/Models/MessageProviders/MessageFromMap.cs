using MF.Toolkit.Interfaces.Messages.Models;
using MF.Toolkit.Reloaded.Messages.Models.ItemMessages;

namespace MF.Toolkit.Reloaded.Messages.Models.MessageProviders;

/// <summary>
/// Gets message indirectly from a message map by its label.
/// </summary>
internal class MessageFromMap(string label, MessageMap map) : IMessageProvider
{
    public Message Message => map[label];
}
