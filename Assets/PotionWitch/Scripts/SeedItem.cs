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
}
