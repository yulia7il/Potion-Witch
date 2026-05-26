using UnityEngine;

// Holds a single plant in a pot. Built so future steps
// (growth stages, watering, harvesting, animations, VFX)
// slot in as new small methods rather than growing Plant().
//
// Implements IPopupGate so a PopupOpener on the same pot can ask
// "is this plant harvestable right now?" before opening the harvest popup.
// Click detection + overlay management live in PopupOpener / PopupManager.
public class PlantPot : MonoBehaviour, IPopupGate
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

    [Tooltip("SunJar linked to this pot. Receives the sun budget when a plant is placed.")]
    [SerializeField] private SunSpawner sunSpawner;

    [Tooltip("Scene InventoryManager. Receives the plant's harvest item when Harvest() runs.")]
    [SerializeField] private InventoryManager inventoryManager;

    // Cached references to the plant we spawned. Kept private so external
    // callers go through Water() / future helpers instead of poking at state.
    private GameObject currentPlantInstance;
    private PlantGrowth currentPlantGrowth;

    // The PlantData used for the currently planted plant. Stored so Harvest()
    // can look up which inventory item to grant. Cleared on harvest.
    private PlantData currentPlantData;

    // True once the water meter has been filled for the current plant. Locks
    // out further watering progress so the plant only grows one stage from
    // a single fill cycle.
    private bool hasBeenWatered;

    private void Awake()
    {
        if (waterMeterUI != null) waterMeterUI.Hide();
    }

    // ---------- Harvest gate ----------

    // True when this pot holds a fully grown plant the player can harvest.
    // Mature is the final reachable stage in current gameplay (Full is the
    // reserved-for-later third visual). Change this if Full gets wired up.
    public bool CanHarvest()
    {
        if (!isOccupied) return false;
        if (currentPlantGrowth == null) return false;
        return currentPlantGrowth.CurrentStage == PlantGrowth.GrowthStage.Mature;
    }

    // IPopupGate — used by PopupOpener on this pot to gate the harvest popup.
    bool IPopupGate.CanOpen() => CanHarvest();

    // ---------- Planting ----------

    // Public entry point. Called by UIDragSeed when a seed is dropped on this pot.
    // Returns true if a plant was actually spawned.
    public bool Plant(PlantData plantData)
    {
        if (!CanPlant(plantData)) return false;

        currentPlantData = plantData;
        SpawnPlant(plantData.plantPrefab);
        MarkAsOccupied();
        HidePlusSignHint();

        if (waterMeterUI != null) waterMeterUI.ResetMeter();
        sunSlotsManager?.ShowSlots(plantData.requiredSunCount);
        sunSpawner?.SetAvailableSuns(plantData.requiredSunCount);
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

    // ---------- Sun ----------

    // Called by SunSlotsManager when every active SunSlot has been filled.
    public void OnAllSunSlotsFilled()
    {
        currentPlantGrowth?.GrowToNextStage();
    }

    // ---------- Harvest ----------

    // Public entry point. Called by HarvestCollectButton when the player
    // clicks Collect in the harvest popup. Grants the plant's harvest item
    // to the inventory, then resets the pot to empty state.
    // Closing the popup itself is handled by PopupCloser on the same button,
    // so this method only owns the gameplay reset + inventory grant.
    public void Harvest()
    {
        if (!CanHarvest()) return;

        AddHarvestToInventory();
        DestroyPlantInstance();
        HideWaterMeter();
        HideSunSlots();
        ResetSunSpawner();
        ShowPlusSignHint();
        ClearOccupancy();
    }

    // Looks up the inventory item defined on the planted PlantData and adds
    // one to the InventoryManager. Skips if any wiring is missing so a
    // half-configured pot still resets cleanly. Missing data emits a warning
    // so setup problems are obvious in the Console.
    private void AddHarvestToInventory()
    {
        // Always prefer the persistent singleton — survives scene loads, so
        // pots that weren't wired up in the Inspector still hit the right
        // inventory. The serialized field is kept as a fallback for scenes
        // launched in isolation (Play directly in the Garden scene) where
        // the singleton may not have been created yet.
        InventoryManager target = InventoryManager.Instance != null ? InventoryManager.Instance : inventoryManager;

        if (target == null)
        {
            Debug.LogWarning($"[Harvest] '{name}' could not find an InventoryManager. " +
                             "Make sure one exists in the scene (or persists via DontDestroyOnLoad).");
            return;
        }

        if (currentPlantData == null)
        {
            Debug.LogWarning($"[Harvest] '{name}' has no currentPlantData — nothing to add to inventory.");
            return;
        }

        InventoryItemData item = currentPlantData.HarvestItem;
        if (item == null)
        {
            Debug.LogWarning($"[Harvest] PlantData '{currentPlantData.name}' has no HarvestItem assigned — assign one in the Inspector.");
            return;
        }

        target.AddItem(item, 1);
        Debug.Log($"[Harvest] Collected {item.itemName} (Total: {target.GetAmount(item)})");
    }

    private void DestroyPlantInstance()
    {
        if (currentPlantInstance != null) Destroy(currentPlantInstance);
        currentPlantInstance = null;
        currentPlantGrowth = null;
    }

    private void HideWaterMeter()
    {
        if (waterMeterUI != null) waterMeterUI.Hide();
    }

    private void HideSunSlots()
    {
        if (sunSlotsManager != null) sunSlotsManager.HideSlots();
    }

    private void ResetSunSpawner()
    {
        if (sunSpawner != null) sunSpawner.SetAvailableSuns(0);
    }

    // Mirror of HidePlusSignHint — brings the empty-pot indicator back so
    // the player knows they can plant a fresh seed.
    private void ShowPlusSignHint()
    {
        if (plusSignHint == null) return;
        plusSignHint.SetActive(true);
    }

    // Clears the flags that gate planting / watering for the next cycle.
    private void ClearOccupancy()
    {
        isOccupied = false;
        hasBeenWatered = false;
        currentPlantData = null;
    }

    // ---------- Water ----------

    // Called by WorldDraggableTool (Water Can) when released on this pot.
    // Returns true if watering actually advanced the plant.
    public bool Water()
    {
        if (!isOccupied) return false;
        if (currentPlantGrowth == null) return false;

        currentPlantGrowth.GrowToNextStage();
        return true;
    }

    // Called by WaterParticleCollision each time a water particle hits the pot.
    // Advances the UI meter; once the meter completes, performs the one-shot
    // watering (Water() + hide meter) and locks out further hits.
    // Returns true on the call that completed the meter.
    public bool AddWaterProgress(float amount)
    {
        if (!isOccupied) return false;
        if (currentPlantGrowth == null) return false;
        if (hasBeenWatered) return false;
        if (waterMeterUI == null) return false;

        bool filled = waterMeterUI.AddFill(amount);
        if (!filled) return false;

        hasBeenWatered = true;
        Water();
        waterMeterUI.Hide();
        return true;
    }
}
