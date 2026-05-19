using UnityEngine;

// Lives on the WaterMeter UI root under the Canvas. Drives the meter by
// growing Tooltip_Fill's height (sizeDelta.y) inside the Tooltip mask. The
// fill animates smoothly toward a target so a burst of progress doesn't
// snap the bar.
public class PlantWaterMeterUI : MonoBehaviour
{
    [Tooltip("RectTransform of Tooltip_Fill — the sprite whose height grows inside the mask.")]
    [SerializeField] private RectTransform fillVisual;

    [Tooltip("Height of fillVisual when the meter is full (sizeDelta.y at targetFill == 1).")]
    [SerializeField] private float maxFillHeight = 100f;

    [Tooltip("How fast the visible bar chases the target, in height units per second. " +
             "Linear (Mathf.MoveTowards) — the bar moves at constant speed and stops on arrival.")]
    [SerializeField] private float fillAnimationSpeed = 5f;

    // 0..1 progress the meter is animating *toward*. AddFill/SetFill nudge this;
    // Update() walks the visible bar to match. Decoupling target from visual lets
    // a single big AddFill animate in over multiple frames instead of snapping.
    private float targetFill;

    // Current animated sizeDelta.y. Kept as a float so MoveTowards advances by
    // exact sub-pixel steps each frame.
    private float currentVisualHeight;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (fillVisual == null) return;

        float targetHeight = maxFillHeight * targetFill;
        currentVisualHeight = Mathf.MoveTowards(currentVisualHeight, targetHeight, fillAnimationSpeed * Time.deltaTime);
        ApplyHeight(currentVisualHeight);
    }

    // Updates the target the meter is animating toward. Does NOT snap the
    // visible bar — Update() handles the easing. Resets/initial states that
    // need an instant change should call ResetMeter() instead.
    public void SetFill(float normalized)
    {
        targetFill = Mathf.Clamp01(normalized);
    }

    // Adds to the target fill and reports whether the meter has now reached full.
    // The visible bar may still be catching up when this returns true.
    public bool AddFill(float amount)
    {
        targetFill = Mathf.Clamp01(targetFill + amount);
        return targetFill >= 1f;
    }

    // Snaps both the target and the visible bar back to empty, and re-shows
    // the meter. Used when a fresh plant is placed in the pot.
    public void ResetMeter()
    {
        targetFill = 0f;
        currentVisualHeight = 0f;
        ApplyHeight(0f);
        Show();
    }

    // Writes the height to fillVisual, preserving its current width.
    private void ApplyHeight(float height)
    {
        Vector2 size = fillVisual.sizeDelta;
        size.y = height;
        fillVisual.sizeDelta = size;
    }
}
