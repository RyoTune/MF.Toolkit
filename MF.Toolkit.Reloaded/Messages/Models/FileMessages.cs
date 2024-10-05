using MF.Toolkit.Reloaded.Messages.Parser;

namespace MF.Toolkit.Reloaded.Messages.Models;

internal class FileMessages(string file) : IMessagesProvider
{
    private readonly string file = file;
    private MessageList? _messages;
    private DateTime lastWrite = DateTime.Now;

    public MessageList Messages => this.GetMessages();

    public MessageList GetMessages()
    {
        if (_messages == null)
        {
            this.lastWrite = new FileInfo(this.file).LastWriteTime;
            _messages = MessageParser.Parse(File.ReadAllText(this.file));
        }

        if (this.HasFileChanged(out var newWrite))
        {
            this.lastWrite = newWrite;
            _messages = MessageParser.Parse(File.ReadAllText(this.file));
            Log.Debug($"Updated MSG: {this.file}");
        }

        return _messages;
    }

    private bool HasFileChanged(out DateTime newWrite)
    {
        var file = new FileInfo(this.file);
        newWrite = file.LastWriteTime;
        return newWrite != this.lastWrite;
    }
}
