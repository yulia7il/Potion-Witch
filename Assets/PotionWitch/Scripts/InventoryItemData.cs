using UnityEngine;

// ScriptableObject that defines a single inventory item type.
// Examples: ITEM_Mint, ITEM_Sage.
//
// A PlantData references one of these via its harvestItem field so the
// harvest flow knows what to give the player. Kept intentionally tiny
// for the MVP — no categories, no stack limits, no potion data yet.
[CreateAssetMenu(menuName = "Game/Inventory Item", fileName = "ITEM_New")]
public class InventoryItemData : ScriptableObject
{
    [Tooltip("Display name of the item, e.g. \"Mint\".")]
    public string itemName;

    [Tooltip("Icon shown in inventory UI later.")]
    public Sprite icon;
}
