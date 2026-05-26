using System.Collections.Generic;
using UnityEngine;

// Drives the cauldron inventory bar. The slot GameObjects already exist in
// the scene (built by hand) — this script just pushes data into them.
//
// Reads from InventoryManager.Instance so it always points at the persistent
// (DontDestroyOnLoad) inventory, even when the Cauldron scene's own local
// InventoryManager gets destroyed as a duplicate on scene load.
//
// MVP scope:
//   - Pull-based refresh only (OnEnable + manual ContextMenu).
//   - No drag/drop, no crafting, no consuming.
//   - Slots beyond the inventory count are cleared; extra stacks are dropped
//     with a warning until paging/scrolling exists.
public class CauldronInventoryBarUI : MonoBehaviour
{
    [Tooltip("Fixed slot UIs, in display order. Assigned in the Inspector.")]
    [SerializeField] private List<CauldronInventorySlotUI> slots = new List<CauldronInventorySlotUI>();

    // Set true after the first Start(). Used to skip the very first OnEnable,
    // which fires before InventoryManager.Awake has had a chance to set Instance
    // when the Cauldron scene is the first scene loaded.
    private bool hasStarted = false;

    // Start runs after every Awake in the scene, so InventoryManager.Instance
    // is guaranteed to be set by now (if one exists in the scene at all).
    private void Start()
    {
        hasStarted = true;
        Refresh();
    }

    // Re-sync whenever the bar becomes visible again (e.g. cauldron popup
    // reopens). Skipped on the first activation — Start() handles that.
    private void OnEnable()
    {
        if (hasStarted) Refresh();
    }

    // Reads the current inventory and pushes it into the fixed slots.
    public void Refresh()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            // No persistent inventory yet (e.g. scene was opened without one).
            // Blank the slots so we don't leave stale icons sitting on screen.
            Debug.LogWarning("[CauldronInventoryBarUI] InventoryManager.Instance is null. Clearing slots.");
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null) slots[i].Clear();
            }
            return;
        }

        IReadOnlyList<InventoryStack> stacks = inventory.GetStacks();

        // Fill as many slots as we have stacks for.
        int fillCount = Mathf.Min(stacks.Count, slots.Count);
        for (int i = 0; i < fillCount; i++)
        {
            if (slots[i] != null) slots[i].Setup(stacks[i]);
            else Debug.LogWarning($"[CauldronInventoryBarUI] Slot[{i}] reference is null — assign it in the Inspector.");
        }

        // Blank out the remaining slots so old icons don't linger.
        for (int i = fillCount; i < slots.Count; i++)
        {
            if (slots[i] != null) slots[i].Clear();
        }

        // No paging yet — warn loudly so we notice when we outgrow the bar.
        if (stacks.Count > slots.Count)
        {
            Debug.LogWarning($"[CauldronInventoryBarUI] Inventory has {stacks.Count} stacks but only {slots.Count} slots. Extras are not shown.");
        }
    }

    // Manual refresh hook for testing from the Inspector during Play Mode.
    [ContextMenu("Refresh")]
    private void RefreshFromInspector()
    {
        Refresh();
    }
}
