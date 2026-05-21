using UnityEngine;

// Scene-wide popup coordinator. Owns the single shared dark overlay and
// tracks which popup is currently open so gameplay scripts don't each
// duplicate that bookkeeping. Stays generic — doesn't care what a popup
// contains.
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
        popup.SetActive(true);
        currentlyOpenPopup = popup;
    }

    public void Close()
    {
        if (currentlyOpenPopup != null) currentlyOpenPopup.SetActive(false);
        HideOverlay();
        currentlyOpenPopup = null;
    }

    public void Close(GameObject popup)
    {
        if (popup == null) return;

        popup.SetActive(false);

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
            currentlyOpenPopup.SetActive(false);
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
