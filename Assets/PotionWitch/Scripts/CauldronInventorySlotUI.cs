using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One visual cell in the cauldron inventory bar. Knows nothing about the
// inventory itself — it just shows whatever stack the bar hands it, or
// blanks out when told to. The bar (CauldronInventoryBarUI) decides which
// stack goes where.
public class CauldronInventorySlotUI : MonoBehaviour
{
    [Tooltip("Image that shows the item's icon sprite.")]
    [SerializeField] private Image iconImage;

    [Tooltip("Text that shows the stack amount, e.g. \"3\".")]
    [SerializeField] private TMP_Text amountText;

    [Tooltip("Optional placeholder shown only when the slot is empty (e.g. a faded frame). Can be left unassigned.")]
    [SerializeField] private GameObject emptyVisual;

    // Fill the slot from an inventory stack. Caller guarantees stack != null.
    public void Setup(InventoryStack stack)
    {
        if (iconImage != null)
        {
            iconImage.sprite = stack.item != null ? stack.item.icon : null;
            iconImage.enabled = iconImage.sprite != null;
        }

        if (amountText != null)
        {
            amountText.text = stack.amount.ToString();
            amountText.enabled = true;
        }

        if (emptyVisual != null) emptyVisual.SetActive(false);
    }

    // Blank the slot — used for slots past the end of the inventory.
    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (amountText != null)
        {
            amountText.text = string.Empty;
            amountText.enabled = false;
        }

        if (emptyVisual != null) emptyVisual.SetActive(true);
    }
}
