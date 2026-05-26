using System.Collections.Generic;
using UnityEngine;

// Stores how many of each InventoryItemData the player currently owns.
// Other gameplay scripts (e.g. PlantPot on harvest) call AddItem here.
//
// Singleton + DontDestroyOnLoad so the same inventory survives scene changes
// (e.g. Garden -> Cauldron). The first instance loaded becomes Instance; any
// duplicate that arrives later (because a second scene also has one in it)
// destroys itself so we never end up with two competing inventories.
//
// MVP scope:
//   - No save system (data is lost when the game exits).
//   - No UI binding (UI pulls via GetStacks()).
//   - No categories or stack limits.
public class InventoryManager : MonoBehaviour
{
    // Global access point. Other scripts can use InventoryManager.Instance
    // instead of holding a serialized reference. Null until the first
    // InventoryManager wakes up.
    public static InventoryManager Instance { get; private set; }

    // Internal storage. One InventoryStack per item type the player has owned.
    // Kept private so the only way in/out is through the public methods below.
    private readonly List<InventoryStack> stacks = new List<InventoryStack>();

    // Runs before Start on every InventoryManager that loads. Decides
    // whether this instance is the keeper or a duplicate to discard.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A keeper already exists from a previous scene — kill this copy
            // before it touches the static Instance or runs any other code.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Clear the static reference if this instance is the keeper and is being
    // destroyed (e.g. application quit). Prevents a stale Instance pointer
    // if the game ever recreates the manager later.
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Adds 'amount' of 'item' to the inventory. Creates a new stack if this
    // is the first one of that item, otherwise increases the existing stack.
    public void AddItem(InventoryItemData item, int amount = 1)
    {
        // Guard against callers forgetting to pass an item asset.
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
    }

    // Removes 'amount' of 'item'. If the stack drops to zero it is removed
    // entirely so GetAmount / HasItem stay clean. No-op if the item is not held.
    public void RemoveItem(InventoryItemData item, int amount = 1)
    {
        // Guard against callers forgetting to pass an item asset.
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

    // Read-only view of all current stacks. UI scripts use this to render
    // the inventory bar. Returned as IReadOnlyList so callers cannot mutate
    // the internal list — they must go through AddItem / RemoveItem.
    public IReadOnlyList<InventoryStack> GetStacks()
    {
        return stacks;
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
