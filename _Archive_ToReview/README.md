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

## Phase 2 — 2026-05-22 — Adventure Creator removal

Done after the user manually removed the `AC.MainCamera` MonoBehaviour from
Gurden's Main Camera and cleared the UnityEvent that referenced
`AC.Templates.InputSystemIntegration.InputRemapper`. A re-scan of `Gurden.unity`
returned zero `m_EditorClassIdentifier: AC::` and zero references to the
`AC.MainCamera` script GUID `9fd2648e8ea494b7aaeaaa1ea71c5d3c`. With the live
scene clean, the rest of Adventure Creator was archived in one pass.

### PotionWitch_AC_residue/

| Restored path | Why archived |
|---|---|
| `Assets/PotionWitch/ManagerPackage.asset` (+ `.meta`) | AC `ManagerPackage` ScriptableObject. Not referenced by Gurden. Pure data — required only if AC is reinstalled. |
| `Assets/PotionWitch/PotionWitch_ManagerPackage.asset` (+ `.meta`) | Duplicate of the above with project-prefixed name. Same reason. |
| `Assets/PotionWitch/Managers/` (whole folder, both duplicate sets) | 18 AC manager ScriptableObjects (ActionsManager, CursorManager, InventoryManager, MenuManager, SceneManager, SettingsManager, SpeechManager, VariablesManager, plus `PotionWitch_*` clones). All depend on AC scripts. |
| `Assets/PotionWitch/UI/ActionLists/` | 22 AC `ActionListAsset` scriptable objects (`CreateRecipe`, `QuitButton`, `SetupPauseMenu`, etc.), each duplicated with a ` 1` suffix → 44 files total. None relate to plant / pot / sun / harvest gameplay. |
| `Assets/PotionWitch/UI/ActionLists 1/` | Empty folder left from a previous duplicate-rename event. |
| `Assets/PotionWitch/UI/*.prefab` (16 prefabs + metas) | Local clones of AC menu prefabs (`ContainerUI`, `ConversationUI`, `CraftingUI`, `CursorUI`, `DocumentUI`, `HotspotUI`, `InGameUI`, `InteractionUI`, `InventoryUI`, `LoadUI`, `ObjectivesUI`, `OptionsUI`, `PauseUI`, `ProfilesUI`, `SaveUI`, `SubtitlesUI`). Different GUIDs from the AC originals — these were project copies. None referenced by Gurden. |
| `Assets/PotionWitch/Input System/InputsUI.prefab` (+ `.meta`) | AC InputSystem-template input-remap UI prefab. Only consumer was `Assets/PotionWitch/Managers/MenuManager.asset` (archived in the same pass). |

### AdventureCreator/

| Restored path | Why archived |
|---|---|
| `Assets/AdventureCreator/` (+ `.meta`) | Whole plugin — 123 MB, ~673 `.cs` files, including the `AC` assembly definition. Zero PotionWitch script files import `AC`. After the user removed the two live AC references in Gurden, no remaining asset under `Assets/` referenced AC. Restoring this folder is enough to reinstall AC — its `AC.asmdef` and managers will be detected automatically by Unity on reimport. |

### Empty folders deleted after Phase 2

These had no remaining content after the moves above:

- `Assets/Scenes/` (+ `.meta`) — only held `WitchPotion.unity`, archived in Phase 1.
- `Assets/Sprites/` (+ `.meta`) — only held `Square.png`, archived in Phase 1.
- `Assets/PotionWitch/UI/` (+ `.meta`) — only held the AC menu clones and ActionList folders archived in Phase 2.
