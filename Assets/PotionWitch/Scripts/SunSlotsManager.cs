using System.Collections.Generic;
using UnityEngine;

// Attach to the SunSlots parent object.
// Controls how many slots are visible based on the planted crop's sun requirement.
//
// Inspector checklist:
//   [ ] slots list populated with Slot_0 … Slot_4 in order
//   [ ] SunSlotsManager reference assigned on the PlantPot for this bed
public class SunSlotsManager : MonoBehaviour
{
    [Tooltip("Ordered list of SunSlot children (Slot_0 first).")]
    public List<SunSlot> slots;

    [Tooltip("The PlantPot to notify when all active slots are filled.")]
    [SerializeField] private PlantPot plantPot;

    // Activates the first `amount` slots in their empty state; deactivates the rest.
    public void ShowSlots(int amount)
    {
        if (slots == null) return;

        amount = Mathf.Clamp(amount, 0, slots.Count);

        for (int i = 0; i < slots.Count; i++)
        {
            SunSlot slot = slots[i];
            if (slot == null) continue;

            if (i < amount)
            {
                slot.gameObject.SetActive(true);
                slot.ResetSlot();
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    // Returns true only if every currently active slot has been filled.
    public bool AreAllActiveSlotsFilled()
    {
        if (slots == null) return false;
        foreach (SunSlot slot in slots)
        {
            if (slot == null) continue;
            if (!slot.gameObject.activeInHierarchy) continue;
            if (!slot.isFilled) return false;
        }
        return true;
    }

    // Called by WorldDraggableTool after each successful slot fill.
    // Notifies PlantPot if all active slots are now filled.
    public void CheckCompletion()
    {
        if (!AreAllActiveSlotsFilled()) return;
        Debug.Log("[SunSlotsManager] All active slots filled");
        plantPot?.OnAllSunSlotsFilled();
    }
}
