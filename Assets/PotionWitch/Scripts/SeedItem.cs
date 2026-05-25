using UnityEngine;

public enum PlantItemType { Seed, Leaf }

[CreateAssetMenu(menuName = "Game/Plant")]
public class PlantData : ScriptableObject
{
    public string id;         // "mint" / "sage"
    public Sprite seedIcon;   // ������ �����
    public Sprite leafIcon;   // ������ ����
    public GameObject plantPrefab; // ���������
    [Min(0)] public int requiredSunCount = 3;

    [Tooltip("Inventory item produced when this plant is harvested. Drop the matching InventoryItemData asset here (e.g. ITEM_Mint for Plant_Mint).")]
    [SerializeField] private InventoryItemData harvestItem;

    // Read-only accessor so PlantPot can grant this item on harvest without
    // making the field public.
    public InventoryItemData HarvestItem => harvestItem;
}
