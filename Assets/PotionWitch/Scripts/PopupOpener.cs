using UnityEngine;
using UnityEngine.InputSystem;

// Attach to any world-space GameObject with a Collider2D. On left-click that
// lands on the collider (or a child of this transform), opens the assigned
// popup through PopupManager.
//
// Optional: assign a 'gate' — any MonoBehaviour that implements IPopupGate.
// If set, the popup only opens when gate.CanOpen() returns true. Used by
// PlantPot so the harvest popup only opens when the plant is fully grown.
[RequireComponent(typeof(Collider2D))]
public class PopupOpener : MonoBehaviour
{
    [Tooltip("Scene PopupManager that handles the shared overlay and currently-open popup.")]
    [SerializeField] private PopupManager popupManager;

    [Tooltip("Popup GameObject to open when this object is clicked.")]
    [SerializeField] private GameObject popup;

    [Tooltip("Optional. MonoBehaviour implementing IPopupGate that decides whether the popup may open on click. Leave empty to always open.")]
    [SerializeField] private MonoBehaviour gateBehaviour;

    [Tooltip("Optional. Camera used to convert clicks into world space. Falls back to Camera.main.")]
    [SerializeField] private Camera clickCamera;

    private Collider2D ownCollider;
    private IPopupGate gate;

    private void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
        gate = gateBehaviour as IPopupGate;

        if (gateBehaviour != null && gate == null)
        {
            Debug.LogWarning($"[PopupOpener] '{name}' has a gateBehaviour assigned that does not implement IPopupGate — gate will be ignored.");
        }
    }

    private void Update()
    {
        if (!WasLeftClickedThisFrame()) return;
        if (!IsClickOnOurCollider()) return;
        if (!IsGateAllowingOpen()) return;

        TryOpenPopup();
    }

    // ---------- Steps ----------

    private bool WasLeftClickedThisFrame()
    {
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }

    private bool IsClickOnOurCollider()
    {
        Camera cam = clickCamera != null ? clickCamera : Camera.main;
        if (cam == null) return false;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 screen = new Vector3(mousePos.x, mousePos.y, Mathf.Abs(cam.transform.position.z - transform.position.z));
        Vector3 world = cam.ScreenToWorldPoint(screen);

        Collider2D hit = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
        if (hit == null) return false;
        if (hit == ownCollider) return true;
        return hit.transform.IsChildOf(transform);
    }

    private bool IsGateAllowingOpen()
    {
        return gate == null || gate.CanOpen();
    }

    private void TryOpenPopup()
    {
        if (popupManager == null)
        {
            Debug.LogWarning($"[PopupOpener] '{name}' has no PopupManager assigned — drag it in the Inspector.");
            return;
        }
        if (popup == null)
        {
            Debug.LogWarning($"[PopupOpener] '{name}' has no Popup assigned — drag the popup GameObject in the Inspector.");
            return;
        }

        popupManager.Open(popup);
    }
}
