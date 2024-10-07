using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Interfaces.Messages.Models;

namespace MF.Toolkit.Reloaded.Messages.Models.ItemMessages;

internal class LangItemMessages : Dictionary<Language, ItemMessages>, ILangItemMessages
{
    public LangItemMessages()
    {
        foreach (var lang in Enum.GetValues<Language>())
        {
            this[lang] = new ItemMessages(Label);
        }
    }

    IItemMessages ILangItemMessages.this[Language language] => this[language];
    public string Label { get; } = Guid.NewGuid().ToString();
    public IItemMessages EN => this[Language.EN];
    public IItemMessages DE => this[Language.DE];
    public IItemMessages ES => this[Language.ES];
    public IItemMessages FR => this[Language.FR];
    public IItemMessages IT => this[Language.IT];
    public IItemMessages JP => this[Language.JP];
    public IItemMessages KO => this[Language.KO];
    public IItemMessages PT_BR => this[Language.PT_BR];
    public IItemMessages RU => this[Language.RU];
    public IItemMessages ZH_CN => this[Language.ZH_CN];
    public IItemMessages ZH_TW => this[Language.ZH_TW];

    public ItemMessages GetLanguage(Language language) => this[language];
}
