using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach this together with UIPlantItem on each seed UI item in the inventory.
// Drag spawns a ghost icon that follows the cursor; on release we look for a
// PlantPot at the world point under the cursor and ask it to plant the seed.
[RequireComponent(typeof(UIPlantItem))]
public class UIDragSeed : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Optional. CanvasGroup of the inventory panel. " +
             "Hidden visually on drag start (alpha=0, raycasts off, non-interactable) " +
             "instead of using SetActive(false) which would kill the drag flow. " +
             "Restored on drag end if planting failed.")]
    public CanvasGroup inventoryCanvasGroup;

    [Tooltip("Optional. UI prefab used as the drag ghost. Must contain an Image " +
             "on its root or in a child. If null, a basic Image is built in code.")]
    public GameObject dragVisualPrefab;

    private UIPlantItem item;
    private Image sourceImage; // the seed's own UI Image, used to copy sprite + size
    private GameObject ghost;

    // Cached during CreateDragGhost so OnDrag doesn't re-walk the hierarchy each frame.
    // Used to convert eventData.position into local canvas space for the ghost.
    private Canvas rootCanvas;
    private RectTransform rootCanvasRect;

    private void Awake()
    {
        item = GetComponent<UIPlantItem>();

        // Prefer the icon Image that UIPlantItem already references on a child;
        // fall back to local lookups so this still works for flatter prefabs.
        sourceImage = item != null ? item.iconImage : null;
        if (sourceImage == null) sourceImage = GetComponent<Image>();
        if (sourceImage == null) sourceImage = GetComponentInChildren<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null || item.plantData == null) return;

        CreateDragGhost(eventData);
        HideInventoryVisual();
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool planted = TryPlantAtPointer(eventData);
        DestroyGhost();

        // Keep inventory hidden after a successful plant; bring it back on miss.
        if (!planted) RestoreInventoryVisual();
    }

    // ---------- Helpers ----------

    // Spawns the visible follower icon on the root Canvas.
    // Uses dragVisualPrefab if assigned, otherwise builds a basic Image in code.
    private void CreateDragGhost(PointerEventData eventData)
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;
        rootCanvas = parentCanvas.rootCanvas;
        rootCanvasRect = rootCanvas.transform as RectTransform;

        if (dragVisualPrefab != null)
        {
            // worldPositionStays = false so the prefab's local values apply cleanly.
            ghost = Instantiate(dragVisualPrefab, rootCanvas.transform, false);
        }
        else
        {
            ghost = new GameObject("DragGhost",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ghost.transform.SetParent(rootCanvas.transform, false);
        }

        ghost.transform.SetAsLastSibling(); // render above siblings

        RectTransform ghostRect = ghost.GetComponent<RectTransform>();
        if (ghostRect != null)
        {
            ghostRect.localScale = Vector3.one;
            // Anchor the ghost at the canvas's pivot so the local point returned by
            // ScreenPointToLocalPointInRectangle can be assigned straight to anchoredPosition.
            // This works for Screen Space - Camera, Screen Space - Overlay, and World Space.
            ghostRect.anchorMin = rootCanvasRect.pivot;
            ghostRect.anchorMax = rootCanvasRect.pivot;

            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : rootCanvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rootCanvasRect, eventData.position, cam, out Vector2 localPoint))
            {
                ghostRect.anchoredPosition = localPoint;
            }
        }

        // Image may live on the root or on a child of the prefab.
        Image ghostImage = ghost.GetComponentInChildren<Image>();
        if (ghostImage != null) ConfigureGhostImage(ghostImage);
    }

    // Applies sprite, color, raycast, aspect, and size to the ghost's Image.
    // Sprite: source Image first, fallback to PlantData.seedIcon.
    // Size:   source RectTransform first, fallback to 100x100.
    private void ConfigureGhostImage(Image img)
    {
        Sprite sprite = null;
        if (sourceImage != null && sourceImage.sprite != null) sprite = sourceImage.sprite;
        else if (item != null && item.plantData != null) sprite = item.plantData.seedIcon;

        img.sprite = sprite;
        img.color = Color.white;
        img.raycastTarget = false;
        img.preserveAspect = true;

        Vector2 size = new Vector2(100f, 100f);
        if (sourceImage != null)
        {
            Vector2 srcSize = sourceImage.rectTransform.rect.size;
            if (srcSize.x > 0f && srcSize.y > 0f) size = srcSize;
        }
        img.rectTransform.sizeDelta = size;
    }

    // Moves the ghost to follow the cursor.
    // Uses the same canvas-aware conversion as CreateDragGhost so this works
    // under Screen Space - Camera (and stays correct for Overlay / World Space).
    private void UpdateGhostPosition(PointerEventData eventData)
    {
        if (ghost == null || rootCanvasRect == null) return;

        RectTransform ghostRect = ghost.GetComponent<RectTransform>();
        if (ghostRect == null) return;

        Camera cam = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? rootCanvas.worldCamera : null;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvasRect, eventData.position, cam, out Vector2 localPoint))
        {
            ghostRect.anchoredPosition = localPoint;
        }
    }

    // Removes the ghost icon. Safe to call even if no ghost exists.
    private void DestroyGhost()
    {
        if (ghost == null) return;
        Destroy(ghost);
        ghost = null;
    }

    // Looks under the cursor for a PlantPot in 2D world space.
    // Returns true if a plant was actually spawned.
    private bool TryPlantAtPointer(PointerEventData eventData)
    {
        if (item == null || item.plantData == null) return false;
        if (Camera.main == null) return false;

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D hit = Physics2D.OverlapPoint(worldPoint2D);
        if (hit == null) return false;

        // GetComponentInParent so the collider can live on a child of the pot.
        PlantPot pot = hit.GetComponentInParent<PlantPot>();
        if (pot == null) return false;

        return pot.Plant(item.plantData);
    }

    // Hides the inventory without disabling its GameObject (keeps drag alive).
    private void HideInventoryVisual()
    {
        if (inventoryCanvasGroup == null) return;
        inventoryCanvasGroup.alpha = 0f;
        inventoryCanvasGroup.blocksRaycasts = false;
        inventoryCanvasGroup.interactable = false;
    }

    // Restores inventory visibility and interaction.
    private void RestoreInventoryVisual()
    {
        if (inventoryCanvasGroup == null) return;
        inventoryCanvasGroup.alpha = 1f;
        inventoryCanvasGroup.blocksRaycasts = true;
        inventoryCanvasGroup.interactable = true;
    }
}
