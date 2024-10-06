using MF.Toolkit.Interfaces.Common;

namespace MF.Toolkit.Interfaces.Messages.Models;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface ILangItemMessages
{
    string Label { get; }
    IItemMessages DE { get; }
    IItemMessages EN { get; }
    IItemMessages ES { get; }
    IItemMessages FR { get; }
    IItemMessages IT { get; }
    IItemMessages JP { get; }
    IItemMessages KO { get; }
    IItemMessages PT_BR { get; }
    IItemMessages RU { get; }
    IItemMessages ZH_CN { get; }
    IItemMessages ZH_TW { get; }
    IItemMessages this[Language language] { get;}
}