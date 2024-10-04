namespace MF.Toolkit.Reloaded.Messages.Models;

public class Message
{
    public string? Speaker { get; set; }

    public string Identifier { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}