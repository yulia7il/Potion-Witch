# _Archive_ToReview

Files moved here are **not deleted**, just relocated outside the `Assets/` tree
so Unity stops importing them. To restore any file, move both the `*` and the
matching `*.meta` back to their original path — the GUID inside the `.meta`
will rebind every existing reference.

## Phase 1 — 2026-05-22

### OldScenes/

| Restored path (if needed) | Why archived |
|---|---|
| `Assets/Scenes/WitchPotion.unity` (+ `.meta`) | Outdated MVP scene. Was the only entry in `EditorBuildSettings.asset`, but the live scene is `Assets/PotionWitch/Sample 2D Scene/Gurden.unity`. Build Settings updated to Gurden in the same Phase 1 pass. No prefab, script, or scriptable-object referenced this scene. |
| `Assets/PotionWitch/Sample 2D Scene/SampleScene.unity` (+ `.meta`) | Default Unity sample scene. Zero references anywhere in the project. Was not in Build Settings. |
| `Assets/Sprites/Square.png` (+ `.meta`) | Only consumer was `WitchPotion.unity` (archived in this same pass). No other prefab, scene, scriptable-object, or material referenced it. |
