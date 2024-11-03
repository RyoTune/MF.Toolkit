using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Interfaces.Messages;

namespace MF.Toolkit.Reloaded.Messages;

internal static class MsgUtils
{
    public static string GetItemMsgPath(Language lang, ItemMsg type)
    {
        var msgPath = type switch
        {
            ItemMsg.Name => "message/system/Equip_Message_1.msg",
            ItemMsg.Description => "message/system/Equip_Message_2.msg",
            ItemMsg.Effect => "message/system/Equip_Message_3.msg",
            _ => throw new Exception()
        };

        if (lang == Language.Any)
        {
            return msgPath;
        }

        return $"{lang.ToCode()}/{msgPath}";
    }

    public static string ToLangAgnostic(string msgPath)
    {
        foreach (var lang in Enum.GetValues<Language>())
        {
            var langCode = lang.ToCode();
            if (msgPath.StartsWith(langCode))
            {
                msgPath = msgPath.Substring(langCode.Length + 1);
            }
        }

        msgPath = msgPath.TrimStart('\\', '/');
        return msgPath;
    }

    public static Language GetMsgLanguage(string msgPath)
        => Enum.GetValues<Language>().FirstOrDefault(x => msgPath.StartsWith(x.ToCode(), StringComparison.OrdinalIgnoreCase));
}
