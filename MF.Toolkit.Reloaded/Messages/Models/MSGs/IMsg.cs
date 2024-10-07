using MF.Toolkit.Reloaded.Messages.Models.MessageProviders;

namespace MF.Toolkit.Reloaded.Messages.Models.MSGs;

internal interface IMsg
{
    string MsgPath { get; }

    IMessageProvider[] Messages { get; }
}