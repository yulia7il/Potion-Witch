using UnityEngine;

// One slot in the SunSlots row. Starts as an empty silhouette and flips to
// a filled sun once a dragged Sun is dropped on it. Future steps (counting
// filled slots, triggering effects when all are full) can hook into Fill().
public class SunSlot : MonoBehaviour
{
    [Tooltip("Empty-slot sprite shown before the slot is filled.")]
    public GameObject silhouette;

    [Tooltip("Filled-slot sprite shown after a Sun has been dropped here.")]
    public GameObject filledSun;

    [Tooltip("True once a Sun has been placed. Prevents filling the same slot twice.")]
    public bool isFilled;

    private void Start()
    {
        if (silhouette != null) silhouette.SetActive(true);
        if (filledSun != null) filledSun.SetActive(false);
    }

    // Resets the slot to its empty visual state. Called by SunSlotsManager on re-planting.
    public void ResetSlot()
    {
        isFilled = false;
        if (silhouette != null) silhouette.SetActive(true);
        if (filledSun != null) filledSun.SetActive(false);
    }

    // Called by WorldDraggableTool (Sun) when a Sun is released over this slot.
    // Returns true if the slot actually accepted the Sun.
    public bool Fill()
    {
        if (isFilled) return false;

        isFilled = true;
        if (silhouette != null) silhouette.SetActive(false);
        if (filledSun != null) filledSun.SetActive(true);
        return true;
    }
}
