using UnityEngine;

// Sits on the root of a spawned plant prefab and toggles which growth-stage
// child sprite is visible. Designed to grow with the game: more stages, timers,
// VFX, and save data can be layered on without changing how PlantPot drives it.
public class PlantGrowth : MonoBehaviour
{
    // The three visual stage roots. Assigned in the prefab Inspector.
    // Each one is a child GameObject that holds the sprite for that stage.
    [Tooltip("Visible when the plant has just been planted.")]
    public GameObject sporeStage;

    [Tooltip("Visible after the first watering.")]
    public GameObject matureStage;

    [Tooltip("Visible at the final growth stage. Unused for now — kept off.")]
    public GameObject fullStage;

    public enum GrowthStage
    {
        Spore,
        Mature,
        Full
    }

    // Current stage. Exposed so future systems (UI, save) can read it.
    public GrowthStage CurrentStage { get; private set; } = GrowthStage.Spore;

    private void Start()
    {
        // Plant begins life as a spore.
        ShowStage(GrowthStage.Spore);
    }

    // Public entry point. PlantPot calls this when the player waters the pot.
    // Advances Spore → Mature for now. Mature → Full will hook in later.
    public void GrowToNextStage()
    {
        if (CurrentStage == GrowthStage.Spore)
        {
            ShowStage(GrowthStage.Mature);
        }
        // Mature → Full intentionally left out until later requirements land.
    }

    // Centralized visibility switch. All show/hide goes through here so adding
    // a new stage later is a one-line addition instead of a scattered edit.
    private void ShowStage(GrowthStage stage)
    {
        CurrentStage = stage;
        SetActiveSafe(sporeStage, stage == GrowthStage.Spore);
        SetActiveSafe(matureStage, stage == GrowthStage.Mature);
        SetActiveSafe(fullStage, stage == GrowthStage.Full);
    }

    // Null-safe SetActive — keeps the prefab usable even if a stage slot is
    // intentionally left empty during early authoring.
    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go == null) return;
        go.SetActive(active);
    }
}
