using MF.Toolkit.Reloaded.Messages.Models.ItemMessages;
using MF.Toolkit.Reloaded.Messages.Models.MessageProviders;
using MF.Toolkit.Reloaded.Messages.Parser;

namespace MF.Toolkit.Reloaded.Messages.Models.MSGs;

internal class FileMsg(string msgPath, string file) : IMsg
{
    private readonly string _file = file;
    private DateTime _lastWrite;
    private readonly MessageMap _messages = [];

    public string MsgPath { get; } = msgPath;

    public IMessageProvider[] Messages
    {
        get
        {
            SyncWithFile();
            return _messages.Values.Select(x => new MessageFromMap(x.Label, _messages)).ToArray();
        }
    }

    private void SyncWithFile()
    {
        var file = new FileInfo(_file);
        if (_messages.Count < 1)
        {
            LoadMessages();
            _lastWrite = file.LastWriteTime;
            Log.Information($"Updated MSG from File || MSG: {MsgPath}\nFile: {_file}");
        }

        if (_lastWrite != file.LastWriteTime)
        {
            LoadMessages();
            _lastWrite = file.LastWriteTime;
            Log.Information($"Updated MSG from File || MSG: {MsgPath}\nFile: {_file}");
        }
    }

    private void LoadMessages()
    {
        _messages.Clear();

        var fileMessages = MessageParser.Parse(File.ReadAllText(_file));
        foreach (var message in fileMessages)
        {
            _messages[message.Label] = message;
        }
    }
}
