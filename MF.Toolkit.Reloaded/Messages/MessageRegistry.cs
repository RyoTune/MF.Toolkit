using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Interfaces.Messages.Models;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Messages.Models;
using System.Diagnostics.CodeAnalysis;

namespace MF.Toolkit.Reloaded.Messages;

internal class MessageRegistry : IRegisterMod, IUseConfig
{
    private readonly Dictionary<string, List<IMessagesProvider>> msgProviders = new(StringComparer.OrdinalIgnoreCase);
    private Language langPref;

    public void RegisterMod(string modId, string modDir)
    {
        var msgsDir = Path.Join(modDir, "Metaphor", "MSG");
        if (Directory.Exists(msgsDir))
        {
            this.RegisterFolder(msgsDir);
        }
    }

    public bool TryGetModMessages(string msgPath, [NotNullWhen(true)] out MessageList? modDict)
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
        var filesRegistered = new HashSet<string>();
        foreach (var file in Directory.EnumerateFiles(folder, "*.msg", SearchOption.AllDirectories))
        {
            if (!filesRegistered.Contains(file))
            {
                this.RegisterWithLanguages(folder, file, filesRegistered);
            }
        }
    }

    public void RegisterMessage(string msgPath, Message msg) => this.RegisterMessages(msgPath, [msg]);

    public void RegisterMessages(string msgPath, IEnumerable<Message> messages) => this.RegisterProvider(msgPath, new ListMessages(messages));

    private void RegisterProvider(string msgPath, IMessagesProvider provider)
    {
        msgPath = msgPath.Replace('\\', '/');
        if (this.msgProviders.TryGetValue(msgPath, out var existing))
        {
            existing.Add(provider);
        }
        else
        {
            this.msgProviders[msgPath] = [provider];
        }
    }

    private void RegisterWithLanguages(string baseDir, string inputFile, HashSet<string> filesRegistered)
    {
        var inputMsgPath = Path.GetRelativePath(baseDir, inputFile);
        var inputLang = Enum.GetValues<Language>().FirstOrDefault(x => inputMsgPath.StartsWith(x.ToCode(), StringComparison.OrdinalIgnoreCase));
        if (inputLang == Language.None)
        {
            var msgProvider = new FileMessages(inputFile);
            this.RegisterProvider(inputMsgPath, msgProvider);
            filesRegistered.Add(inputFile);
            Log.Information($"Registered MSG: {inputMsgPath}\nFile: {inputFile}");
            return;
        }

        var baseMsgPath = inputMsgPath.Substring(inputMsgPath.IndexOf(inputLang.ToCode()) + inputLang.ToCode().Length);
        var langFiles = new Dictionary<Language, string>();

        // Find all available language files first.
        foreach (var lang in Enum.GetValues<Language>())
        {
            if (lang == Language.None) continue;

            var langFile = Path.Join(baseDir, lang.ToCode(), baseMsgPath);
            if (File.Exists(langFile))
            {
                langFiles[lang] = langFile;
                filesRegistered.Add(langFile);
            }
        }

        foreach (var lang in Enum.GetValues<Language>())
        {
            if (lang == Language.None) continue;

            var langMsgPath = Path.Join(lang.ToCode(), baseMsgPath);

            // Mod has file for lang.
            if (langFiles.TryGetValue(lang, out var file))
            {
                var msgProvider = new FileMessages(file);
                this.RegisterProvider(langMsgPath, msgProvider);
                Log.Information($"Registered MSG ({lang}): {langMsgPath}\nFile: {file}");
            }

            // Mod does not have file for lang, use preferred lang.
            else if (langFiles.TryGetValue(this.langPref, out var prefFile))
            {
                var msgProvider = new FileMessages(prefFile);
                this.RegisterProvider(langMsgPath, msgProvider);
                Log.Information($"Registered MSG (Preferred: {lang}->{this.langPref}): {langMsgPath}\nFile: {prefFile}");
            }

            // Mod does not have preferred language, use first available.
            else
            {
                (var fallbackLang, var fallbackFile) = langFiles.First();
                var msgProvider = new FileMessages(fallbackFile);
                this.RegisterProvider(langMsgPath, msgProvider);
                Log.Information($"Registered MSG (Fallback: {lang}->{fallbackLang}): {langMsgPath}\nFile: {fallbackFile}");
            }
        }
    }

    public void ConfigChanged(Config config) => this.langPref = config.LangPref;
}
