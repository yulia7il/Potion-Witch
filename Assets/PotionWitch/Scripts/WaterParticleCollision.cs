using UnityEngine;

// Bridges the water-can ParticleSystem to the PlantPot's Water() call.
// Sits on the same GameObject as the ParticleSystem so Unity routes
// OnParticleCollision messages here.
//
// Required ParticleSystem settings on this object:
//   - Collision module: enabled
//   - Type: World
//   - Send Collision Messages: enabled
//   - Collides With: include the layer the PlantPot collider lives on
[RequireComponent(typeof(ParticleSystem))]
public class WaterParticleCollision : MonoBehaviour
{
    [Tooltip("Minimum seconds between consecutive AddWaterProgress calls on the same pot. " +
             "Prevents one stream of particles from advancing the meter every frame.")]
    [SerializeField] private float waterCooldown = 0.5f;

    [Tooltip("Normalized meter increment applied per accepted hit (1 = full meter).")]
    [SerializeField] private float waterAmountPerHit = 0.1f;

    // Tracks the last time we advanced watering on a given pot, so a continuous
    // particle stream doesn't spam progress every frame.
    private PlantPot lastPot;
    private float nextWaterTime;

    private void OnParticleCollision(GameObject other)
    {
        PlantPot pot = other.GetComponentInParent<PlantPot>();
        if (pot == null) return;

        // Reset cooldown if we hit a different pot than last time.
        if (pot != lastPot)
        {
            lastPot = pot;
            nextWaterTime = 0f;
        }

        if (Time.time < nextWaterTime) return;

        nextWaterTime = Time.time + waterCooldown;
        Debug.Log($"[WaterParticles] Adding {waterAmountPerHit:0.##} progress to '{pot.name}'");
        pot.AddWaterProgress(waterAmountPerHit);
    }
}
