using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Messages.Models;
using MF.Toolkit.Reloaded.Messages.Models.MessageLists;
using MF.Toolkit.Reloaded.Messages.Models.MSGs;
using System.Diagnostics.CodeAnalysis;

namespace MF.Toolkit.Reloaded.Messages;

internal class MessageRegistry : IRegisterMod, IUseConfig
{
    private readonly MsgsLangMap _msgs = [];
    private Language _secondLang;

    public void RegisterMod(string modId, string modDir)
    {
        var msgsDir = Path.Join(modDir, "Metaphor", "MSG");
        if (Directory.Exists(msgsDir))
        {
            this.RegisterFolder(msgsDir);
        }
    }

    public bool TryGetModMessages(string msgPath, [NotNullWhen(true)] out MessageList? messages)
    {
        if (_msgs.TryGetMessages(msgPath, _secondLang, out messages))
        {
            Log.Debug($"Using mod messages for MSG: {msgPath}");
        }
        else
        {
            Log.Debug($"No mod messages for MSG: {msgPath}");
        }

        return messages != null;
    }

    public void RegisterFolder(string folder)
    {
        foreach (var file in Directory.EnumerateFiles(folder, "*.msg", SearchOption.AllDirectories))
        {
            var msgPath = Path.GetRelativePath(folder, file);
            var msgLang = MsgUtils.GetMsgLanguage(msgPath);
            var fileMsg = new FileMsg(msgPath, file);
            _msgs.RegisterMsg(msgLang, fileMsg);
        }
    }

    public void RegisterMsg(Language language, IMsg msg) => _msgs.RegisterMsg(language, msg);

    public void ConfigChanged(Config config) => _secondLang = config.SecondLanguage;
}
