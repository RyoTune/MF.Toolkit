using MF.Toolkit.Interfaces.Common;

namespace MF.Toolkit.Interfaces.Messages.Models;

public class LangItemMessages : Dictionary<Language, ItemMessages>
{
    public LangItemMessages()
    {
        foreach (var value in Enum.GetValues<Language>())
        {
            this[value] = new(this.Label);
        }
    }

    public string Label { get; } = Guid.NewGuid().ToString();

    public ItemMessages EN => this[Language.EN];
    public ItemMessages DE => this[Language.DE];
    public ItemMessages ES => this[Language.ES];
    public ItemMessages FR => this[Language.FR];
    public ItemMessages IT => this[Language.IT];
    public ItemMessages JP => this[Language.JP];
    public ItemMessages KO => this[Language.KO];
    public ItemMessages PT_BR => this[Language.PT_BR];
    public ItemMessages RU => this[Language.RU];
    public ItemMessages ZH_CN => this[Language.ZH_CN];
    public ItemMessages ZH_TW => this[Language.ZH_TW];
}

public class ItemMessages
{
    private readonly Message name = new();
    private readonly Message desc = new();
    private readonly Message effect = new();

    public ItemMessages(string label)
    {
        this.name.Label = label;
        this.desc.Label = label;
        this.effect.Label = label;
    }

    public void SetName(string name) => this.name.Content = name;

    public void SetDescription(string description) => this.desc.Content = description;

    public void SetEffect(string effect) => this.effect.Content = effect;

    public (Message Name, Message Description, Message Effect) GetMessages() => (this.name, this.desc, this.effect);
}
