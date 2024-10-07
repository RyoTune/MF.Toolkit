using MF.Toolkit.Interfaces.Messages.Models;

namespace MF.Toolkit.Reloaded.Messages.Models.MessageProviders;

internal interface IMessageProvider
{
    Message Message { get; }
}