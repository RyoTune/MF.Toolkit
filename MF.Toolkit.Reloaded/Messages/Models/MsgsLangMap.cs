using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Reloaded.Messages.Models.MessageLists;
using MF.Toolkit.Reloaded.Messages.Models.MessageProviders;
using MF.Toolkit.Reloaded.Messages.Models.MSGs;

namespace MF.Toolkit.Reloaded.Messages.Models;

internal class MsgsLangMap : Dictionary<string, LangMessageMap>
{
    public bool TryGetMessages(string msgPath, Language secondLang, out MessageList? messages)
    {
        messages = null;

        var agnosticMsgPath = MsgUtils.ToLangAgnostic(msgPath).Replace('\\', '/');
        if (TryGetValue(agnosticMsgPath, out var langMessageMap))
        {
            messages = [];

            var msgLang = MsgUtils.GetMsgLanguage(msgPath);

            // Create a flatten list of messages that matches as best as possible
            // to the language preferrences.
            foreach (var label in langMessageMap.GetLabels())
            {
                if (msgLang == Language.Any) continue;

                // Label has language provider.
                if (langMessageMap.TryGetLabelMessage(msgLang, label, out var langLabelProvider))
                {
                    messages.Add(langLabelProvider.Message);
                }

                // Label has language provider for preferred fallback.
                else if (langMessageMap.TryGetLabelMessage(secondLang, label, out var prefLabelProvider))
                {
                    messages.Add(prefLabelProvider.Message);
                }

                // Prefer using EN has fallback.
                else if (langMessageMap.TryGetLabelMessage(Language.EN, label, out var enLabelProvider))
                {
                    messages.Add(enLabelProvider.Message);
                }

                // Use any provider for label.
                else
                {
                    var anyLabelProvider = langMessageMap.GetAnyLabelMessageProvider(label);
                    messages.Add(anyLabelProvider.Message);
                }
            }
        }

        return messages != null;
    }

    public void RegisterMsg(Language language, IMsg msg)
    {
        foreach (var message in msg.Messages)
        {
            AddMsgMessage(language, msg.MsgPath, message);
        }
    }

    public void AddMsgMessage(Language language, string msgPath, IMessageProvider message)
    {
        msgPath = MsgUtils.ToLangAgnostic(msgPath).Replace('\\', '/');
        if (TryGetValue(msgPath, out var langMap))
        {
            langMap.AddMessage(language, message);
        }
        else
        {
            this[msgPath] = new LangMessageMap();
            this[msgPath].AddMessage(language, message);
        }

        Log.Debug($"Added messsage \"{message.Message.Label}\" to \"{msgPath}\".");
    }
}
