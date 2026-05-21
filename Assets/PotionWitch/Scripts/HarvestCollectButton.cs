using UnityEngine;
using UnityEngine.UI;

// Attach to the Collect button inside the harvest popup. On click it asks
// the linked PlantPot to harvest itself. Closing the popup is handled by
// the PopupCloser component on the same button — this script only owns
// the gameplay-reset side of the click.
//
// MVP wiring: a single pot links directly to its Collect button in the
// Inspector. When multiple pots exist, replace this direct ref with a
// per-pot binding driven by whichever PopupOpener opened the harvest popup.
[RequireComponent(typeof(Button))]
public class HarvestCollectButton : MonoBehaviour
{
    [Tooltip("The PlantPot to harvest when this button is clicked.")]
    [SerializeField] private PlantPot plantPot;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnCollectClicked);
    }

    private void OnCollectClicked()
    {
        if (plantPot == null)
        {
            Debug.LogWarning($"[HarvestCollectButton] '{name}' has no PlantPot assigned — drag it in the Inspector.");
            return;
        }
        plantPot.Harvest();
    }
}
