# Potion Witch — Architecture

A beginner-friendly map of the current MVP. Read this top-to-bottom on your first pass; after that, jump around using the table of contents.

> This document describes **what exists today**. Anything labelled "Planned" is a future idea, not implemented yet. The project rules in `Assets/PotionWitch/RULES.txt` are the source of truth for *how* code should be written; this file is the source of truth for *what* the code currently does.

## Table of contents

1. [How to read this document](#1-how-to-read-this-document)
2. [Folder structure](#2-folder-structure)
3. [The core gameplay loop](#3-the-core-gameplay-loop)
4. [Scene hierarchy — Gurden.unity](#4-scene-hierarchy--gurdenunity)
5. [Script responsibilities (one by one)](#5-script-responsibilities-one-by-one)
6. [Data flow between systems](#6-data-flow-between-systems)
7. [Architecture patterns used](#7-architecture-patterns-used)
8. [Planned systems — Inventory, Cauldron, Potions](#8-planned-systems--inventory-cauldron-potions)
9. [Common questions](#9-common-questions)

---

## 1. How to read this document

If you're new to Unity, three vocabulary anchors first:

- **GameObject** — a thing in a scene. A pot, a button, the camera. By itself it does almost nothing.
- **Component** — a script (or built-in like `Camera`, `SpriteRenderer`, `Collider2D`) attached to a GameObject. The behaviour lives in components.
- **Prefab** — a saved template for a GameObject + its components + its children. We instantiate prefabs to create things at runtime (e.g., the seed produces a plant prefab inside the pot).
- **ScriptableObject** — a data asset that lives in the Project, not in a scene. Think of it as a `.json` file you can edit in the Inspector. `PlantData` is one.

When a section says "this script lives on X", X is a GameObject in the scene; "this script is referenced by Y" means Y has an Inspector field pointing at this script's GameObject.

---

## 2. Folder structure

```
Assets/
├── DefaultVolumeProfile.asset             Render-pipeline defaults (don't touch)
├── InputSystem_Actions.inputactions       Unity default input asset
├── UniversalRenderPipelineGlobalSettings.asset
├── PotionWitch/                           ← your project lives here
│   ├── Graphics/
│   │   ├── Managers/
│   │   │   └── Seeds/                     PlantData assets (Plant_Mint, Plant_Sage)
│   │   ├── Pilot-Assets/                  Source sprites (BG, plant stages, UI icons, …)
│   │   ├── Prefabs/
│   │   │   ├── INV/                       INV_Select Seed + INV_HarvestPopup popups
│   │   │   ├── SunJar/                    SunJar + Sun prefab + SunSlot prefab
│   │   │   ├── Plant_Mint.prefab          Plant prefab (Spore/Mature/Full visuals)
│   │   │   ├── Plant_Sage.prefab          Same shape, different plant
│   │   │   └── WaterMeter.prefab          Water meter UI prefab
│   │   └── VFX/
│   │       └── PS_Water.prefab            Water-can particle system
│   ├── Input System/
│   │   ├── Controls.inputactions          Project input map (the live one)
│   │   └── EventSystem_InputSystem.prefab UI EventSystem wired for new Input System
│   ├── Sample 2D Scene/
│   │   └── Gurden.unity                   THE live MVP scene
│   ├── Scripts/                           All 16 gameplay scripts
│   └── RULES.txt                          Project rules / architecture rules
├── Settings/                              URP renderer + scene template
└── TextMesh Pro/                          TMP fonts/sprites/shaders (used by popups)
```

Outside `Assets/`:

```
_Archive_ToReview/                         Old AC plugin + archived dead content
CLEANUP_REPORT.md                          Phase-1 audit
CLEANUP_REPORT_FINAL.md                    Cleanup wrap-up
ARCHITECTURE.md                            ← this file
```

**Why this layout matters**: everything Unity should build/import lives under `Assets/`. The archive is outside `Assets/` so Unity ignores it. If you want to bring something back, move it back into `Assets/` keeping the `.meta` file alongside.

---

## 3. The core gameplay loop

```
   ┌───────────────────────────────────────────────────────────────────┐
   │  1. Player opens the Seed inventory (INV_Select Seed popup)       │
   │  2. Player drags a seed onto a Plant Pot                          │
   │  3. The pot spawns a plant (Spore stage) and shows a water meter  │
   │  4. Player drags the Water Can over the pot                       │
   │  5. Water particles hit the pot → meter fills                     │
   │  6. Full meter → plant grows: Spore → Mature                      │
   │  7. SunJar spawns Sun pickups (one at a time)                     │
   │  8. Player drags each Sun into a SunSlot                          │
   │  9. All slots filled → plant grows: Mature → Full                 │
   │ 10. Player clicks the full plant → Harvest popup opens            │
   │ 11. Player clicks Collect → pot resets, ready for a new seed      │
   └───────────────────────────────────────────────────────────────────┘
```

A simpler way to remember it:

> **Seed → Plant → Water → Sun → Harvest → repeat.**

Each arrow is a method call between two scripts. The next sections show which scripts own each step.

---

## 4. Scene hierarchy — Gurden.unity

This is what you see when you open Gurden in the Hierarchy panel. (Names match the actual scene; sub-items are the most relevant children, not exhaustive.)

```
Gurden
├── Main Camera                       Orthographic 2D camera. AudioListener + URP camera data.
├── EventSystem                       UI input routing (uses Input System UI module).
├── ForGround                         Foreground sprites layer.
├── BG_0                              Background sprite.
├── CanvasHUD                         Top-level Screen-Space Canvas.
│   ├── Overlay_Darken                Shared dim overlay used by every popup.
│   ├── PopupManager                  PopupManager component lives here. Children = popups.
│   │   ├── INV_Select Seed           Seed inventory popup (CanvasGroup'd, see PopupManager).
│   │   │   └── UIPlantItem(s)        Seed icons with UIDragSeed + UIPlantItem.
│   │   └── INV_HarvestPopup          Harvest popup with the Collect button.
│   │       └── (button)              HarvestCollectButton + PopupCloser.
│   ├── WaterMeter                    PlantWaterMeterUI lives here.
│   ├── ToolTip / ToolTip_Fill        Visual elements driven by PlantWaterMeterUI.
│   ├── Icon_Cauldron                 Static decoration (planned interactable, §8).
│   ├── Icon_Letters                  Decorative letters art.
│   ├── Icon_Drop                     Water icon next to the meter.
│   └── Shelf_0 / Shelf_0 (1)         Background shelves.
├── WorldInteractable                 Parent for clickable world objects.
│   ├── Plant Pot_0                   PlantPot component lives here.
│   │   └── (plant spawn anchor)      Plant prefab is instantiated under the pot.
│   ├── Water Can_0                   WorldDraggableTool (ToolType = WaterCan).
│   │   ├── WaterCan_Visual           Tilts during drag.
│   │   └── PS_Water                  Particle system (WaterParticleCollision).
│   ├── SunSlots                      SunSlotsManager + 5 SunSlot children.
│   ├── Spawn (SunJar)                SunSpawner component + SunJar visuals.
│   └── WaterHitZone                  Collider used by water particles to find the pot.
└── (lighting + URP rigs)
```

The two important sub-trees:
- **`CanvasHUD`** owns everything that should sit on screen (UI). It hosts `PopupManager` which owns popups.
- **`WorldInteractable`** owns everything the player can click or drag in the world (pot, water can, suns, sun jar).

---

## 5. Script responsibilities (one by one)

All 16 scripts live in `Assets/PotionWitch/Scripts/`.

### 5.1 Data

#### `SeedItem.cs` — defines `PlantData` (ScriptableObject)
- **What it is**: A data asset. Two instances exist today: `Plant_Mint.asset`, `Plant_Sage.asset`.
- **Fields**: `id`, `seedIcon`, `leafIcon`, `plantPrefab`, `requiredSunCount`.
- **Why a ScriptableObject and not a class**: so you can edit values in the Inspector without writing code, and reuse the same data between the inventory UI and the pot.
- **File-name note**: the file is called `SeedItem.cs` for historical reasons but contains only the `PlantItemType` enum and the `PlantData` class. There is no `SeedItem` class. Safe to rename to `PlantData.cs` later.

### 5.2 Plant lifecycle

#### `PlantPot.cs`
- **Role**: The **source of truth** for one pot. Decides whether you can plant, what's planted, and whether you can harvest.
- **Lives on**: each `Plant Pot_*` GameObject in the scene.
- **Public API**:
  - `Plant(PlantData)` — called by `UIDragSeed` when a seed is dropped on the pot. Returns `true` if it actually planted.
  - `Water()` — instantly advances growth one step. Currently called *through* `AddWaterProgress`, not directly from any external script.
  - `AddWaterProgress(float)` — called by `WaterParticleCollision` on every accepted particle hit. Fills the meter; when the meter completes, calls `Water()` internally.
  - `OnAllSunSlotsFilled()` — called by `SunSlotsManager` when every active slot is filled. Advances growth.
  - `Harvest()` — called by `HarvestCollectButton` when Collect is clicked. Resets the pot.
  - `CanHarvest()` — returns true when the plant is fully grown.
- **Also implements `IPopupGate`** — so a `PopupOpener` on the same pot only opens the harvest popup when the plant is ready.
- **Private state**: `currentPlantInstance`, `currentPlantGrowth`, `hasBeenWatered`. Kept private so the rest of the project can't poke at it — they must go through the public methods.

#### `PlantGrowth.cs`
- **Role**: The visual stages of a single plant prefab. Toggles which child sprite is active (`Spore`, `Mature`, `Full`).
- **Lives on**: the root of each plant prefab (`Plant_Mint.prefab`, `Plant_Sage.prefab`).
- **Public API**:
  - `GrowToNextStage()` — advance one step in the chain `Spore → Mature → Full`. Idempotent at the Full stage.
  - `CurrentStage` (getter) — read-only; used by `PlantPot.CanHarvest()`.
- **Why it's separate from `PlantPot`**: a pot owns *what's planted*; the growth visual owns *how the plant looks right now*. Keeping them separate means future additions (timers, particle effects on growth, save-data for stage) live in `PlantGrowth` without touching `PlantPot`.

#### `PlantWaterMeterUI.cs`
- **Role**: The water meter you see above the pot. A height-driven fill bar.
- **Lives on**: the `WaterMeter` UI GameObject under `CanvasHUD`.
- **Public API**:
  - `Show()` / `Hide()` — toggle visibility.
  - `SetFill(0..1)` — set a target the bar should animate toward.
  - `AddFill(amount)` — bump the target up; returns `true` when it reaches 1.
  - `ResetMeter()` — snap back to empty and show.
- **Why the animation in `Update()`**: a single big water-particle hit could otherwise jump the bar visually. Letting the bar chase its target with `Mathf.MoveTowards` makes the fill feel smooth.

### 5.3 Sun

#### `SunSpawner.cs`
- **Role**: The SunJar. Spawns one Sun prefab per click on the jar, up to the budget set by `PlantPot`.
- **Lives on**: the SunJar GameObject in `WorldInteractable`.
- **Public API**:
  - `SetAvailableSuns(int)` — called by `PlantPot.Plant()` (gives the budget) and by `PlantPot.Harvest()` (resets to 0).
  - `NotifyActiveSunResolved()` — called by `WorldDraggableTool` when a dragged Sun has landed in a slot or returned home. Lets the jar spawn another.
- **Input model**: polls the Input System mouse directly in `Update()` (not via `EventSystem`). Same pattern as `WorldDraggableTool` — keeps the logic visible and debuggable.

#### `SunSlot.cs`
- **Role**: One slot in the SunSlots row. Empty silhouette → filled-sun on drop.
- **Lives on**: each `Slot_0`, `Slot_1`, … GameObject under `SunSlots`.
- **Public API**: `Fill()` (returns true if it accepted), `ResetSlot()`.

#### `SunSlotsManager.cs`
- **Role**: Owner of the SunSlot row. Knows how many slots are visible for the current plant and reports when they're all full.
- **Lives on**: the `SunSlots` parent GameObject.
- **Public API**:
  - `ShowSlots(amount)` — called by `PlantPot.Plant()`. Activates the first N slots and resets them.
  - `HideSlots()` — called by `PlantPot.Harvest()`.
  - `CheckCompletion()` — called by `WorldDraggableTool` after a successful slot fill. If all active slots are filled, calls `PlantPot.OnAllSunSlotsFilled()`.
  - `AreAllActiveSlotsFilled()` — boolean query.

### 5.4 Tools (water can + sun)

#### `WorldDraggableTool.cs`
- **Role**: Generic in-world drag handler. Used for both the water can and individual suns. Branches behaviour with a `ToolType` enum.
- **Lives on**: the Water Can GameObject and each spawned Sun.
- **Input model**: polls `Mouse.current` directly. Doesn't rely on `OnMouseDown` (which silently fails when raycasters / sorting layers are misconfigured).
- **Lifecycle**: `BeginDrag` → `Drag` (per frame while held) → `EndDrag`.
- **On release**:
  - `WaterCan` → snaps back to its start position. Watering itself happens via particles (see `WaterParticleCollision`).
  - `Sun` → looks for a `SunSlot` under the cursor. If found and not filled, fills it, asks the manager to check completion, tells the parent `SunSpawner` it's resolved, and destroys itself. Otherwise returns home.
- **Why polling instead of `IDragHandler`**: world objects don't fit Unity's UI drag pipeline cleanly. Polling is uniform, predictable, and survives EventSystem hiccups.

#### `WaterParticleCollision.cs`
- **Role**: Bridge between the water-can `ParticleSystem` and the `PlantPot`.
- **Lives on**: the `PS_Water` GameObject (sibling of the water-can sprite).
- **Mechanism**: Unity calls `OnParticleCollision(other)` whenever a water particle collides with another collider. We look up `PlantPot` on `other`, then call `pot.AddWaterProgress(0.1f)` with a per-pot cooldown so a continuous stream doesn't tick every frame.

### 5.5 Popups (shared system)

The rule from `RULES.txt`: **no script ever calls `popup.SetActive(true/false)` directly.** All popup operations go through `PopupManager`.

#### `PopupManager.cs`
- **Role**: The **only** system that opens or closes popups. Owns the shared dim overlay (`Overlay_Darken`) and tracks the currently-open popup.
- **Lives on**: the `PopupManager` GameObject inside `CanvasHUD`.
- **Hide strategy**: if the popup has a `CanvasGroup`, hides via `alpha=0` + raycasts off (so in-progress UI drags survive); otherwise `SetActive(false)`. This is what keeps `UIDragSeed` able to finish a drag after closing the seed inventory.
- **Public API**: `Open(popup)`, `Close()`, `Close(popup)`.

#### `PopupOpener.cs`
- **Role**: World-space click → request popup open.
- **Lives on**: any clickable world object that should open a popup (e.g., `Plant Pot_0` references the harvest popup).
- **Gating**: takes an optional `MonoBehaviour` that implements `IPopupGate`. If set, the popup only opens when `gate.CanOpen()` is true. `PlantPot` is the canonical gate — the harvest popup opens only when the plant is fully grown.

#### `PopupCloser.cs`
- **Role**: UI button click → close popup. Self-wires to the `Button.onClick` event in `Awake`.
- **Lives on**: the OK / X / Collect button inside a popup.
- **Two modes**:
  - `specificPopup` left empty → closes whichever popup is currently open.
  - `specificPopup` set → closes exactly that popup.

#### `IPopupGate.cs`
- **Role**: One-method interface (`bool CanOpen()`) so a gameplay script can veto a popup open. `PlantPot` implements it.

### 5.6 Seed inventory (current UI)

#### `UIPlantItem.cs`
- **Role**: Data carrier on each seed item in the inventory popup. References a `PlantData` and an icon `Image`. Auto-assigns the icon based on `PlantItemType.Seed` / `Leaf` in `OnValidate`.

#### `UIDragSeed.cs`
- **Role**: Drag handler on each seed item. Spawns a ghost icon that follows the cursor; on release, raycasts the world under the cursor for a `PlantPot` and calls `pot.Plant(plantData)`.
- **Coordinates with `PopupManager`**: hides the inventory popup at drag start (so the user can see the world); restores it on a missed drop.
- **Lives on**: each seed UI item, alongside `UIPlantItem`.

#### `HarvestCollectButton.cs`
- **Role**: The Collect button inside the harvest popup. On click → `plantPot.Harvest()`.
- **Closing the popup is *not* this script's job** — a `PopupCloser` on the same button does that. Separating the two keeps each script single-purpose.

---

## 6. Data flow between systems

Read this as: "who calls whom, and with what data."

```
                     ┌─────────────────────────┐
                     │  PlantData (asset)      │  ← Mint, Sage (ScriptableObjects)
                     └─────────────┬───────────┘
                                   │ reference
                                   ▼
   ┌───────────────────┐    ┌──────────────────┐    ┌──────────────────────┐
   │   UIPlantItem     ├───►│   UIDragSeed     ├───►│      PlantPot        │◄──┐
   │  (icon + data)    │    │  (ghost + drop)  │    │   (source of truth)  │   │
   └───────────────────┘    └──────────────────┘    └─────┬────────────────┘   │
                                                          │                    │
                                  ┌───────────────────────┼──────────────┐     │
                                  │                       │              │     │
                                  ▼                       ▼              ▼     │
                          ┌───────────────┐      ┌──────────────────┐ ┌───────┴───────┐
                          │ PlantGrowth   │      │PlantWaterMeterUI │ │SunSlotsManager│
                          │ (Spore/Mature │      │ (fill bar)       │ │ (row of slots)│
                          │  /Full)       │      └──────────────────┘ └───────┬───────┘
                          └───────────────┘             ▲                     │
                                                        │ AddFill             │ Show/Hide/Check
                                                ┌───────┴────────┐    ┌───────┴───────┐
                                                │WaterParticle   │    │   SunSlot     │
                                                │Collision       │    │ (Fill/Reset)  │
                                                └────────────────┘    └───────▲───────┘
                                                        ▲                     │
                                                        │ OnParticleCollision │ Fill()
                                                        │                     │
                                                  ┌─────┴──────┐    ┌─────────┴──────┐
                                                  │PS_Water    │    │WorldDraggable  │
                                                  │(particles) │    │Tool  (Sun)     │
                                                  └────────────┘    └─────────▲──────┘
                                                        ▲                     │
                                                        │ child of            │ spawns
                                                        │                     │
                                                  ┌─────┴──────────┐  ┌───────┴──────┐
                                                  │WorldDraggable  │  │  SunSpawner  │
                                                  │Tool (WaterCan) │  │   (SunJar)   │
                                                  └────────────────┘  └──────────────┘

   ┌──────────────────────┐ open/close ┌──────────────────────┐
   │   PopupOpener        ├───────────►│    PopupManager      │
   │  (world click)       │            │ (only system that    │
   └──────────────────────┘            │  shows/hides popups) │
                                       └──────────▲───────────┘
   ┌──────────────────────┐ Close()               │
   │   PopupCloser        ├───────────────────────┘
   │  (UI button)         │
   └──────────────────────┘
```

Three rules embedded in this diagram:

1. **`PlantPot` is the only thing that mutates plant state.** Other scripts ask it to do things; they don't reach in.
2. **`PopupManager` is the only thing that opens/closes popups.** `PopupOpener` and `PopupCloser` are requesters, not actors.
3. **`PlantData` flows in one direction**: from the asset → through `UIPlantItem` / `UIDragSeed` → into `PlantPot.Plant()`. Nothing writes to it at runtime.

---

## 7. Architecture patterns used

These are the small habits that keep the project readable.

- **One script, one responsibility.** `HarvestCollectButton` calls `Harvest()`; `PopupCloser` closes the popup. They sit on the same button, but each owns one verb. Same idea in `PlantPot` (state) vs `PlantGrowth` (visuals).
- **Single source of truth.** Each pot's state lives only in its `PlantPot`. The UI reads it, doesn't mirror it.
- **Public methods, private state.** `PlantPot.currentPlantInstance` is private; you can't grab the plant from outside. You ask the pot to do something instead.
- **Optional gating via interface.** `IPopupGate` is one method, used by exactly one consumer (`PopupOpener`). When a small interface enables a clean veto path, that's worth more than a giant base class.
- **Null-safe optional refs.** Most Inspector fields are `[SerializeField]`-private and null-checked at the point of use (e.g., `if (waterMeterUI != null)`). That way a half-wired scene still runs and surfaces problems via behaviour, not crashes.
- **Polling input over `OnMouseDown`.** Both `WorldDraggableTool` and `PopupOpener` read `Mouse.current` directly. Predictable, debuggable, and not at the mercy of UI raycaster setup.
- **Hide via `CanvasGroup` when a drag must survive.** `PopupManager` prefers `alpha=0` to `SetActive(false)` when a popup has a `CanvasGroup`. This is what lets `UIDragSeed` keep going after closing the inventory mid-drag.
- **No premature abstractions.** There is no `IPlant` interface, no `BasePopup` class, no event bus. Add abstraction when the second concrete case appears — not before.

---

## 8. Planned systems — Inventory, Cauldron, Potions

> Everything below this line is **not implemented yet**. It's a sketch of where the architecture is heading so future work doesn't accidentally break current systems. Treat it as a discussion, not a spec.

### 8.1 Inventory

The codebase is already half-prepared for this:

- `PlantData` has both a `seedIcon` and a `leafIcon`.
- `UIPlantItem` already supports `PlantItemType.Seed` vs `PlantItemType.Leaf`.
- `HarvestCollectButton`'s comment literally says: *"MVP: no inventory system yet — the harvested plant is simply removed. When an inventory system is added later, give the seed/leaf to it from here."*

**Suggested shape** (smallest thing that works):

- A new `Inventory` MonoBehaviour, single scene instance. Holds a dictionary `Dictionary<PlantData, int>` of leaf counts.
- Two methods: `Add(PlantData, int)` and `TryRemove(PlantData, int)`.
- `PlantPot.Harvest()` calls `Inventory.Add(plantedData, 1)` *before* destroying the plant instance. (Cache the planted `PlantData` when `Plant()` runs so `Harvest()` knows which leaf to grant.)
- A new `INV_Leaves` popup (sibling of `INV_Select Seed`), opened through `PopupManager`, rendered with the same `UIPlantItem` script switched to `PlantItemType.Leaf`.

**Where it plugs in**:
- `PlantPot.cs` — add `private PlantData plantedData;` set in `Plant()` and consumed in `Harvest()`.
- New file `Inventory.cs` in `Assets/PotionWitch/Scripts/`.
- New prefab `INV_Leaves.prefab` in `Assets/PotionWitch/Graphics/Prefabs/INV/`.

### 8.2 Cauldron

There's already a `Cauldron.png` sprite and an `Icon_Cauldron` GameObject in `Gurden.unity`. Today they're decoration.

**Suggested shape**:

- A new `Cauldron` GameObject under `WorldInteractable`, with a `Collider2D` and a `PopupOpener` pointing at a new `INV_Cauldron` popup. **No new opener pattern needed** — `PopupOpener` already handles "click world object → open popup".
- The cauldron popup is essentially a mixing station: drop slots that accept leaves from the inventory popup. Re-use the `UIDragSeed`/`UIPlantItem` drag pattern, generalising it to "drag inventory item to drop slot".
- When all required slots are filled, click a "Brew" button → trigger a recipe lookup.

**Where it plugs in**:
- New `Cauldron.cs` MonoBehaviour: owns the recipe-match logic. Implements `IPopupGate` if you want to gate the popup on "is the cauldron empty / busy / cooling down".
- Generalise `UIDragSeed` → `UIInventoryDrag` (or keep `UIDragSeed` and add a sibling). Decide *only* when the second concrete case actually arrives, per the rules.

### 8.3 Potions

The output of the cauldron.

**Suggested shape**:

- A new `PotionData` ScriptableObject (same shape as `PlantData` but holding `id`, `icon`, `bottlePrefab`).
- A new `Recipe` ScriptableObject mapping `List<PlantData> ingredients` → `PotionData result`. Stored in `Assets/PotionWitch/Graphics/Managers/Recipes/`.
- `Cauldron.Brew()` reads the active slots, finds a matching `Recipe`, calls `Inventory.AddPotion(recipe.result)`.

**Where it plugs in**:
- New folder `Assets/PotionWitch/Graphics/Managers/Potions/` and `.../Recipes/`.
- New scripts `PotionData.cs`, `Recipe.cs` next to `SeedItem.cs`.
- `Inventory` gets a parallel `Dictionary<PotionData, int>` (or a single generic `Dictionary<ScriptableObject, int>` if you want to unify — but that abstraction can wait).

### 8.4 What *not* to do yet

The rules file says it best — *don't build future systems early*. Each of the three above should be added in this order:

1. Inventory (smallest; unlocks the rest).
2. Cauldron with hardcoded recipes (one or two), to find out what the API needs to look like in practice.
3. Recipe / PotionData ScriptableObjects, once you've seen the cauldron in action and know what shape recipes really need.

Don't add an event bus, an `IItem` interface, a `BasePopup` class, or save/load until a concrete second use forces them. That's how the MVP stayed small.

---

## 9. Common questions

**"Where do I add a new plant type?"**
1. Right-click in `Assets/PotionWitch/Graphics/Managers/Seeds/` → Create → Game → Plant. Fill in `id`, sprites, prefab, required suns.
2. Duplicate `Plant_Mint.prefab`, adjust the sprites for each growth stage child.
3. Add a new seed item to the `INV_Select Seed` popup with `UIPlantItem` + `UIDragSeed` and point it at the new `PlantData` asset.

**"How does the harvest popup know which pot is harvesting?"**
Today: it doesn't — there's exactly one pot in the scene, and `HarvestCollectButton` references it directly in the Inspector. When you add more pots, replace that direct reference with "the pot whose `PopupOpener` opened this popup", set on open. (The comment on `HarvestCollectButton` already calls this out.)

**"Why doesn't `PlantPot.Water()` get called from outside?"**
It's public on purpose — future tools (a one-shot magic-wand watering, a debug button) might want to bypass the meter. Today the only call site is internal to `AddWaterProgress`, and that's fine. Public-but-unused is a small cost; renaming or removing it later is cheap.

**"Where does the seed inventory popup come from? I don't see code instantiating it."**
It lives as a child of `PopupManager` in Gurden and starts hidden because `PopupManager.hideChildPopupsOnAwake` is true. Opening is triggered by some opener button in the HUD (see `PopupOpener` on a button — likely `Open_Plants_Inv`).

**"What's `Overlay_Darken`?"**
A scene-wide dim layer that sits behind every popup. `PopupManager` owns it and toggles it whenever a popup opens or closes, so every popup looks like it dims the world without each popup having its own overlay.

**"Why is `PopupCloser` separate from `HarvestCollectButton`?"**
One button does two things on click: trigger gameplay (`Harvest`) and close UI (`PopupManager.Close`). Splitting them into two components keeps each one single-purpose, and `PopupCloser` is reusable on any other "close me" button without dragging gameplay code along.

---

If something in this document drifts out of date with the code, the code wins. Update this file when the architecture shifts; don't try to reverse-engineer a stale section.
