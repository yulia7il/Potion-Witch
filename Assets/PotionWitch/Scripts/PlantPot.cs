using UnityEngine;

// Holds a single plant in a pot. Built so future steps
// (growth stages, watering, harvesting, animations, VFX)
// slot in as new small methods rather than growing Plant().
public class PlantPot : MonoBehaviour
{
    [Tooltip("Where the plant prefab will appear. If null, the pot's own transform is used.")]
    public Transform plantSpawnPoint;

    [Tooltip("True once a plant has been placed. Prevents planting twice in the same pot.")]
    public bool isOccupied;

    [Tooltip("Optional. UI / sprite hint shown over the empty pot (e.g. a 'plus' icon). Hidden once a plant is placed.")]
    public GameObject plusSignHint;

    [Tooltip("Optional. WaterMeter UI shown over the pot once a plant is placed.")]
    [SerializeField] private PlantWaterMeterUI waterMeterUI;

    [Tooltip("SunSlots row linked to this pot. Activated with the correct count when a plant is placed.")]
    [SerializeField] private SunSlotsManager sunSlotsManager;

    // Cached references to the plant we spawned. Kept private so external
    // callers go through Water() / future helpers instead of poking at state.
    private GameObject currentPlantInstance;
    private PlantGrowth currentPlantGrowth;

    private void Awake()
    {
        if (waterMeterUI != null) waterMeterUI.Hide();
    }

    // Public entry point. Called by UIDragSeed when a seed is dropped on this pot.
    // Returns true if a plant was actually spawned.
    public bool Plant(PlantData plantData)
    {
        if (!CanPlant(plantData)) return false;

        SpawnPlant(plantData.plantPrefab);
        MarkAsOccupied();
        HidePlusSignHint();

        Debug.Log($"[WaterMeter] Plant() succeeded on '{name}'.");
        Debug.Log($"[WaterMeter] waterMeterUI is {(waterMeterUI == null ? "NULL" : "assigned: '" + waterMeterUI.name + "'")}.");
        if (waterMeterUI != null)
        {
            Debug.Log($"[WaterMeter] Calling waterMeterUI.Show() on '{waterMeterUI.name}'.");
            waterMeterUI.Show();
        }
        sunSlotsManager?.ShowSlots(plantData.requiredSunCount);
        return true;
    }

    // Called by WorldDraggableTool (Water Can) when released on this pot.
    // Returns true if watering actually advanced the plant.
    public bool Water()
    {
        if (!isOccupied) return false;
        if (currentPlantGrowth == null) return false;

        currentPlantGrowth.GrowToNextStage();
        return true;
    }

    // Pure validation: is this pot ready to receive this seed right now?
    private bool CanPlant(PlantData plantData)
    {
        if (isOccupied) return false;
        if (plantData == null) return false;
        if (plantData.plantPrefab == null) return false;
        return true;
    }

    // Creates the plant instance parented under the pot at the spawn anchor
    // and caches the PlantGrowth component so Water() can drive it later.
    private void SpawnPlant(GameObject prefab)
    {
        Transform anchor = GetSpawnAnchor();
        currentPlantInstance = Instantiate(prefab, anchor.position, Quaternion.identity, transform);
        currentPlantGrowth = currentPlantInstance.GetComponent<PlantGrowth>();
    }

    // Picks where the plant should appear: explicit marker if set, else the pot itself.
    private Transform GetSpawnAnchor()
    {
        return plantSpawnPoint != null ? plantSpawnPoint : transform;
    }

    // Centralizes occupancy state so future hooks (events, save data) can attach here.
    private void MarkAsOccupied()
    {
        isOccupied = true;
    }

    // Hides the plus-sign hint if one was assigned. No-op otherwise.
    private void HidePlusSignHint()
    {
        if (plusSignHint == null) return;
        plusSignHint.SetActive(false);
    }
}
