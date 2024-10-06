namespace MF.Toolkit.Interfaces.Common;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public enum Language
{
    Any,
    EN,
    DE,
    ES,
    FR,
    IT,
    JP,
    KO,
    PT_BR,
    RU,
    ZH_CN,
    ZH_TW,
}

public static class LanguageExtensions
{
    public static string ToCode(this Language language) => language.ToString().Replace('_', '-');
}
