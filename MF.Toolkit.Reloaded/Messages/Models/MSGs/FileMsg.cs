using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Messages.Models.ItemMessages;
using MF.Toolkit.Reloaded.Messages.Models.MessageProviders;
using MF.Toolkit.Reloaded.Messages.Parser;

namespace MF.Toolkit.Reloaded.Messages.Models.MSGs;

internal class FileMsg : IMsg
{
    private readonly string _file;
    private readonly MessageMap _messages = [];

    public FileMsg(string msgPath, string file)
    {
        MsgPath = msgPath;

        _file = file;
        FileWatcher.CreateWatcher(file, SyncWithFile);

        SyncWithFile();
    }

    public string MsgPath { get; }

    public IMessageProvider[] Messages => _messages.Values.Select(x => new MessageFromMap(x.Label, _messages)).ToArray();

    private void SyncWithFile()
    {
        var fileMessages = MessageParser.Parse(File.ReadAllText(_file));

        var initialLoad = _messages.Count < 1;
        foreach (var message in fileMessages)
        {
            if (initialLoad)
            {
                _messages[message.Label] = message;
            }
            else
            {
                _messages[message.Label].Speaker = message.Speaker;
                _messages[message.Label].Content = message.Content;
            }
        }

        Log.Debug($"Synced \"{MsgPath}\" with file.\nFile: {_file}");
    }
}
