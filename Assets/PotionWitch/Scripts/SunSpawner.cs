using System.Collections.Generic;
using UnityEngine;

// Attach to SunJar. Spawns 2–5 suns at random unique spawn points on Start.
//
// Inspector checklist:
//   [ ] sunPrefab assigned (must have WorldDraggableTool with ToolType = Sun and a Collider2D)
//   [ ] spawnPoints assigned (Point_0 … Point_4 children of SunJar)
//   [ ] SunSlot objects each have Collider2D on root (or child) + silhouette + filledSun assigned
//   [ ] Camera.main is valid, or assign dragCamera on each spawned sun's WorldDraggableTool
public class SunSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject sunPrefab;
    public Transform[] spawnPoints;

    [Header("Spawn Count")]
    [Min(1)] public int minSuns = 2;
    [Min(1)] public int maxSuns = 5;

    private void Start()
    {
        if (sunPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[SunSpawner] sunPrefab or spawnPoints not assigned.", this);
            return;
        }

        int count = Mathf.Clamp(Random.Range(minSuns, maxSuns + 1), 1, spawnPoints.Length);

        // Partial Fisher-Yates to get `count` unique indices without extra allocation.
        List<int> indices = new List<int>(spawnPoints.Length);
        for (int i = 0; i < spawnPoints.Length; i++) indices.Add(i);

        for (int i = 0; i < count; i++)
        {
            int pick = Random.Range(i, indices.Count);
            (indices[i], indices[pick]) = (indices[pick], indices[i]);
        }

        for (int i = 0; i < count; i++)
        {
            Transform point = spawnPoints[indices[i]];
            GameObject sun = Instantiate(sunPrefab, point.position, Quaternion.identity, transform);
            float scale = Random.Range(0.9f, 1.1f);
            sun.transform.localScale = Vector3.one * scale;
        }
    }
}
