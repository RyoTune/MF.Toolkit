using MF.Toolkit.Reloaded.Messages.Models.MessageProviders;
using MF.Toolkit.Reloaded.Messages.Models.MSGs;

namespace MF.Toolkit.Reloaded.Messages.Models;

internal class Msg : IMsg
{
    public Msg(string msgPath, IMessageProvider[] messages)
    {
        MsgPath = msgPath;
        Messages = messages;
    }

    public Msg(string msgPath, IMessageProvider message)
    {
        MsgPath = msgPath;
        Messages = [message];
    }

    public string MsgPath { get; }

    public IMessageProvider[] Messages { get; }
}
