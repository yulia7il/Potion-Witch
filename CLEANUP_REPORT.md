# Potion Witch — Cleanup Audit Report
Date: 2026-05-22
Branch: main-witch-craft
Status: **Read-only audit. Nothing has been deleted or moved.**

---

## 0. Headline findings (read this first)

1. **The Unity Build Settings point at the wrong scene.**
   `ProjectSettings/EditorBuildSettings.asset` lists `Assets/Scenes/WitchPotion.unity` as the only enabled build scene, but the **actual MVP scene is `Assets/PotionWitch/Sample 2D Scene/Gurden.unity`** (the file you've been editing on this branch). `WitchPotion.unity` has zero references to your core MVP scripts. This needs to be fixed regardless of cleanup — your build is currently shipping the wrong scene.
2. **Adventure Creator is barely used.** It occupies ~123 MB and 673 scripts. The only AC component referenced by the live scene is `AC.MainCamera` on the Main Camera GameObject. No `PotionWitch/Scripts/*.cs` file `using AC;` or references the `AC.` namespace. AC can almost certainly be removed once the Main Camera is swapped — but I am not deleting it without your sign-off.
3. **The PotionWitch folder contains a duplicated set of AC manager assets** (`ManagerPackage.asset` + `PotionWitch_ManagerPackage.asset`, same for ActionsManager / CursorManager / InventoryManager / MenuManager / SceneManager / SettingsManager / SpeechManager / VariablesManager). Only one of each set can be the "live" one. Same applies to the `ActionLists` and `ActionLists 1` (empty) folders, and the many `*_PauseGame 1.asset` style " 1" copies inside `PotionWitch/UI/ActionLists`.
4. **Two leftover/test scenes** are still in the project: `Assets/Scenes/WitchPotion.unity` and `Assets/PotionWitch/Sample 2D Scene/SampleScene.unity`. Neither contains any of your core MVP scripts.

---

## 1. Folder size overview

| Folder | Size | Notes |
|---|---|---|
| `Assets/AdventureCreator` | **123 MB** (673 .cs) | Huge plugin; one component used by live scene |
| `Assets/PotionWitch` | 3.3 MB | Your MVP folder |
| `Assets/Settings` | 3.8 MB | URP defaults + a template scene |
| `Assets/TextMesh Pro` | 4.1 MB | Used (TMP fonts/resources). Keep. |
| `Assets/Scenes` | 33 KB | Contains an outdated `WitchPotion.unity` |
| `Assets/Sprites` | 12 KB | Single `Square.png`, only used by the outdated scene |

---

## 2. Core MVP — confirmed alive (DO NOT TOUCH)

GUIDs verified inside `Gurden.unity` (the live scene).

| Script | GUID | In Gurden? |
|---|---|---|
| `PlantPot.cs` | cc57fbc7c5554544c99f7c1d80864675 | yes |
| `PlantGrowth.cs` | 854c344f60a0a844b93acb5034e9c1a9 | referenced via Plant_Mint/Sage prefabs |
| `PlantWaterMeterUI.cs` | 8a2c9f1b4e7d4a4e9b1c2d3e4f5a6b7c | yes |
| `SunSpawner.cs` | 33897931a5cba1e4aa225b918476aa18 | via SunJar prefab |
| `SunSlot.cs` | 1b3d5f7a9c2e4b6d8f0a1c3e5d7b9f2a | yes |
| `SunSlotsManager.cs` | 3b8a6ea56e6852945a90c76783848540 | yes |
| `WorldDraggableTool.cs` | e6fca8140a8284b46aeb4370bfd4a89d | yes |
| `PopupManager.cs` | 9ec30acfca39be742bec5ab508dc4fe9 | yes |
| `PopupOpener.cs` | 2d99ee3c8fac87a4a8e0caccc094a225 | yes |
| `PopupCloser.cs` | 501fb2f946b86014d8939d3970fe0142 | yes (via INV prefabs) |
| `IPopupGate.cs` | e304163ff7124dd408a0f631d82dbdf3 | interface, used by PopupManager |
| `PlantData` (defined in `SeedItem.cs`) | n/a | yes — referenced by `Plant_Mint.asset` and `Plant_Sage.asset` |

Supporting scripts in `PotionWitch/Scripts/` — **also alive, all referenced in scene / prefabs**:

| Script | GUID | Where referenced |
|---|---|---|
| `HarvestCollectButton.cs` | 4cd3678fe2f196743ae942a0f3d3ad10 | Gurden + INV_HarvestPopup prefab |
| `SeedItem.cs` (the file holds the `PlantData` ScriptableObject) | 6eac242dd4f8cb7409617705769f3a54 | Plant_Mint/Sage assets |
| `UIDragSeed.cs` | 54b44fa21dfd1174f8ba193f797cd891 | INV_Select Seed prefab |
| `UIPlantItem.cs` | 5e86ea34b7aba214cb881a3e925ff2dd | INV_Select Seed prefab |
| `WaterParticleCollision.cs` | bd331ea9328d04e40967286375c49de2 | PS_Water prefab |

Note: there is **no class named `SeedItem`** anywhere in the codebase. The file `SeedItem.cs` only contains the `PlantItemType` enum and the `PlantData` ScriptableObject. Consider renaming the file to `PlantData.cs` later — but that is a tidy-up, not a cleanup, so I left it alone.

---

## 3. Files / folders flagged for cleanup

### 3.1 Outdated scenes

| Item | Why it looks unused | References found | Risk | Recommendation |
|---|---|---|---|---|
| `Assets/Scenes/WitchPotion.unity` | 1050-line scene with no core MVP scripts inside (0 matches). Only in EditorBuildSettings — which is itself outdated, since you've been working in Gurden. | EditorBuildSettings.asset references it; Square.png referenced by it. | **Medium** | Update EditorBuildSettings to point at `Assets/PotionWitch/Sample 2D Scene/Gurden.unity`, then **move WitchPotion.unity to `_Archive_ToReview/`**. |
| `Assets/PotionWitch/Sample 2D Scene/SampleScene.unity` | Default Unity sample scene; 0 references to MVP scripts; not in build settings. | None outside itself. | **Low** | Move to `_Archive_ToReview/`. |
| `Assets/Sprites/Square.png` | Only referenced from `WitchPotion.unity`. | WitchPotion.unity only. | **Low** | Move to `_Archive_ToReview/` together with WitchPotion.unity. |

### 3.2 Duplicate AC manager assets in `Assets/PotionWitch/`

The folder has **two parallel sets**:

| Probably canonical (the one in active use) | Probable duplicate |
|---|---|
| `PotionWitch_ManagerPackage.asset` | `ManagerPackage.asset` |
| `PotionWitch_ActionsManager.asset` | `ActionsManager.asset` |
| `PotionWitch_CursorManager.asset` | `CursorManager.asset` |
| `PotionWitch_InventoryManager.asset` | `InventoryManager.asset` |
| `PotionWitch_MenuManager.asset` | `MenuManager.asset` |
| `PotionWitch_SceneManager.asset` | `SceneManager.asset` |
| `PotionWitch_SettingsManager.asset` | `SettingsManager.asset` |
| `PotionWitch_SpeechManager.asset` | `SpeechManager.asset` |
| `PotionWitch_VariablesManager.asset` | `VariablesManager.asset` |

Risk: **High** until you confirm which set the AC Editor window points at (`Adventure Creator → Editor`). Either set could be the wrong one to keep. Recommendation: **Do nothing yet** — open the AC editor and tell me which `ManagerPackage` is "Loaded". Once known, the other set goes to `_Archive_ToReview/AC_ManagerDuplicates/`.

### 3.3 Duplicate UI prefabs (PotionWitch/UI vs AdventureCreator/UI)

`Assets/PotionWitch/UI/` contains 16 prefabs whose filenames match the AC originals (`ContainerUI.prefab`, `ConversationUI.prefab`, `CraftingUI.prefab`, `CursorUI.prefab`, `DocumentUI.prefab`, `HotspotUI.prefab`, `InGameUI.prefab`, `InteractionUI.prefab`, `InventoryUI.prefab`, `LoadUI.prefab`, `ObjectivesUI.prefab`, `OptionsUI.prefab`, `PauseUI.prefab`, `ProfilesUI.prefab`, `SaveUI.prefab`, `SubtitlesUI.prefab`). `diff` confirms the .prefab and .meta files are **different files with different GUIDs** — i.e. these are local clones, not the AC originals being symlinked. None of them are referenced by `Gurden.unity`. They map to AC's menu UI (pause / load / save / objectives / etc.) which the MVP doesn't use.

| Item | Risk | Recommendation |
|---|---|---|
| `Assets/PotionWitch/UI/*.prefab` (16 files) | **Medium** — referenced by the duplicate AC ManagerPackage in §3.2 | Block on §3.2. If the `PotionWitch_*` ManagerPackage is the one that's loaded, then these are its menu prefabs and should stay; if not, move to `_Archive_ToReview/PotionWitch_UI_AC_clones/`. |

### 3.4 AC ActionList asset duplicates inside PotionWitch

`Assets/PotionWitch/UI/ActionLists/` contains 22 `.asset` files, each present **twice** (e.g. `CreateRecipe.asset` and `CreateRecipe 1.asset`). The " 1" suffix is Unity's standard rename-to-resolve-duplicate-import behaviour. These are all generic AC menu-related ActionLists (CreateRecipe, ClearRecipe, QuitButton, SetupPauseMenu, SetupProfilesMenu, ShowSelectedObjective, etc.) — **none** map to MVP gameplay (plant / pot / sun / harvest).

| Item | Risk | Recommendation |
|---|---|---|
| `Assets/PotionWitch/UI/ActionLists/*` (22+22 duplicates) | **Medium** — same dependency chain as §3.3 | Block on §3.2/§3.3 decision. Assuming PotionWitch is supposed to be MVP-only, move the whole folder to `_Archive_ToReview/`. At minimum, the " 1"-suffixed half is safe to archive on its own — they are pure duplicates. |
| `Assets/PotionWitch/UI/ActionLists 1/` | **Low** — folder is empty | Delete (no contents, no GUIDs at risk). |

### 3.5 Unused / oversized plugin folders

| Item | Why it looks unused | References | Risk | Recommendation |
|---|---|---|---|---|
| `Assets/AdventureCreator/2D Demo/` | AC's 2D adventure sample (scenes, NPCs, art) | none from `Assets/PotionWitch/**` | **Medium** — purely an example pack, but AC's Editor may load demo managers as defaults | Move whole folder to `_Archive_ToReview/AdventureCreator_Demos/` |
| `Assets/AdventureCreator/Demo/` | AC's 3D adventure sample | none from `Assets/PotionWitch/**` | **Medium** — same caveat | Move to `_Archive_ToReview/AdventureCreator_Demos/` |
| `Assets/AdventureCreator/Scripts/Templates/` (TitleScreen, SampleScene2D, SampleScene3D, GraphicOptions, InputSystem, AnimatedCursor) | Project-template sample scenes shipped with AC | none from `Assets/PotionWitch/**` | **Medium** — some AC editor windows offer to install templates from here | Manual confirmation needed before archiving |
| `Assets/AdventureCreator/Manual.pdf` (and `changelog.txt`) | Documentation files | none | **Low** | Optional: archive. They don't affect builds. |
| `Assets/Settings/Scenes/URP2DSceneTemplate.unity` | URP default 2D scene template | none | **Low** | Keep. Used by Unity's "New Scene" dialog. (Not a real cleanup target.) |

### 3.6 Adventure Creator as a whole — classification you asked for

| Bucket | Items |
|---|---|
| 1. Definitely used | `AC.MainCamera` script on `Gurden.unity`'s Main Camera GameObject (one component reference, GUID `a79441f348de89743a2939f4d699eac1`). |
| 2. Probably unused | The whole rest of `Assets/AdventureCreator/Scripts/` (672 of 673 scripts). No PotionWitch script imports `AC` namespace; no other AC component is on a live GameObject in `Gurden.unity`. |
| 3. Needs manual confirmation | `Assets/AdventureCreator/Resources/PersistentEngine.prefab`, `RuntimeActionList.prefab`, `BackgroundImageUI.prefab`, `References.asset`. AC's runtime auto-instantiates from `Resources` even if you don't reference it explicitly. **Removing AC requires removing the AC.MainCamera component from Gurden first.** Confirm whether you intend to keep AC at all before touching this. |
| 4. Safe to remove only after backup | The 2D Demo, 3D Demo, and `Scripts/Templates/*` folders — they don't impact runtime but AC's editor may complain on uninstall. |

**My recommendation for AC, plain language:** Decide now whether you want AC in this project at all. The MVP doesn't use it for anything except a Main Camera. If you choose to drop AC, the steps are: (a) replace `AC.MainCamera` on Gurden's Main Camera with a plain Unity Camera + a small fade script, (b) delete the duplicate `PotionWitch_*Manager` assets, (c) delete `Assets/AdventureCreator/`. That reclaims ~123 MB and ~670 scripts. **I will not do this until you say so.**

---

## 4. Things I checked and confirmed CLEAN (don't touch)

- `Assets/PotionWitch/Scripts/` — every `.cs` is referenced by Gurden, a prefab, or a ScriptableObject.
- `Assets/PotionWitch/Graphics/Pilot-Assets/` — sprites are referenced by prefabs or scene.
- `Assets/PotionWitch/Graphics/Prefabs/` — Plant_Mint, Plant_Sage, WaterMeter, SunJar/*, INV/* are all referenced (77 occurrences across the listed GUIDs in Gurden).
- `Assets/PotionWitch/Graphics/VFX/PS_Water.prefab` — used.
- `Assets/PotionWitch/Graphics/Managers/Seeds/Plant_Mint.asset` & `Plant_Sage.asset` — used by SeedItem/UIPlantItem.
- `Assets/PotionWitch/Input System/` — `Controls.inputactions` is wired to EventSystem in Gurden.
- `Assets/TextMesh Pro/` — required for TMP text in popups.
- `Assets/Settings/` (URP assets) — required by the render pipeline.

---

## 5. Proposed `_Archive_ToReview/` structure

```
_Archive_ToReview/
├── README.md                          (a note explaining why each item was moved)
├── OldScenes/
│   ├── WitchPotion.unity              (+ .meta)
│   ├── SampleScene.unity              (+ .meta)
│   └── Square.png                     (+ .meta)   ← only used by WitchPotion
├── PotionWitch_UI_AC_clones/          (only if §3.2 confirms the non-prefixed set is canonical)
│   └── (16 prefabs + ActionLists folder)
├── PotionWitch_AC_Manager_duplicates/ (the loser of §3.2)
│   └── ManagerPackage.asset + 9 *Manager assets
└── AdventureCreator_Demos/
    ├── 2D Demo/
    ├── Demo/
    └── Templates/
```

---

## 6. Hard blockers before any move

1. **You must fix EditorBuildSettings first.** Open Unity → File → Build Settings → drag in `Assets/PotionWitch/Sample 2D Scene/Gurden.unity`, remove `Assets/Scenes/WitchPotion.unity`. Otherwise archiving WitchPotion will break the build.
2. **You must tell me which AC ManagerPackage is "Loaded"** in the AC editor window before I touch any `*Manager.asset` in `PotionWitch/`. Picking the wrong one will break the AC editor's understanding of your project.
3. **Decide AC's fate.** If AC stays, the items in §3.2/§3.3/§3.4 might genuinely belong to it. If AC goes, almost all of `AdventureCreator/` + the duplicate manager set goes with it.

---

## 7. Suggested order of operations (after your approval)

1. Create `_Archive_ToReview/` folder + README.
2. Fix `EditorBuildSettings.asset` to point at Gurden.
3. Move `WitchPotion.unity`, `SampleScene.unity`, `Sprites/Square.png` → `_Archive_ToReview/OldScenes/`. **Low risk** — none are referenced by the MVP.
4. Delete empty `Assets/PotionWitch/UI/ActionLists 1/` folder. **Low risk.**
5. After you answer §6.2: archive the loser ManagerPackage set.
6. After you answer §6.3: either fully remove AC, or just archive the demos (§3.5).
7. Re-open Unity, let it reimport, verify Gurden still plays through the full loop (Seed → Plant → Water → Sun → Harvest).

---

## 8. Awaiting your decisions

Please answer these three before I make any change:

- **Q1:** OK to fix EditorBuildSettings to point at `Gurden.unity` and archive `WitchPotion.unity` + `SampleScene.unity` + `Sprites/Square.png`?
- **Q2:** In Unity's AC editor window, which `ManagerPackage` is currently loaded — the one named `ManagerPackage` or the one named `PotionWitch_ManagerPackage`?
- **Q3:** Adventure Creator — keep it (because you plan to use it later), or drop it entirely? Or "keep but archive the demos / templates only"?

I will not move or delete anything until you reply.
