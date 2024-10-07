using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Reloaded.Messages.Models.MessageProviders;
using System.Diagnostics.CodeAnalysis;

namespace MF.Toolkit.Reloaded.Messages.Models;

internal class LangMessageMap
{
    private readonly LangLabelMessageMap _labelMessageProviders = [];

    public int Count => _labelMessageProviders.Count;

    public void AddMessage(Language language, IMessageProvider messageProvider)
    {
        var label = messageProvider.Message.Label;
        if (_labelMessageProviders.TryGetValue(label, out var provider))
        {
            provider[language] = messageProvider;
        }
        else
        {
            _labelMessageProviders[label] = [];
            _labelMessageProviders[label][language] = messageProvider;
        }
    }

    public string[] GetLabels() => _labelMessageProviders.Keys.ToArray();

    public bool TryGetLabelMessage(Language language, string label, [NotNullWhen(true)] out IMessageProvider? messageProvider)
    {
        var langProviders = _labelMessageProviders[label];
        langProviders.TryGetValue(language, out messageProvider);
        if (string.IsNullOrEmpty(messageProvider?.Message.Content))
        {
            messageProvider = null;
        }

        return messageProvider != null;
    }

    public IMessageProvider GetAnyLabelMessageProvider(string label)
        => _labelMessageProviders[label][_labelMessageProviders[label].Keys.First()];
}

/// <summary>
/// Label message provider per language.
/// </summary>
internal class LangLabelMessageMap : Dictionary<string, LangMessageProviderMap>
{
}

/// <summary>
/// Language specific message providers.
/// </summary>
internal class LangMessageProviderMap : Dictionary<Language, IMessageProvider>
{
}
