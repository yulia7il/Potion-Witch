using UnityEditor;
using UnityEngine;

// Custom Inspector for InventoryDebugCheats.
//
// Draws the normal serialized fields first, then adds clearly-labeled buttons
// underneath so cheats are one click away instead of buried in the three-dot
// context menu.
//
// Lives in an Editor/ folder so Unity compiles it into the editor-only
// assembly — it won't ship in builds.
[CustomEditor(typeof(InventoryDebugCheats))]
public class InventoryDebugCheatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the standard fields (mintItem, sageItem, inventoryManager,
        // cauldronInventoryBarUI) exactly as Unity normally would.
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Cheats", EditorStyles.boldLabel);

        InventoryDebugCheats cheats = (InventoryDebugCheats)target;

        if (GUILayout.Button("Add Mint"))
        {
            if (EnsurePlayMode()) cheats.AddMint();
        }

        if (GUILayout.Button("Add Sage"))
        {
            if (EnsurePlayMode()) cheats.AddSage();
        }

        if (GUILayout.Button("Clear Inventory"))
        {
            if (EnsurePlayMode()) cheats.ClearInventory();
        }

        if (GUILayout.Button("Refresh Cauldron UI"))
        {
            if (EnsurePlayMode()) cheats.RefreshCauldronUI();
        }
    }

    // Returns true if the editor is currently in Play Mode. Logs a friendly
    // warning to the Console otherwise so it's obvious why nothing happened.
    private static bool EnsurePlayMode()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("Inventory debug cheats only work in Play Mode.");
            return false;
        }
        return true;
    }
}
