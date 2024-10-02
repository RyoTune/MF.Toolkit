namespace MF.Toolkit.Interfaces.Messages;

/// <summary>
/// MSG related functions and utils.
/// </summary>
public unsafe interface IMessage
{
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
    MSG* CreateMsg(string str, MsgFlags flags, MsgConfig1 config1, MsgConfig2 config2);

    /// <summary>
    /// Create a custom MSG serial.
    /// </summary>
    /// <param name="msg">Serial MSG value.</param>
    /// <returns>Custom Serial ID.</returns>
    int CreateMsgSerial(MSG* msg);

    /// <summary>
    /// Create a custom MSG serial.
    /// </summary>
    /// <param name="str">Serial string value.</param>
    /// <returns>Custom Serial ID.</returns>
    int CreateMsgSerial(string str);
}
