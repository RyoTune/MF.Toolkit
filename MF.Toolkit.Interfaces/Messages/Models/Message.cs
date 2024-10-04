namespace MF.Toolkit.Interfaces.Messages.Models;

/// <summary>
/// Parsed MSG.
/// </summary>
public class Message
{
    /// <summary>
    /// Identifier.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Speaker ID.
    /// </summary>
    public string? Speaker { get; set; }

    /// <summary>
    /// MSG content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}