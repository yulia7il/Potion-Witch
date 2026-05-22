# Potion Witch — Final Cleanup Report
Date: 2026-05-22
Branch: main-witch-craft
Phases executed: 1 (old scenes) + 2 (Adventure Creator removal)
Result: project is now a clean standalone Potion Witch MVP with **zero Adventure Creator references**.

---

## 1. Outcome at a glance

| Metric | Before | After |
|---|---|---|
| `Assets/` total size | ~134 MB | **9.3 MB** |
| `Assets/**.cs` files | 689 (673 AC + 16 PotionWitch) | **16** (PotionWitch only) |
| `using AC;` occurrences | 8 (all inside `Assets/AdventureCreator/`) | **0** |
| `AC.` namespace use in PotionWitch scripts | 0 | **0** (unchanged) |
| `m_EditorClassIdentifier: AC::` under `Assets/` | many | **0** |
| AC GUID references in `Gurden.unity` | 2 (MainCamera + InputRemapper UnityEvent) | **0** |
| Scenes in Build Settings | `Assets/Scenes/WitchPotion.unity` (wrong) | `Assets/PotionWitch/Sample 2D Scene/Gurden.unity` |
| Empty / leftover folders | `Assets/PotionWitch/UI/ActionLists 1/`, `Assets/Scenes/`, `Assets/Sprites/`, `Assets/PotionWitch/UI/` | All deleted |

Everything moved is preserved under `_Archive_ToReview/` at the project root. Nothing was deleted from disk — restoring any file is `mv` back to its old path. Restoration of the whole AC plugin is `mv _Archive_ToReview/AdventureCreator Assets/`.

---

## 2. Exactly what was moved or removed

### 2.1 Archived to `_Archive_ToReview/OldScenes/` (Phase 1)
- `Assets/Scenes/WitchPotion.unity` (+ `.meta`)
- `Assets/PotionWitch/Sample 2D Scene/SampleScene.unity` (+ `.meta`)
- `Assets/Sprites/Square.png` (+ `.meta`)

### 2.2 Archived to `_Archive_ToReview/PotionWitch_AC_residue/` (Phase 2)
- `Assets/PotionWitch/ManagerPackage.asset` (+ `.meta`)
- `Assets/PotionWitch/PotionWitch_ManagerPackage.asset` (+ `.meta`)
- `Assets/PotionWitch/Managers/` (whole folder + `.meta`) — 18 AC `*Manager.asset` files (both the unsuffixed and `PotionWitch_*` duplicates)
- `Assets/PotionWitch/UI/ActionLists/` (whole folder + `.meta`) — 44 AC `ActionListAsset` files
- `Assets/PotionWitch/UI/ActionLists 1/` (empty folder + `.meta`)
- `Assets/PotionWitch/UI/ContainerUI.prefab` … `SubtitlesUI.prefab` — 16 AC menu prefab clones (each + `.meta`)
- `Assets/PotionWitch/Input System/InputsUI.prefab` (+ `.meta`)

### 2.3 Archived to `_Archive_ToReview/AdventureCreator/` (Phase 2)
- `Assets/AdventureCreator/` (whole folder, ~123 MB, ~673 `.cs`, + `.meta`)

### 2.4 Empty folders deleted after Phase 2
- `Assets/Scenes/` (+ `.meta`)
- `Assets/Sprites/` (+ `.meta`)
- `Assets/PotionWitch/UI/` (+ `.meta`)

---

## 3. Final project structure

```
Assets/
├── DefaultVolumeProfile.asset             URP volume defaults (required by render pipeline)
├── InputSystem_Actions.inputactions       Unity Input System asset, referenced by EventSystem in Gurden
├── UniversalRenderPipelineGlobalSettings.asset   URP global settings
├── PotionWitch/                           Your MVP code + content
│   ├── Graphics/
│   │   ├── Managers/Seeds/                Plant_Mint.asset + Plant_Sage.asset (PlantData ScriptableObjects)
│   │   ├── Pilot-Assets/                  Source sprites (BG, Cauldron, Plant stages, UI icons, Seeds, Text, etc.)
│   │   ├── Prefabs/
│   │   │   ├── INV/                       INV_HarvestPopup.prefab, INV_Select Seed.prefab
│   │   │   ├── SunJar/                    Slot_0, SpawnPoints, SunJar, SunPrefab prefabs
│   │   │   ├── Plant_Mint.prefab, Plant_Sage.prefab, WaterMeter.prefab
│   │   └── VFX/                           PS_Water.prefab (water-can particle system)
│   ├── Input System/                      Controls.inputactions + EventSystem_InputSystem.prefab
│   ├── Sample 2D Scene/                   Gurden.unity ← the live MVP scene
│   ├── Scripts/                           16 gameplay scripts (see §4)
│   └── RULES.txt                          Your authored project rules / architecture doc
├── Settings/                              URP renderer + scene template
│   ├── Lit2DSceneTemplate.scenetemplate
│   ├── Renderer2D.asset
│   ├── Scenes/URP2DSceneTemplate.unity    URP template — used by File → New Scene, not a runtime scene
│   └── UniversalRP.asset
└── TextMesh Pro/                          TMP fonts, materials, sprites, shaders (used by popups)
    ├── Fonts/
    ├── Resources/
    ├── Shaders/
    └── Sprites/
```

Outside `Assets/`:

```
_Archive_ToReview/        Phase 1 + Phase 2 archive (125 MB, ignored by Unity)
├── README.md             Index — what was archived and why
├── OldScenes/            WitchPotion.unity, SampleScene.unity, Square.png (+ metas)
├── PotionWitch_AC_residue/   AC ScriptableObjects + menu prefabs that lived in PotionWitch/
├── AdventureCreator/     The full AC plugin (123 MB)
└── AdventureCreator.meta
```

`CLEANUP_REPORT.md` (phase-1 audit) and `CLEANUP_REPORT_FINAL.md` (this file) live at the project root.

---

## 4. Gameplay systems currently in use

All 16 scripts live in `Assets/PotionWitch/Scripts/`. Verified by GUID lookup that each is referenced by `Gurden.unity`, a prefab, or a ScriptableObject — no orphans.

### Core systems (the ones you listed as MVP)

| Script | Role |
|---|---|
| `PlantData` (defined inside `SeedItem.cs`) | ScriptableObject holding `id`, `seedIcon`, `leafIcon`, `plantPrefab`, `requiredSunCount`. Two instances live in `Graphics/Managers/Seeds/`: `Plant_Mint.asset`, `Plant_Sage.asset`. |
| `PlantPot.cs` | Source of truth for a planted pot. Owns the plant lifecycle: receives water progress, triggers growth stages, exposes `Plant()` and `Harvest()`. |
| `PlantGrowth.cs` | Visual growth stages of a plant prefab (referenced via `Plant_Mint.prefab` / `Plant_Sage.prefab`). |
| `PlantWaterMeterUI.cs` | UI representation of the water meter above the pot. Driven by `PlantPot`. |
| `SunSpawner.cs` | Lives on the SunJar; periodically spawns sun pickups inside the jar. |
| `SunSlot.cs` | One sun-slot UI element. Knows whether it's filled. |
| `SunSlotsManager.cs` | Owns the collection of `SunSlot`s for a plant. Reports "full plant" when all required slots are filled. |
| `WorldDraggableTool.cs` | Generic world-space draggable for the water can (and any other in-world tool that should be draggable with the mouse). |
| `PopupManager.cs` | The **only** system allowed to open/close popups. Holds the shared overlay and the currently-open popup reference. |
| `PopupOpener.cs` | UI component that requests popup opening via `PopupManager`. |
| `PopupCloser.cs` | UI component that requests popup closing via `PopupManager`. |
| `IPopupGate.cs` | Interface a popup can implement to veto being opened/closed when busy. |

### Supporting scripts (also alive)

| Script | Role |
|---|---|
| `HarvestCollectButton.cs` | On the Collect button inside the harvest popup. On click → `plantPot.Harvest()`. Popup closing is handled by `PopupCloser` on the same button. |
| `SeedItem.cs` | File that defines `PlantItemType` enum + `PlantData` ScriptableObject. (Note: there is **no** `SeedItem` class — the filename is a leftover. Pure-cosmetic rename to `PlantData.cs` is safe but out of scope for cleanup.) |
| `UIDragSeed.cs` | On each seed inventory item. Spawns a ghost icon on drag; on release, raycasts for a `PlantPot` under the cursor and calls `pot.Plant(plantData)`. Coordinates with `PopupManager` to hide the inventory popup during drag. |
| `UIPlantItem.cs` | Each seed UI item's data carrier. References a `PlantData` and an icon `Image`. Auto-assigns the icon in `OnValidate`. |
| `WaterParticleCollision.cs` | On the water-can `ParticleSystem`. Bridges `OnParticleCollision` → `pot.AddWaterProgress()`, with a per-pot cooldown. |

### Core loop wiring (as wired in Gurden.unity)

```
INV_Select Seed popup
    │  (each UIPlantItem has UIDragSeed)
    ▼
UIDragSeed.OnEndDrag → Camera.main.ScreenToWorldPoint
    │
    ▼
PlantPot.Plant(plantData) → instantiates plantData.plantPrefab into the pot
    │
    ▼
WaterCan_0 (WorldDraggableTool) → user drags it over the pot
    │
    ▼
PS_Water (WaterParticleCollision) → particles collide with PlantPot
    │
    ▼
PlantPot.AddWaterProgress() → PlantWaterMeterUI fills → PlantGrowth advances
    │
    ▼
SunJar (SunSpawner) spawns Sun pickups → user drags Sun (WorldDraggableTool) into SunSlot
    │
    ▼
SunSlotsManager detects full → PlantPot marks plant ready
    │
    ▼
PopupOpener on plant → PopupManager.Open(INV_HarvestPopup)
    │
    ▼
HarvestCollectButton (+ PopupCloser) → plantPot.Harvest() + close popup
```

---

## 5. Static checks I ran (no Unity required)

| Check | Command | Result |
|---|---|---|
| `using AC;` in any `.cs` | `grep -rn "using AC;" Assets/` | 0 |
| `AC.<Class>` reference in any `.cs` | `grep -rn "AC\." Assets/PotionWitch/Scripts/` | 0 |
| `m_EditorClassIdentifier: AC::` in any asset/scene/prefab | `grep -rn "AC::" Assets/` | 0 |
| AC.MainCamera script GUID `9fd2648e...c5d3c` | `grep -rn "9fd2648e8ea494b7aaeaaa1ea71c5d3c" Assets/` | 0 |
| InputsUI.prefab GUID `866106269900bca4caa0ff66d68a79fc` | `grep -rn "866106269900bca4caa0ff66d68a79fc" Assets/` | 0 |
| `.asmdef` files left in `Assets/` | `find Assets -name "*.asmdef"` | 0 |

All checks pass.

---

## 6. What to do next — Unity reimport

**Stop here and reopen Unity.** I cannot run Unity's compiler, so I cannot tell you whether the project compiles cleanly until you let Unity reimport.

When you open Unity:

1. It will detect that `Assets/AdventureCreator/` is gone and rebuild the asset database. This may take a few minutes the first time (it'll regenerate `Library/`).
2. It will regenerate the `.csproj` files at the project root. The old `AC.csproj` will disappear because there is no longer an `AC.asmdef` in `Assets/`. This is expected — those `.csproj` / `.sln` files are auto-generated by Unity, not authored.
3. Open `Assets/PotionWitch/Sample 2D Scene/Gurden.unity` and check the Console.

**Expected outcome:** zero compile errors. The remaining 16 PotionWitch scripts have no `using AC;` and no `AC.` references — they were already independent of the AC assembly.

**If you see errors,** paste the full Console output (errors *and* warnings). I will diagnose and propose the cleanest fix — **I will not invent replacement systems** unless you confirm one is wanted. Most likely cases and what I'd suggest:

| If you see... | Most likely cause | My intended fix |
|---|---|---|
| "Missing script" warnings on Main Camera in Gurden | Unity stripped a component but the YAML entry survives | Open Gurden → remove the missing-script slot from Main Camera → save |
| "Missing script" warnings on objects inside `INV_Select Seed.prefab` or `INV_HarvestPopup.prefab` | A prefab field referenced an AC menu prefab that's now archived | Open the prefab, identify the broken field, and either clear it (if optional) or report what the field was bound to before I touch it |
| `Plant_Mint.asset` / `Plant_Sage.asset` shows "Missing" for `plantPrefab` | Shouldn't happen — the prefabs are still in `Graphics/Prefabs/`. If it does, the asset itself wasn't reimported yet — let Unity finish reimport first |
| Errors mentioning `AC.Templates.InputSystemIntegration.InputRemapper` | A UnityEvent on a button still references the AC method | Tell me which button + which prefab/scene; I'll point you to the OnClick binding to clear |
| Anything else | I'll triage individually | I won't write replacement systems without your sign-off |

---

## 7. Recommended next steps after Unity confirms clean compile

These are **suggestions, not actions**. I haven't done them.

1. **Commit Phase 2.** Your current `git status` shows ~3,800 file deletions (the AC tree moved out of `Assets/`) + the Gurden scene change. Suggested commit message: `Remove Adventure Creator from project; archive residue under _Archive_ToReview`.
2. **Verify the core loop end-to-end in Play Mode**: Seed inventory → drag seed to PlantPot → water meter fills → plant grows → SunJar spawns sun → drag sun to slot → full plant → harvest popup → collect. If anything is broken, that's the smallest possible reproduction surface for diagnosing.
3. **Once Phase 2 is confirmed stable for a few days**, you can delete `_Archive_ToReview/` outright (`rm -rf _Archive_ToReview/` in PowerShell: `Remove-Item -Recurse -Force _Archive_ToReview`). Git history still has every file.
4. **Optional polish** (not cleanup — leave for later):
   - Rename `Assets/PotionWitch/Scripts/SeedItem.cs` → `PlantData.cs`. The file holds the `PlantData` ScriptableObject; there is no `SeedItem` class. Use Unity's Rename so meta GUIDs stay intact.
   - Add a top-level `CLAUDE.md` at the project root that mirrors `Assets/PotionWitch/RULES.txt`, so future Claude Code sessions pick up your architecture rules automatically.

---

## 8. Open questions / things I did **not** touch

- `Assets/InputSystem_Actions.inputactions` is the default Unity Input System asset at `Assets/` root. The actual MVP input asset is `Assets/PotionWitch/Input System/Controls.inputactions`. The root one **may** be unused, but `EditorBuildSettings.asset` still references `2bcd2660ca9b64942af0de543d8d7100` — that GUID belongs to the root file. Leaving it alone until you confirm which input asset Gurden's EventSystem actually uses. Low priority — won't affect compile, just a future tidy-up.
- The auto-generated `.csproj` / `.sln` files at project root (`AC.csproj`, `Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj`, `Pilot witch potion.sln`, `Potion-Witch.sln`) — Unity regenerates these on every project open. `AC.csproj` will disappear by itself on the next reimport.
- `Library/` and `Temp/` — Unity's caches. Will regenerate. Don't touch.

---

## 9. Awaiting your reply

Please reopen Unity, let it reimport, then come back with one of:

- ✅ "Compiles clean, loop plays through" → I'll write a short close-out summary and we're done.
- ⚠ "Compiles but I see X warnings/errors" → paste the Console; I'll triage per §6.
- 🛑 "Won't compile" → paste the full Console; I'll diagnose without writing replacement systems.

I'll wait.
