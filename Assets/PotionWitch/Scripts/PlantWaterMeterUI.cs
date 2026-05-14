using UnityEngine;

// Lives on the WaterMeter UI root under the Canvas. For now it just toggles
// visibility; fill / shader logic will be added in later steps.
public class PlantWaterMeterUI : MonoBehaviour
{
    public void Show()
    {
        Debug.Log($"[WaterMeter] Show() called on '{name}'. activeSelf before = {gameObject.activeSelf}.");
        gameObject.SetActive(true);
        Debug.Log($"[WaterMeter] Show() on '{name}'. activeSelf after = {gameObject.activeSelf}.");
    }

    public void Hide()
    {
        Debug.Log($"[WaterMeter] Hide() called on '{name}'. activeSelf before = {gameObject.activeSelf}.");
        gameObject.SetActive(false);
        Debug.Log($"[WaterMeter] Hide() on '{name}'. activeSelf after = {gameObject.activeSelf}.");
    }
}
