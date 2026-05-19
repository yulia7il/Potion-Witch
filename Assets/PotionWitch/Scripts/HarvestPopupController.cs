using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pure UI controller for the harvest reward popup. The dark overlay is a
// scene-wide singleton shared with future popups, so this script just toggles
// references it was wired to in the Inspector. Anything game-side (deciding
// *when* to harvest, awarding the leaf, etc.) lives elsewhere — PlantPot
// decides when to call Show().
public class HarvestPopupController : MonoBehaviour
{
    [Tooltip("Root of the popup panel (INV_HarvestPopup). Toggled active/inactive.")]
    [SerializeField] private GameObject popupRoot;

    [Tooltip("Shared scene-wide dim layer (Overlay_Darken). Same instance is used by all popups.")]
    [SerializeField] private GameObject overlayDarken;

    [Tooltip("Image inside the popup that shows the rewarded item's icon.")]
    [SerializeField] private Image rewardIcon;

    [Tooltip("TMP label showing the reward amount (e.g. 'X1').")]
    [SerializeField] private TMP_Text rewardAmountText;

    [Tooltip("Confirm button — closes the popup and the overlay.")]
    [SerializeField] private Button okButton;

    private void Awake()
    {
        Hide();
        if (okButton != null) okButton.onClick.AddListener(Hide);
    }

    // Called by PlantPot when a fully grown plant is clicked. Drives the UI
    // only; awarding the leaf to inventory is intentionally not handled here.
    public void Show(PlantData plantData, int amount)
    {
        if (plantData != null && rewardIcon != null) rewardIcon.sprite = plantData.leafIcon;
        if (rewardAmountText != null) rewardAmountText.text = "X" + amount;

        if (popupRoot != null) popupRoot.SetActive(true);
        if (overlayDarken != null) overlayDarken.SetActive(true);
    }

    public void Hide()
    {
        if (popupRoot != null) popupRoot.SetActive(false);
        if (overlayDarken != null) overlayDarken.SetActive(false);
    }
}
