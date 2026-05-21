using UnityEngine;
using UnityEngine.UI;

// Reusable close helper. Attach to a UI Button (OK / X) — it wires itself
// to the button's onClick in Awake. Also exposes Close() so anything else
// (UnityEvent in the Inspector, a script) can call it too.
//
// If 'specificPopup' is empty, this closes whichever popup is currently open.
// If set, it closes only that specific popup.
public class PopupCloser : MonoBehaviour
{
    [Tooltip("Scene PopupManager that handles the shared overlay and currently-open popup.")]
    [SerializeField] private PopupManager popupManager;

    [Tooltip("Optional. If set, this closer always closes this specific popup. If empty, it closes whichever popup is currently open.")]
    [SerializeField] private GameObject specificPopup;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(Close);
    }

    public void Close()
    {
        if (popupManager == null)
        {
            Debug.LogWarning($"[PopupCloser] '{name}' has no PopupManager assigned — drag it in the Inspector.");
            return;
        }

        if (specificPopup != null) popupManager.Close(specificPopup);
        else popupManager.Close();
    }
}
