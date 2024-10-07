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
    /// <param name="msgPath">MSG file to edit.</param>
    /// <param name="messages">New messages.</param>
    void EditMsg(string msgPath, IEnumerable<Message> messages);

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
    /// Creates a set of item messages for setting name, description, and effect
    /// with per language settings, if needed.
    /// </summary>
    /// <returns>Message label to use for referencing the messages.</returns>
    ILangItemMessages CreateItemMessages();

    /// <summary>
    /// Set the message label to use for an item's message.
    /// </summary>
    /// <param name="itemId">Item ID of item to set.</param>
    /// <param name="type">Type of message.</param>
    /// <param name="label">Message label to use.</param>
    void SetItemMessage(int itemId, ItemMsg type, string label);
}
