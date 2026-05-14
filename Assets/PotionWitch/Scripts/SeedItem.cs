using UnityEngine;

public enum PlantItemType { Seed, Leaf }

[CreateAssetMenu(menuName = "Game/Plant")]
public class PlantData : ScriptableObject
{
    public string id;         // "mint" / "sage"
    public Sprite seedIcon;   // אייקון זרעים
    public Sprite leafIcon;   // אייקון עלים
    public GameObject plantPrefab; // אופציונלי
}
