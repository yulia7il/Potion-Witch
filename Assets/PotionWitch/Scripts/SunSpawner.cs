using UnityEngine;
using UnityEngine.InputSystem;

// Attach to SunJar. Polls the New Input System in Update() — same pattern as
// WorldDraggableTool — so clicks are detected reliably regardless of EventSystem
// or physics raycaster setup.
[RequireComponent(typeof(Collider2D))]
public class SunSpawner : MonoBehaviour
{
    [Tooltip("Sun prefab to instantiate. Must have WorldDraggableTool (ToolType = Sun) and Collider2D.")]
    public GameObject sunPrefab;

    [Tooltip("Where the spawned Sun appears. Assign the SunSpawnPoint child.")]
    public Transform sunSpawnPoint;

    [Tooltip("Optional. Falls back to Camera.main if left empty.")]
    public Camera spawnCamera;

    [Tooltip("How many suns are left to spawn. Set via SetAvailableSuns().")]
    public int availableSuns;

    [Tooltip("True while a spawned Sun is still in the scene and unresolved.")]
    public bool hasActiveSun;

    private Collider2D ownCollider;

    private void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (!mouse.leftButton.wasPressedThisFrame) return;

        Vector3 worldPos = GetPointerWorldPosition();
        Debug.Log($"[SunSpawner] Pointer down at world pos {worldPos}");

        Collider2D hit = Physics2D.OverlapPoint(new Vector2(worldPos.x, worldPos.y));

        if (hit == null)
        {
            Debug.Log("[SunSpawner] No collider hit");
            return;
        }

        Debug.Log($"[SunSpawner] Hit collider: {hit.name}");

        if (hit != ownCollider && !hit.transform.IsChildOf(transform))
        {
            Debug.Log($"[SunSpawner] Hit does not belong to SunJar");
            return;
        }

        Debug.Log("[SunSpawner] Hit belongs to SunJar");
        TrySpawn();
    }

    private void TrySpawn()
    {
        if (availableSuns <= 0)
        {
            Debug.Log("[SunSpawner] Blocked: no available suns");
            return;
        }

        if (hasActiveSun)
        {
            Debug.Log("[SunSpawner] Blocked: already has active sun");
            return;
        }

        SpawnSun();
    }

    // Called by PlantPot (or any future system) when a new crop is planted.
    public void SetAvailableSuns(int amount)
    {
        availableSuns = amount;
        hasActiveSun = false;
    }

    // Called by WorldDraggableTool after a Sun is successfully dropped into a slot.
    public void NotifyActiveSunResolved()
    {
        hasActiveSun = false;
    }

    private void SpawnSun()
    {
        Transform spawnAt = sunSpawnPoint != null ? sunSpawnPoint : transform;
        GameObject sun = Instantiate(sunPrefab, spawnAt.position, Quaternion.identity, transform);

        WorldDraggableTool drag = sun.GetComponent<WorldDraggableTool>();
        if (drag != null) drag.parentSpawner = this;

        availableSuns--;
        hasActiveSun = true;
        Debug.Log("[SunSpawner] Spawned Sun");
    }

    private Vector3 GetPointerWorldPosition()
    {
        Camera cam = spawnCamera != null ? spawnCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[SunSpawner] No camera found — spawnCamera unassigned and Camera.main is null");
            return Vector3.zero;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 screen = new Vector3(mousePos.x, mousePos.y, Mathf.Abs(cam.transform.position.z));
        return cam.ScreenToWorldPoint(screen);
    }
}
