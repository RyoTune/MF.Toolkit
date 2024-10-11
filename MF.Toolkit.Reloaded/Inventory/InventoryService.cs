using MF.Toolkit.Interfaces.Inventory;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using Reloaded.Hooks.Definitions;

namespace MF.Toolkit.Reloaded.Inventory;

internal class InventoryService : IUseConfig, IInventory
{
    private delegate byte GetItemCount(ushort itemId);
    private readonly Dictionary<ItemType, IHook<GetItemCount>> hooks = [];
    private bool unlockAllItems;

    private HashSet<int> _unlockedItems = [];

    public InventoryService()
    {
        ScanHooks.Add(
            $"{nameof(GetItemCount)}: Armor",
            "40 53 48 83 EC 20 0F B7 D9 E8 ?? ?? ?? ?? 0F B6 84 ?? ?? 04 00 00 48 83 C4 20 5B C3",
            (hooks, result) => this.hooks[ItemType.Armor] = hooks.CreateHook<GetItemCount>((id) => GetItemCountImpl(ItemType.Armor, id), result).Activate());

        ScanHooks.Add(
            $"{nameof(GetItemCount)}: Gear",
            "40 53 48 83 EC 20 0F B7 D9 E8 ?? ?? ?? ?? 0F B6 04",
            (hooks, result) => this.hooks[ItemType.Gear] = hooks.CreateHook<GetItemCount>((id) => GetItemCountImpl(ItemType.Gear, id), result).Activate());

        ScanHooks.Add(
            $"{nameof(GetItemCount)}: Weapons",
            "40 53 48 83 EC 20 0F B7 D9 E8 ?? ?? ?? ?? 0F B6 84 ?? ?? 08 00 00 48 83 C4 20 5B C3",
            (hooks, result) => this.hooks[ItemType.Weapons] = hooks.CreateHook<GetItemCount>((id) => GetItemCountImpl(ItemType.Weapons, id), result).Activate());

        ScanHooks.Add(
            $"{nameof(GetItemCount)}: Accessories",
            "40 53 48 83 EC 20 0F B7 D9 E8 ?? ?? ?? ?? 0F B6 84 ?? ?? 0C 00 00 48 83 C4 20 5B C3",
            (hooks, result) => this.hooks[ItemType.Accessories] = hooks.CreateHook<GetItemCount>((id) => GetItemCountImpl(ItemType.Accessories, id), result).Activate());

        ScanHooks.Add(
            $"{nameof(GetItemCount)}: Use / Exchange",
            "40 53 48 83 EC 20 0F B7 D9 E8 ?? ?? ?? ?? 0F B6 84 ?? ?? 0E 00 00 48 83 C4 20 5B C3",
            (hooks, result) => this.hooks[ItemType.UseExchange] = hooks.CreateHook<GetItemCount>((id) => GetItemCountImpl(ItemType.UseExchange, id), result).Activate());

        ScanHooks.Add(
            $"{nameof(GetItemCount)}: Information / Key Items",
            "40 53 48 83 EC 20 0F B7 D9 E8 ?? ?? ?? ?? 0F B6 84 ?? ?? 10 00 00 48 83 C4 20 5B C3",
            (hooks, result) => this.hooks[ItemType.InfoKey] = hooks.CreateHook<GetItemCount>((id) => GetItemCountImpl(ItemType.InfoKey, id), result).Activate());
#if DEBUG
        ScanHooks.Add(
            $"{nameof(GetItemCount)}: Outfits",
            "40 53 48 83 EC 20 0F B7 D9 E8 ?? ?? ?? ?? 0F B6 84 ?? ?? 14 00 00 48 83 C4 20 5B C3",
            (hooks, result) => this.hooks[ItemType.Outfits] = hooks.CreateHook<GetItemCount>((id) => GetItemCountImpl(ItemType.Outfits, id), result).Activate());
#endif
    }

    private byte GetItemCountImpl(ItemType type, ushort itemId)
    {
        Log.Verbose($"{nameof(GetItemCount)} ({type}): {itemId}");
        var genItemid = GetGenericItemId(type, itemId);
        if (_unlockedItems.Contains(genItemid))
        {
            Log.Debug($"Item Unlocked || {genItemid}");
            return 1;
        }

        return unlockAllItems ? (byte)1 : hooks[type].OriginalFunction(itemId);
    }

    public void UnlockItem(int itemId)
    {
        Log.Debug($"Registered Item Unlock: {itemId}");
        _unlockedItems.Add(itemId);
    }

    public void ConfigChanged(Config config) => this.unlockAllItems = config.UnlockAllItems;

    private static int GetGenericItemId(ItemType type, int itemId) => (int)type * 0x1000 + itemId;

    private enum ItemType
    {
        Armor,
        Gear,
        Weapons,
        Accessories,
        UseExchange,
        InfoKey,
        Outfits,
    }
}
