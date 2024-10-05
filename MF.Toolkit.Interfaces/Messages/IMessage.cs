using MF.Toolkit.Interfaces.Messages.Models;

namespace MF.Toolkit.Interfaces.Messages;

/// <summary>
/// MSG related functions and utils.
/// </summary>
public unsafe interface IMessage
{
    /// <summary>
    /// Add to or edit an existing MSG file.
    /// </summary>
    /// <param name="msgFilePath">MSG file to edit.</param>
    /// <param name="msgs">New MSGs.</param>
    void EditMsgFile(string msgFilePath, IEnumerable<Message> msgs);

    /// <summary>
    /// Create a MSG from a string (plaintext).
    /// </summary>
    /// <param name="str">String.</param>
    /// <returns>MSG object pointer.</returns>
    MSG* CreateMsg(string str);

    /// <summary>
    /// Create a MSG from a string (plaintext) with specific settings.
    /// </summary>
    /// <param name="str">String.</param>
    /// <param name="flags">MSG flags.</param>
    /// <param name="config1">MSG config 1.</param>
    /// <param name="config2">MSG config 2.</param>
    /// <returns>MSG object pointer.</returns>
    MSG* CreateMsg(string str, MsgFlag flags, MsgConfig1 config1, MsgConfig2 config2);

    /// <summary>
    /// Set the message content for an item's text.
    /// </summary>
    /// <param name="itemId">Item ID of item to set.</param>
    /// <param name="type">The type of text.</param>
    /// <param name="message">The message content.</param>
    void SetItemMessage(int itemId, ItemText type, string message);
}
