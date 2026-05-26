using System.Collections.Generic;
using UnityEngine;

// Play Mode testing helper for the Cauldron inventory UI. Lets us pump items
// into the inventory without harvesting plants every time.
//
// Drop this on any GameObject in the Cauldron scene, assign the item assets
// and (optionally) the bar UI, then right-click the component header in the
// Inspector during Play Mode to invoke the context-menu actions.
//
// Not wired to any gameplay buttons by design — keep it strictly a debug tool.
public class InventoryDebugCheats : MonoBehaviour
{
    [Tooltip("Mint item asset, e.g. ITEM_Mint.")]
    [SerializeField] private InventoryItemData mintItem;

    [Tooltip("Sage item asset, e.g. ITEM_Sage.")]
    [SerializeField] private InventoryItemData sageItem;

    [Tooltip("Reference kept for Inspector visibility / future use. Cheats route through InventoryManager.Instance.")]
    [SerializeField] private InventoryManager inventoryManager;

    [Tooltip("Optional. If assigned, RefreshCauldronUI re-syncs the bar after a cheat.")]
    [SerializeField] private CauldronInventoryBarUI cauldronInventoryBarUI;

    // Public so the custom Inspector (InventoryDebugCheatsEditor) can invoke
    // them from buttons. The [ContextMenu] attributes also stay so the
    // three-dot menu keeps working.
    [ContextMenu("Add Mint")]
    public void AddMint()
    {
        AddOne(mintItem, "mint");
    }

    [ContextMenu("Add Sage")]
    public void AddSage()
    {
        AddOne(sageItem, "sage");
    }

    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("[InventoryDebugCheats] InventoryManager.Instance is null. Nothing to clear.");
            return;
        }

        // Copy first — RemoveItem mutates the underlying list, so iterating
        // GetStacks() directly would skip entries.
        List<InventoryStack> snapshot = new List<InventoryStack>(inventory.GetStacks());
        for (int i = 0; i < snapshot.Count; i++)
        {
            InventoryStack stack = snapshot[i];
            if (stack != null && stack.item != null)
            {
                inventory.RemoveItem(stack.item, stack.amount);
            }
        }

        Debug.Log("[InventoryDebugCheats] Inventory cleared.");
    }

    [ContextMenu("Refresh Cauldron UI")]
    public void RefreshCauldronUI()
    {
        if (cauldronInventoryBarUI == null)
        {
            Debug.LogWarning("[InventoryDebugCheats] cauldronInventoryBarUI is not assigned.");
            return;
        }
        cauldronInventoryBarUI.Refresh();
    }

    private void AddOne(InventoryItemData item, string label)
    {
        if (item == null)
        {
            Debug.LogWarning($"[InventoryDebugCheats] {label} item asset is not assigned.");
            return;
        }

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("[InventoryDebugCheats] InventoryManager.Instance is null. Enter Play Mode first.");
            return;
        }

        inventory.AddItem(item, 1);
        Debug.Log($"[InventoryDebugCheats] Added 1 {label}.");
    }
}
