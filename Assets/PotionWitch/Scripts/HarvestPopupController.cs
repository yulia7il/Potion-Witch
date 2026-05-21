using UnityEngine;
using UnityEngine.UI;

// Thin controller for the harvest popup. Open/close and overlay handling
// are delegated to PopupManager so this stays focused on the panel itself.
public class HarvestPopupController : MonoBehaviour
{
    [Tooltip("Root of the popup panel (INV_HarvestPopup). Toggled active/inactive.")]
    [SerializeField] private GameObject popupRoot;

    [Tooltip("Confirm button — closes the popup via PopupManager.")]
    [SerializeField] private Button okButton;

    [Tooltip("Scene PopupManager that owns the shared dark overlay.")]
    [SerializeField] private PopupManager popupManager;

    private void Awake()
    {
        if (popupRoot != null) popupRoot.SetActive(false);
        if (okButton != null) okButton.onClick.AddListener(() => popupManager.HideCurrentPopup());
    }

    public void Show()
    {
        if (popupManager == null) return;
        popupManager.ShowPopup(popupRoot);
    }
}
