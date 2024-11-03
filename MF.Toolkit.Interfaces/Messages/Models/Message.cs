using System.Text;

namespace MF.Toolkit.Interfaces.Messages.Models;

/// <summary>
/// Parsed MSG item.
/// </summary>
public class Message
{
    private string _content = string.Empty;

    /// <summary>
    /// Identifier.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Speaker ID.
    /// </summary>
    public string? Speaker { get; set; }

    /// <summary>
    /// Message content.
    /// </summary>
    public string Content
    {
        get => FormatContent(_content);
        set => _content = value;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();

        if (this.Speaker != null)
        {
            sb.AppendLine($"#{this.Speaker}");
        }

        sb.AppendLine($"@{this.Label}");
        sb.AppendLine($"{{\n{this.Content}\n}}");

        return sb.ToString();
    }

    private static string FormatContent(string content)
    {
        // Fix empty message contents.
        if (content == "<WAIT>" || content == string.Empty)
        {
            content = " \n<WAIT>";
        }

        if (!content.Contains("<WAIT>"))
        {
            return $"{content}\n<WAIT>";
        }

        return content;
    }
}