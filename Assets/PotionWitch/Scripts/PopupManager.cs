using UnityEngine;

// Scene-wide popup coordinator. Owns the single shared dark overlay and
// tracks which popup is currently open so gameplay scripts don't each
// duplicate that bookkeeping. Stays generic — doesn't care what a popup
// contains.
//
// Hide strategy:
//   - If the popup has a CanvasGroup, hide via alpha=0 + raycasts/interactable off,
//     so children stay active (e.g. an in-progress UI drag survives).
//   - Otherwise, SetActive(false) on the popup root.
//
// Public API:
//   Open(popup)         — show this popup (closes any other open one first)
//   Close()             — close whichever popup is currently open
//   Close(popup)        — close this specific popup (only clears overlay if it was the current one)
public class PopupManager : MonoBehaviour
{
    [Tooltip("Shared scene-wide dim layer (Overlay_Darken). Same instance is reused by every popup.")]
    [SerializeField] private GameObject overlayDarken;

    [Tooltip("On Awake, hide every direct child except overlayDarken. Lets popups live as children and start closed without extra wiring.")]
    [SerializeField] private bool hideChildPopupsOnAwake = true;

    private GameObject currentlyOpenPopup;

    public GameObject CurrentlyOpenPopup => currentlyOpenPopup;
    public bool IsAnyPopupOpen => currentlyOpenPopup != null;

    private void Awake()
    {
        HideOverlay();
        if (hideChildPopupsOnAwake) HideAllChildPopups();
    }

    public void Open(GameObject popup)
    {
        if (popup == null)
        {
            Debug.LogWarning("[PopupManager] Open called with NULL popup — nothing to show.");
            return;
        }

        CloseCurrentIfDifferent(popup);
        ShowOverlay();
        ShowPopup(popup);
        currentlyOpenPopup = popup;
    }

    public void Close()
    {
        if (currentlyOpenPopup != null) HidePopup(currentlyOpenPopup);
        HideOverlay();
        currentlyOpenPopup = null;
    }

    public void Close(GameObject popup)
    {
        if (popup == null) return;

        HidePopup(popup);

        if (popup == currentlyOpenPopup)
        {
            HideOverlay();
            currentlyOpenPopup = null;
        }
    }

    // ---------- Helpers ----------

    private void CloseCurrentIfDifferent(GameObject incoming)
    {
        if (currentlyOpenPopup != null && currentlyOpenPopup != incoming)
        {
            HidePopup(currentlyOpenPopup);
        }
    }

    private void ShowPopup(GameObject popup)
    {
        popup.SetActive(true);
        CanvasGroup cg = popup.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
    }

    private void HidePopup(GameObject popup)
    {
        CanvasGroup cg = popup.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
        else
        {
            popup.SetActive(false);
        }
    }

    private void ShowOverlay()
    {
        if (overlayDarken != null) overlayDarken.SetActive(true);
    }

    private void HideOverlay()
    {
        if (overlayDarken != null) overlayDarken.SetActive(false);
    }

    private void HideAllChildPopups()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child == overlayDarken) continue;
            child.SetActive(false);
        }
    }
}
