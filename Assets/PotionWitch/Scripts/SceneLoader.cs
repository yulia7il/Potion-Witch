using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Attach to any GameObject (e.g. a UI Button). Pick the target scene in the
// Inspector via the SceneAsset field — no need to type scene names. The asset
// reference is Editor-only; the scene name is serialized for runtime builds.
public class SceneLoader : MonoBehaviour
{
#if UNITY_EDITOR
    [Tooltip("Drag the scene asset to load here. Must also be added to Build Settings.")]
    [SerializeField] private SceneAsset sceneAsset;
#endif

    [SerializeField, HideInInspector] private string sceneName;

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[SceneLoader] '{name}' has no scene assigned — drag a SceneAsset in the Inspector.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        sceneName = sceneAsset != null ? sceneAsset.name : string.Empty;
    }
#endif
}
