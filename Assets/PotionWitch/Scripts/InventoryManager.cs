using System.Collections.Generic;
using UnityEngine;

// Stores how many of each InventoryItemData the player currently owns.
// Other gameplay scripts (e.g. PlantPot on harvest) call AddItem here.
//
// MVP scope:
//   - No save system.
//   - No UI binding.
//   - No categories or stack limits.
// Future systems (inventory UI, potion crafting) will read from this manager
// via HasItem / GetAmount and consume items via RemoveItem.
public class InventoryManager : MonoBehaviour
{
    // Internal storage. One InventoryStack per item type the player has owned.
    // Kept private so the only way in/out is through the public methods below.
    private readonly List<InventoryStack> stacks = new List<InventoryStack>();

    // Adds 'amount' of 'item' to the inventory. Creates a new stack if this
    // is the first one of that item, otherwise increases the existing stack.
    public void AddItem(InventoryItemData item, int amount = 1)
    {
        // TEMPORARY debug: warn if a caller forgot to pass an item asset.
        if (item == null)
        {
            Debug.LogWarning("[Inventory] Cannot add item. Item is null.");
            return;
        }
        if (amount <= 0) return;

        InventoryStack stack = FindStack(item);
        if (stack != null)
        {
            stack.amount += amount;
        }
        else
        {
            stacks.Add(new InventoryStack(item, amount));
        }

        // TEMPORARY debug logs — remove once an Inventory UI shows quantities.
        Debug.Log($"[Inventory] Added {amount} {item.itemName}");
        Debug.Log($"[Inventory] Total {item.itemName} = {GetAmount(item)}");
    }

    // Removes 'amount' of 'item'. If the stack drops to zero it is removed
    // entirely so GetAmount / HasItem stay clean. No-op if the item is not held.
    public void RemoveItem(InventoryItemData item, int amount = 1)
    {
        // TEMPORARY debug: warn if a caller forgot to pass an item asset.
        if (item == null)
        {
            Debug.LogWarning("[Inventory] Cannot remove item. Item is null.");
            return;
        }
        if (amount <= 0) return;

        InventoryStack stack = FindStack(item);
        if (stack == null) return;

        stack.amount -= amount;
        if (stack.amount <= 0) stacks.Remove(stack);

        // TEMPORARY debug logs — remove once an Inventory UI shows quantities.
        Debug.Log($"[Inventory] Removed {amount} {item.itemName}");
        Debug.Log($"[Inventory] Total {item.itemName} = {GetAmount(item)}");
    }

    // True if the player owns at least 'amount' of 'item'.
    public bool HasItem(InventoryItemData item, int amount = 1)
    {
        return GetAmount(item) >= amount;
    }

    // Current quantity of 'item' held. Zero if none.
    public int GetAmount(InventoryItemData item)
    {
        InventoryStack stack = FindStack(item);
        return stack != null ? stack.amount : 0;
    }

    // ---------- Helpers ----------

    // Linear lookup is fine for an MVP — the list will only ever hold a
    // handful of item types. Swap to a Dictionary if it ever grows large.
    private InventoryStack FindStack(InventoryItemData item)
    {
        for (int i = 0; i < stacks.Count; i++)
        {
            if (stacks[i].item == item) return stacks[i];
        }
        return null;
    }
}
