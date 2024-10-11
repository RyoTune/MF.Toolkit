namespace MF.Toolkit.Interfaces.Inventory;

/// <summary>
/// Inventory related functionality.
/// </summary>
public interface IInventory
{
    /// <summary>
    /// Permanently unlocks the item with the given ID.
    /// </summary>
    /// <param name="itemId">Item ID to unlock.</param>
    void UnlockItem(int itemId);
}