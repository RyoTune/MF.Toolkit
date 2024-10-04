using MF.Toolkit.Interfaces.Messages.Models;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Messages.Models;
using System.Diagnostics.CodeAnalysis;

namespace MF.Toolkit.Reloaded.Messages;

internal class MessageRegistry : IRegisterMod
{
    private readonly Dictionary<string, List<IMessagesProvider>> msgProviders = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterMod(string modId, string modDir)
    {
        var msgsDir = Path.Join(modDir, "Metaphor", "MSG");
        if (Directory.Exists(msgsDir))
        {
            this.RegisterFolder(msgsDir);
        }
    }

    public bool TryGetModDict(string msgPath, [NotNullWhen(true)] out MessageDictionary? modDict)
    {
        if (this.msgProviders.TryGetValue(msgPath, out var providers))
        {
            modDict = [];
            foreach (var provider in providers)
            {
                modDict.Merge(provider.Messages);
            }

            return true;
        }

        modDict = null;
        return false;
    }

    public void RegisterFolder(string folder)
    {
        foreach (var file in Directory.EnumerateFiles(folder, "*.msg", SearchOption.AllDirectories))
        {
            var msgFilePath = Path.GetRelativePath(folder, file).Replace("\\", "/");
            var msgProvider = new FileMessages(file);
            this.RegisterProvider(msgFilePath, msgProvider);

            Log.Information($"Registered MSG: {msgFilePath}\nFile: {file}");
        }
    }

    public void RegisterMessages(string msgFilePath, IEnumerable<Message> messages) => this.RegisterProvider(msgFilePath, new ListMessages(messages));

    private void RegisterProvider(string msgFilePath, IMessagesProvider provider)
    {
        if (this.msgProviders.TryGetValue(msgFilePath, out var existing))
        {
            existing.Add(provider);
        }
        else
        {
            this.msgProviders[msgFilePath] = [provider];
        }
    }
}
