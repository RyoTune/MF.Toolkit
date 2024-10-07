using MF.Toolkit.Interfaces.Messages.Models;

namespace MF.Toolkit.Reloaded.Messages.Models.MessageProviders;

internal class SingleMessage(Message message) : IMessageProvider
{
    public Message Message { get; } = message;
}
