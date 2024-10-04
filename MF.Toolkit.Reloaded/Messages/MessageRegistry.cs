using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Messages.Models;
using MF.Toolkit.Reloaded.Messages.Parser;
using System.Diagnostics.CodeAnalysis;

namespace MF.Toolkit.Reloaded.Messages;

internal class MessageRegistry : IRegisterMod
{
    private readonly Dictionary<string, List<string>> modMsgFiles = new(StringComparer.OrdinalIgnoreCase);

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
        if (this.modMsgFiles.TryGetValue(msgPath, out var modFiles))
        {
            modDict = [];
            foreach (var file in modFiles)
            {
                var fileMsgs = MessageParser.Parse(File.ReadAllText(file));
                modDict.Merge(fileMsgs);
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
            var msgTargetPath = Path.GetRelativePath(folder, file).Replace("\\", "/");
            if (this.modMsgFiles.TryGetValue(msgTargetPath, out var existing))
            {
                existing.Add(file);
            }
            else
            {
                this.modMsgFiles[msgTargetPath] = [file];
            }

            Log.Information($"Registered MSG: {msgTargetPath}\nFile: {file}");
        }
    }
}
