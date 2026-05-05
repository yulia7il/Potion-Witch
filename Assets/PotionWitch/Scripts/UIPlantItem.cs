using UnityEngine;
using UnityEngine.UI;


public class UIPlantItem : MonoBehaviour
{
    public PlantData plantData;          // פה תחברי את ה-Asset
    public PlantItemType itemType = PlantItemType.Seed;

    [Header("Optional: auto-assign icon")]
    public Image iconImage;

    private void OnValidate()
    {
        // כדי שתראי את האייקון מיד באדיטור (לא חובה)
        if (iconImage == null) iconImage = GetComponent<Image>();
        if (iconImage == null || plantData == null) return;

        iconImage.sprite = (itemType == PlantItemType.Seed) ? plantData.seedIcon : plantData.leafIcon;
    }
}
