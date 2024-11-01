using MF.Toolkit.Interfaces.Metaphor.Models;

namespace MF.Toolkit.Interfaces.Metaphor;

/// <summary>
/// General toolkit API.
/// </summary>
public interface IMetaphorToolkit
{
    /// <summary>
    /// Add a Metaphor toolkit folder to load files from.
    /// </summary>
    /// <param name="modId">Mod ID.</param>
    /// <param name="folderPath">Folder to add.</param>
    void AddFolder(string modId, string folderPath);

    /// <summary>
    /// Set the value of a game BIT.
    /// Internally, this works by replacing the actual value when the game retrieves it.
    /// The actual data may or may not be edited depending on user setting, with no edit as default.
    /// </summary>
    /// <param name="bit">BIT to set.</param>
    /// <param name="value">Value to set BIT to.</param>
    void SetBit(int bit, bool value);

    /// <summary>
    /// Set the value of a game counter.
    /// Internally, this works by replacing the actual value when the game retrieves it.
    /// The actual data may or may not be edited depending on user setting, with no edit as default.
    /// </summary>
    /// <param name="counter">Counter to set.</param>
    /// <param name="value">Value to set counter to.</param>
    void SetCounter(int counter, int value);

    /// <summary>
    /// Redirect a file to a new path when it's loaded.
    /// </summary>
    /// <param name="ogPath">Original path to redirect.</param>
    /// <param name="newPath">New path to redirect to.</param>
    void RedirectFile(string ogPath, string newPath);

    /// <summary>
    /// Get list of user's owned DLC, once that data is available.
    /// </summary>
    /// <param name="ownedDlc">User's owned DLC.</param>
    void CheckOwnedDlc(Action<GameDlc[]> ownedDlc);
}
