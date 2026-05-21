using UnityEngine;
using UnityEngine.InputSystem;

// Generic world-space drag handler for garden tools (Water Can, etc.).
// Polls mouse input in Update() instead of relying on Unity's OnMouseDown
// messages, which silently fail in many setups (wrong physics raycaster,
// sorting layer, EventSystem blocking, etc.). Polling keeps the flow visible
// and easy to debug.
[RequireComponent(typeof(Collider2D))]
public class WorldDraggableTool : MonoBehaviour
{
    // Drop-target behavior switches on this. WaterCan looks for PlantPots,
    // Sun looks for SunSlots. Future tools can add their own case.
    public enum ToolType
    {
        WaterCan,
        Sun
    }

    [Tooltip("What this tool does on release. WaterCan -> PlantPot.Water(). Sun -> SunSlot.Fill().")]
    [SerializeField] private ToolType toolType = ToolType.WaterCan;

    [Tooltip("Optional. Camera used to convert mouse screen position into world space. " +
             "Falls back to Camera.main if left empty.")]
    public Camera dragCamera;

    [Tooltip("Optional. Particle system that plays while the tool is being dragged. " +
             "Currently only used by WaterCan.")]
    [SerializeField] private ParticleSystem dragParticles;

    [Tooltip("Z rotation (degrees) applied to the WaterCan while it is being dragged, " +
             "so it tilts as if pouring. Restored on release.")]
    [SerializeField] private float waterCanDragTiltZ = -25f;

    [Tooltip("Optional. Child transform holding the WaterCan sprite (e.g. WaterCan_Visual). " +
             "When set, only this child tilts during drag, so sibling children like the water " +
             "ParticleSystem keep their original orientation. If left null, the root tilts as before.")]
    [SerializeField] private Transform waterCanVisual;

    // Set by SunSpawner at instantiation time so this sun can report back when resolved.
    [HideInInspector] public SunSpawner parentSpawner;

    // Our own collider, cached in Awake. On pointer-down we test whether the
    // Physics2D hit under the cursor belongs to this object (or a child).
    private Collider2D ownCollider;

    // Cached so we can snap the tool back if the drop target is invalid.
    private Vector3 startPosition;

    // Cached so the WaterCan's pour-tilt can be undone on release. Also
    // restored for any tool routed through ReturnToStart, which is harmless
    // for tools that never rotated. Used as fallback when waterCanVisual
    // is not assigned.
    private Quaternion originalRotation;

    // Cached localRotation of waterCanVisual at Awake. Used to restore the
    // visual on release without disturbing the root, so siblings (like the
    // water ParticleSystem) keep their world rotation.
    private Quaternion originalVisualLocalRotation;

    // Offset between the tool's pivot and the mouse at the moment drag started.
    // Without this the tool would jump so its pivot sits exactly on the cursor.
    private Vector3 grabOffset;

    // Z value of the tool when drag began. Mouse position is 2D, but the
    // transform lives in 3D — we preserve the original depth so the sprite
    // doesn't drift toward/away from the camera while dragging.
    private float dragDepth;

    private bool isDragging;

    // ---------- Unity messages ----------

    private void Awake()
    {
        startPosition = transform.position;
        originalRotation = transform.rotation;
        if (waterCanVisual != null) originalVisualLocalRotation = waterCanVisual.localRotation;
        ownCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // No mouse device present (e.g. headset / pure touch). Nothing to do.
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // Pointer down: only start a drag if the click landed on this tool.
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseWorld = GetMouseWorldPosition();
            Vector2 point = new Vector2(mouseWorld.x, mouseWorld.y);
            Collider2D hit = Physics2D.OverlapPoint(point);

            if (hit != null && IsOurCollider(hit))
            {
                BeginDrag();
            }
        }

        // Mid-drag: keep following the cursor every frame while held.
        if (isDragging && mouse.leftButton.isPressed)
        {
            Drag();
        }

        // Pointer up: release if we had started a drag.
        if (isDragging && mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    // True when the collider Physics2D found belongs to this tool — either
    // the collider on the root or one on a child object.
    private bool IsOurCollider(Collider2D hit)
    {
        if (hit == ownCollider) return true;
        return hit.transform.IsChildOf(transform);
    }

    // ---------- Drag lifecycle ----------

    // Records the offset between the tool and the cursor so the sprite
    // stays anchored to the grab point instead of snapping to the pointer.
    private void BeginDrag()
    {
        dragDepth = transform.position.z;
        Vector3 mouseWorld = GetMouseWorldPosition();
        grabOffset = transform.position - mouseWorld;
        isDragging = true;

        if (toolType == ToolType.WaterCan)
        {
            Quaternion tilt = Quaternion.Euler(0f, 0f, waterCanDragTiltZ);
            if (waterCanVisual != null)
            {
                waterCanVisual.localRotation = originalVisualLocalRotation * tilt;
            }
            else
            {
                transform.rotation = originalRotation * tilt;
            }
            if (dragParticles != null) dragParticles.Play();
        }
    }

    // Updates the tool's world position to follow the mouse, keeping the
    // original grab offset and original Z depth.
    private void Drag()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector3 target = mouseWorld + grabOffset;
        target.z = dragDepth;
        transform.position = target;
    }

    // Decides what happens when the player releases the tool.
    // Branches on toolType so each tool only looks for targets it cares about.
    private void EndDrag()
    {
        isDragging = false;

        if (toolType == ToolType.WaterCan && dragParticles != null)
        {
            dragParticles.Stop();
        }

        switch (toolType)
        {
            case ToolType.WaterCan:
                HandleWaterCanRelease();
                break;
            case ToolType.Sun:
                HandleSunRelease();
                break;
            default:
                OnDroppedOnEmpty();
                break;
        }
    }

    // WaterCan: watering is driven by particle collisions (see
    // WaterParticleCollision), so the drop itself just sends the can home.
    private void HandleWaterCanRelease()
    {
        ReturnToStart();
    }

    // Sun: look for an empty SunSlot, fill it and consume the dragged sun.
    // Own collider is disabled before the overlap check so Physics2D sees through
    // the Sun to the slot underneath, then re-enabled only if the Sun returns to start.
    private void HandleSunRelease()
    {
        ownCollider.enabled = false;

        SunSlot slot = TryFindSunSlotUnderPointer();

        if (slot == null)
        {
            ownCollider.enabled = true;
            ReturnToStart();
            return;
        }

        if (slot.Fill())
        {
            slot.GetComponentInParent<SunSlotsManager>()?.CheckCompletion();
            parentSpawner?.NotifyActiveSunResolved();
            Destroy(gameObject);
        }
        else
        {
            ownCollider.enabled = true;
            ReturnToStart();
        }
    }

    // ---------- Extension hooks ----------

    // Called when the tool was released over empty ground.
    // Default: send the tool back to where the player picked it up.
    protected virtual void OnDroppedOnEmpty()
    {
        ReturnToStart();
    }

    // ---------- Helpers ----------

    // Converts the current mouse screen position into a world-space point
    // using the configured drag camera (or Camera.main as a fallback).
    private Vector3 GetMouseWorldPosition()
    {
        Camera cam = dragCamera != null ? dragCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogWarning($"[WorldDrag] GetMouseWorldPosition on '{name}' — no camera found (dragCamera unassigned and Camera.main is null)");
            return transform.position;
        }

        // New Input System: read the mouse position from the current Mouse device.
        // If no mouse is connected, fall back to the tool's own position.
        if (Mouse.current == null) return transform.position;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 screen = new Vector3(mousePos.x, mousePos.y, 0f);
        // For an orthographic 2D camera the z component of ScreenToWorldPoint
        // sets the distance from the camera. Using the camera's near plane is
        // a safe default; the actual depth is restored in Drag().
        screen.z = Mathf.Abs(cam.transform.position.z - dragDepth);
        return cam.ScreenToWorldPoint(screen);
    }

    // Looks for a SunSlot whose Collider2D contains the current mouse position.
    // Returns null if nothing matching is found.
    private SunSlot TryFindSunSlotUnderPointer()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector2 point = new Vector2(mouseWorld.x, mouseWorld.y);

        Collider2D hit = Physics2D.OverlapPoint(point);
        if (hit == null) return null;

        // GetComponentInParent so the slot's collider can sit on a child object
        // (e.g. on the Silhouette child rather than the Slot root).
        return hit.GetComponentInParent<SunSlot>();
    }

    // Snaps the tool back to where it sat at the start of the scene.
    // Public so other systems (e.g. a future ToolManager) can force a reset.
    public void ReturnToStart()
    {
        transform.position = startPosition;
        if (waterCanVisual != null)
        {
            waterCanVisual.localRotation = originalVisualLocalRotation;
        }
        else
        {
            transform.rotation = originalRotation;
        }
    }
}
