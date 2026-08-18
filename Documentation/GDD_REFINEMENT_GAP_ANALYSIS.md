# DROP BY DROP: FLOW OF HOPE  
## GDD Refinement Gap Analysis (Phase 1)

**Project:** existing Unity implementation (`ZurisMission` / Drop by Drop)  
**Date:** 16 August 2026  
**Scope:** inspect only. No gameplay, scene, prefab, or script changes in this pass.  
**Rule:** extend working systems, modify mismatches, complete partial work, create new systems only when nothing exists to extend.

---

### Source of GDD requirements

No Game Design Document file exists in this repository. Requirements below are taken from:

1. This Phase 1 brief (scene identity, systems list, refinement rules).
2. Fantasy already present in the shipped scenes and HUD copy: village restoration, water collection, materials, well/tank repair, three biome levels.
3. Existing tags, scenes, and scripts as the current “design in code.”

If a full GDD is supplied later, this document should be re-checked against that source of truth.

---

### Existing level / scene inventory (must remain)

| Scene file | Role in brief | Status |
|---|---|---|
| `Assets/Game/Scenes/Scene Levels/MainGame.unity` | Level 1 — Dry Bushlands | Present. In build settings. |
| `Assets/Game/Scenes/Scene Levels/Level2.unity` | Level 2 — Mudlands | Present. In build settings. |
| `Assets/Game/Scenes/Scene Levels/Level3.unity` | Level 3 — Industrial Zone | Present as a scene file. |
| `Assets/Game/Scenes/Scene Levels/Level3End.unity` | Not named in the GDD brief | Present. **Currently listed in `EditorBuildSettings` as the Level 3 build entry.** Shares the same `.meta` GUID as `Level3.unity`. |
| `Assets/Game/Scenes/Scene Levels/StartScreen.unity` | Menu | Present. In build settings. |

**Do not rename, merge, delete, or replace `MainGame`, `Level2`, or `Level3`.**  
The `Level3` vs `Level3End` / GUID collision is a **risk**, not a license to delete. Resolve by wiring, not by rebuilding.

Supporting scenes referenced in code but not in the GDD brief: `StarterInfor` (loaded from `PauseMenu.HUDInfor()`, not in build settings).

---

## Status board

| System | Verdict |
|---|---|
| Player movement | EXISTS BUT NEEDS REFINEMENT |
| Running | EXISTS AND WORKS (with caveats) |
| Lane switching | EXISTS BUT NEEDS REFINEMENT |
| Jumping | EXISTS BUT NEEDS REFINEMENT |
| Sliding | PARTIALLY IMPLEMENTED |
| Hazards | EXISTS BUT NEEDS REFINEMENT |
| Pickups | EXISTS BUT NEEDS REFINEMENT |
| Enemies | PARTIALLY IMPLEMENTED |
| Spawning | EXISTS BUT NEEDS REFINEMENT |
| Procedural generation | PARTIALLY IMPLEMENTED |
| Checkpoints | MISSING |
| UI | EXISTS BUT NEEDS REFINEMENT |
| Health | EXISTS BUT NEEDS REFINEMENT |
| Stamina | MISSING |
| Hydration | EXISTS BUT NEEDS REFINEMENT |
| Resources | EXISTS BUT NEEDS REFINEMENT |
| Pipe puzzles | PARTIALLY IMPLEMENTED |
| Crafting | MISSING |
| Upgrades | MISSING |
| Skills | MISSING |
| Bosses | MISSING |
| Scene transitions | EXISTS BUT NEEDS REFINEMENT |
| Saving | MISSING |
| Audio | MISSING (no game audio system) |

---

## 1. Player movement

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
Three movement scripts exist:

- `PlayerController.cs` — Phase 1 controller: auto-run on Z, A/D lane snap, Space/W jump, sphere-cast ground check, timed speed modifiers. `SceneBootstrapper` adds this at runtime and **disables** `PlayerMovement`.
- `PlayerMovement.cs` — original 4-lane runner; random lane switch every 2s; collision-based ground flag. Kept so the scene still compiles; disabled when `PlayerController` is present.
- `PlayerMovementOG.cs` — free left/right translate (not lanes). Used by `PipeControlslevel3`. Commented crouch/slide scale logic.

### GDD requirement
A single player avatar that runs, changes lanes, jumps, and (later) slides, consistent across Dry Bushlands, Mudlands, and Industrial Zone.

### Difference
Movement is split across three scripts. Level 3 pipe flow still targets `PlayerMovementOG`. `SceneBootstrapper` injects `PlayerController` into any GameObject named `Player`, which can fight Level 3’s OG movement if that scene also uses a `Player` object.

### Required modification
Keep `PlayerController` as the runner controller. Do **not** replace it. Extend it for slide. Keep `PlayerMovement` as a disabled legacy component until scenes no longer serialize it. Point Level 3 pipe HUD at the same player body without adding a fourth controller. Gate `SceneBootstrapper` so it does not disable Level 3’s intended movement if OG is the industrial controller.

### Existing files to modify
`PlayerController.cs`, `SceneBootstrapper.cs`, `PipeControlslevel3.cs`, player GameObjects in `MainGame` / `Level2` / `Level3` (component enable flags only).

### New files required
None for core movement.

### Risk of modification
**Medium.** Wrong bootstrap on Level 3 can break pipe runs. Do not delete `PlayerMovement` or `PlayerMovementOG` while scenes still reference them.

---

## 2. Running

**Verdict:** EXISTS AND WORKS (caveats)

### Current implementation
`PlayerController.MovePlayer()` advances `z` every physics step at `baseSpeed` (25). Speed boosts/slows via `ApplySpeedModifier`. Legacy scripts also auto-run when grounded.

### GDD requirement
Continuous forward run as the core loop of an endless / corridor runner, with temporary speed changes from pickups.

### Difference
Speed modifiers work on `PlayerController`. `HUDControls.SpeedControls` still exists and is wired from pickups. `PipeControlslevel3.SpeedControls` still uses a broken one-frame timer on `PlayerMovementOG.playerSpeed`. Run does not respect a dedicated stamina resource.

### Required modification
Keep `PlayerController` run. Extend `SpeedControls` in Level 3 to call the same modifier path, or keep OG speed only on that scene without duplicating a new runner. Later, drain stamina from run/sprint if the GDD requires it — extend HUD, do not add a second speed owner.

### Existing files to modify
`PlayerController.cs`, `HUDControls.cs`, `PipeControlslevel3.cs`.

### New files required
None.

### Risk of modification
**Low** for Levels 1–2. **Medium** for Level 3 because it is on a different movement script.

---

## 3. Lane switching

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
`PlayerController`: four lanes `{ -6, -2, 2, 6 }`, player-owned A/D.  
`PlayerMovement`: same lane array but `RandomLaneSwitch` every 2 seconds (player agency removed). Disabled when controller is present.  
`Lanemanager2` / `Lanemanager3`: lane *spawn points* for obstacles, plus UI buttons that spawn into lanes.

### GDD requirement
Player-controlled lane changes as the primary dodge. Obstacle/pickup placement should sit on those same lanes.

### Difference
Pickup scatter (`SpawnObjects`) uses a wide random X (`-10..10`), not the four lanes. Lane managers spawn obstacles on lanes but also expose 3–4 HUD buttons as if the player spawns hazards. Random lane switching still exists in the legacy script.

### Required modification
Keep `PlayerController` lanes. Align spawn X to the same lane array (extend `SpawnObjects` / lane managers). Treat lane buttons as debug or remove their gameplay role once auto-spawn is trusted. Do not add a second lane component.

### Existing files to modify
`PlayerController.cs`, `SpawnObjects.cs`, `Lanemanager2.cs`, `Lanemanager3.cs`.

### New files required
None. Optional later: one shared `LaneDefinition` ScriptableObject used by player + spawners (only if Inspector duplication becomes painful).

### Risk of modification
**Medium.** Changing lane X without updating placed scene props will miss collisions.

---

## 4. Jumping

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
Impulse jump on Space / W / Up when sphere-cast grounded (`PlayerController`). Legacy jump uses `OnCollisionEnter` with tag `Ground`, which sets `isGrounded = false` on any non-ground collision (including pickups).

### GDD requirement
Jump to clear low obstacles; reliable ground detect; no random failed jumps.

### Difference
Controller jump is the right base. No jump height tiers, no coyote time, no jump-over vs slide-under split. Ground mask is `~0` (everything).

### Required modification
Extend `PlayerController` (coyote time, ground layer). Do not revive collision-based grounded on the live runner.

### Existing files to modify
`PlayerController.cs`. Optionally ground layer on existing ground prefabs.

### New files required
None.

### Risk of modification
**Low–medium.** Wrong ground mask can stop jumping or allow mid-air jumps.

---

## 5. Sliding

**Verdict:** PARTIALLY IMPLEMENTED

### Current implementation
Only in `PlayerMovementOG`: commented `KeyCode.P` scale-to-0.5 crouch. Not on `PlayerController`. No collider height change, no animation hook, no “slide under” hazard type.

### GDD requirement
Slide as a second vertical dodge (under pipes / low beams), especially Industrial Zone.

### Difference
Design intent exists as dead code. Live runner cannot slide.

### Required modification
Complete on `PlayerController` (input, duration, collider height). Reuse OG’s scale idea only as reference. Add or tag slide-under hazards in existing Level 3 props — do not rebuild the level.

### Existing files to modify
`PlayerController.cs`, player collider on existing Player prefab/object, selected Level 3 obstacle prefabs/tags.

### New files required
None unless a tiny `SlideHazard` tag/component is cleaner than overloading `Obstacle`.

### Risk of modification
**Medium.** Collider resize can miss ground or clip into tiles.

---

## 6. Hazards

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
Tags: `Obstacle`, `Heat&Disease`, `SlowDown`, `AnimalAttack`, `PipeHit`.  
`PlayerHUDBase` maps Obstacle/AnimalAttack → health down; Heat&Disease → player water down; SlowDown → speed 15.  
Placed in `MainGame` / `Level2`; Level 3 uses obstacles + pipe tags.  
`MovingObstacle.cs` is entirely commented out.  
`ObstacleRunner` (`ChasingObstacle.cs`) has unused methods; left+right translate in the same frame cancels motion.

### GDD requirement
Biome-appropriate hazards (bushland heat/animals, mud slow, industrial pipes/traffic) with readable telegraphs and consistent damage.

### Difference
Hazards exist as tagged colliders. No telegraph, no distinct damage tables, no pooling, chasing/moving AI is dead. `DestroyObject` deletes the pickup/hazard on player trigger (good) but spawners keep instantiating.

### Required modification
Keep tag routing in `PlayerHUDBase`. Fill in damage/hydration numbers to match GDD. Repair or retire `ObstacleRunner` / `MovingObstacle` in place — do not add a parallel “HazardManager.” Align `Lanemanager*` obstacle sets with biome.

### Existing files to modify
`PlayerHUDBase.cs`, `HUDControls.cs`, `ChasingObstacle.cs`, `MovingObstacle.cs`, `Lanemanager2.cs`, `Lanemanager3.cs`, existing hazard prefabs (tags/colliders).

### New files required
None unless a shared `HazardDefinition` asset is needed for numbers. Prefer Inspector fields on existing HUD first.

### Risk of modification
**Medium.** Tag changes can silently stop damage.

---

## 7. Pickups

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
Tags: `WaterDROP`, `DamWaterBUCK`, `Materials`, `FruitPickup`, `Herbs`, `SpeedBoast`.  
Handled in `PlayerHUDBase` (L1/L2) and `PipeControlslevel3` (L3).  
`DestroyObject` destroys on player enter.  
`SpawnObjects` randomly instances from an array onto new ground tiles.

### GDD requirement
Collectibles that feed hydration, bucket/well fill, materials for restoration, health, and occasional speed. Density and lanes should be designed, not fully random.

### Difference
Systems exist and fire. Placement is not lane-aware. Typo tag `SpeedBoast`. Logs and names mixed (water drop vs bucket). No hidden collectibles, no pickup feedback beyond debug/HUD text. Level 3 `hasHit` latch can block later pickups after the first trigger.

### Required modification
Extend `SpawnObjects` for lane slots and per-level tables. Fix `PipeControlslevel3.hasHit` (complete, do not replace). Keep tags; do not invent a second inventory.

### Existing files to modify
`SpawnObjects.cs`, `PlayerHUDBase.cs`, `PipeControlslevel3.cs`, `DestroyObject.cs`, prefab tag `SpeedBoast` (keep tag name unless all scene objects are updated together).

### New files required
None required. Optional: ScriptableObject spawn table — only if arrays in `SpawnObjects` become unmaintainable.

### Risk of modification
**Medium.** `hasHit` fix is high value / low surface. Tag rename is high risk.

---

## 8. Enemies

**Verdict:** PARTIALLY IMPLEMENTED

### Current implementation
Tag `AnimalAttack`. Asset packs include animals/people. `ObstacleRunner` intended to move animals/people; methods are never called from `Update` and movement cancels itself.

### GDD requirement
Simple hostile encounters (animals, people, later industrial threats) that occupy lanes and punish contact.

### Difference
Enemies are mostly static tagged obstacles, not AI agents.

### Required modification
Complete `ObstacleRunner` **or** convert those prefabs to the same tagged-hazard path used today. Do not add a new AI framework. Prefer making existing prefabs move along Z/lanes.

### Existing files to modify
`ChasingObstacle.cs` (`ObstacleRunner`), animal/people prefabs already in scenes.

### New files required
None.

### Risk of modification
**Medium.** Enabling broken movement can push enemies off the track.

---

## 9. Spawning

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
Multiple spawners:

| Script | Class name | Role |
|---|---|---|
| `SpawnObjects.cs` | `SpawnObjects` | Pickups on new ground |
| `GroundSpawnner.cs` | `GroundSpawnner` | Next ground tile + calls `SpawnObjects` |
| `Lanemanager2.cs` | `Lanemanager2` | L2 obstacles every 5s + lane buttons + water drain |
| `Lanemanager3.cs` | `Lanemanager3` | Same for L3, 4 buttons, closer spawn |
| `BucketSpawner.cs` | `Spawner` | Spawns 10 obstacles **every frame** at random XZ |

### GDD requirement
One coherent spawn story per level: ground, pickups, hazards, density that ramps.

### Difference
Spawners overlap and do not share lane data. `Spawner` is unsafe if enabled (instantiate-every-frame). Class name `Spawner` vs file `BucketSpawner.cs` is misleading. Lane managers drain water on a timer, mixing spawn with resource sim.

### Required modification
**Do not create a new master spawner.** Disable or gate `Spawner` if present in scenes. Extend `SpawnObjects` + `GroundSpawnner` for L1. Extend `Lanemanager2/3` for L2/L3 density. Move water drain out of the spawn coroutine into HUD/hydration.

### Existing files to modify
`SpawnObjects.cs`, `GroundSpawnner.cs`, `Lanemanager2.cs`, `Lanemanager3.cs`, `BucketSpawner.cs`, scene Inspector enables.

### New files required
None.

### Risk of modification
**High** if `Spawner.Update` is active in a scene. **Medium** when changing spawn distances vs existing tile length (71 units).

---

## 10. Procedural generation

**Verdict:** PARTIALLY IMPLEMENTED

### Current implementation
`GroundSpawnner`: on tag `Trigger`, instantiate `groundPrefabTrigger` at `z + 71`, spawn objects, `Destroy(newGround, 50f)`. Not pooled. Tile length and destroy time are hard-coded.

### GDD requirement
Endless-feeling corridor with biome tiles (bushland / mud / industrial), no gaps, no popping, density that matches the level.

### Difference
This is tile streaming, not a full proc-gen graph. 50s destroy vs speed 25 can remove tiles under the player if speed is boosted. No biome tile variants. No object pooling.

### Required modification
Extend `GroundSpawnner`: spawn offset from prefab bounds, destroy behind the player, optional pool. Do not replace with a new world generator.

### Existing files to modify
`GroundSpawnner.cs`, existing ground prefab.

### New files required
None for v1. Pool can live in the same script.

### Risk of modification
**High** for feel of all three levels. Tune against the existing 71-unit prefab; do not rebuild tiles.

---

## 11. Checkpoints

**Verdict:** MISSING

### Current implementation
No checkpoint component. Restart reloads the active scene (`RunStateManager.RestartRun`, `PauseMenu.Restart`). End tags (`EndLevel1`, `EndLevel2`, `EndLvl3End`) are finish gates, not mid-run saves.

### GDD requirement
Mid-run recovery so a long restoration run is not fully lost (typical endless-runner / story-runner hybrid).

### Difference
Death = full scene reload. Village progress is a display number, not a saved checkpoint.

### Required modification
This is a genuine gap. Prefer extending `RunStateManager` with a last-safe `z` / lane / resource snapshot, plus an existing empty trigger volume tagged as checkpoint — do not add a second game-state singleton.

### Existing files to modify
`RunStateManager.cs`, `HUDControls.cs` (resource snapshot), existing trigger objects if any.

### New files required
Possibly one small `CheckpointTrigger.cs` on existing trigger volumes. No new manager.

### Risk of modification
**Medium.** Must not fight `Time.timeScale` pause/death.

---

## 12. UI

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
- `HUDControls` + TMP text HUD on `MainGame` (Material, Village Progress, Health, Water, Bucket). Sliders optional / cleared in MainGame.
- `PauseMenu` + `PausePanel` / `VictoryPanel2`.
- `RunStateManager` shows pause/victory/death panels. **Death panel is not assigned** by bootstrapper (`SetupPanels(null, victory, pause)`).
- `StartMenu` on `StartScreen` (quit only; start-game delay is commented).
- `PipeControlslevel3` has a **separate** HUD (material, health, three tanks) that writes every frame.
- `Lanemanager2/3` toggle difficulty buttons on the HUD.

### GDD requirement
One readable HUD for health, hydration, resources, village restoration; pause; fail/win; mission goals.

### Difference
Two HUD owners (`HUDControls` vs `PipeControlslevel3`). Death UI incomplete. Victory is a panel, not a designed results beat. Debug logs still fire on every HUD refresh.

### Required modification
Keep `HUDControls` for L1/L2. Extend it for tanks **or** keep `PipeControlslevel3` as the L3 HUD but route death/victory through `RunStateManager`. Do not create `UIManager`. Wire death panel on existing canvas objects. Reduce log spam after verification.

### Existing files to modify
`HUDControls.cs`, `PipeControlslevel3.cs`, `RunStateManager.cs`, `SceneBootstrapper.cs`, `PauseMenu.cs`, `StartMenu.cs`, existing Canvas objects in the three level scenes.

### New files required
None.

### Risk of modification
**Medium.** Canvas references in YAML are easy to break.

---

## 13. Health

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
`HUDControls`: 0–100, random 3–15 damage/heal, death if health or player water ≤ 0.  
`PipeControlslevel3`: duplicate health, `DeathCheck` commented, `hasHit` limits updates.

### GDD requirement
A single health pool, readable on HUD, distinct from hydration, fail state when depleted.

### Difference
Two health pools. Damage is random, not per-hazard. Death panel may not show. Level 3 may not fail on 0 health.

### Required modification
Keep one health on `HUDControls` for runner levels. For L3, either reuse `HUDControls` or complete death in `PipeControlslevel3` via `RunStateManager.NotifyDeath()`. Tune ranges; do not add `HealthSystem`.

### Existing files to modify
`HUDControls.cs`, `PlayerHUDBase.cs`, `PipeControlslevel3.cs`, `RunStateManager.cs`.

### New files required
None.

### Risk of modification
**Low–medium.**

---

## 14. Stamina

**Verdict:** MISSING

### Current implementation
No stamina field, HUD, or drain. Speed is a flat `currentSpeed` / `playerSpeed`.

### GDD requirement
Stamina (or equivalent effort meter) gating sprint / slide / special actions so hydration and stamina are separate pressures.

### Difference
Only speed pickups exist.

### Required modification
New *resource*, but extend `HUDControls` + `PlayerController` rather than a new manager. Add TMP like the existing Health/Water lines. Skip until GDD confirms stamina vs hydration-only.

### Existing files to modify
`HUDControls.cs`, `PlayerController.cs`, HUD canvas in level scenes.

### New files required
None if folded into HUD. Do not add `StaminaManager`.

### Risk of modification
**Low** if unused until tuned. **Medium** if it changes jump/slide feel.

---

## 15. Hydration

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
Two water numbers in `HUDControls`:

- `playerWater` — personal hydration (`WaterDROP` +, `Heat&Disease` −). Death at 0.
- `waterLevel` — bucket / well fill (`DamWaterBUCK` +, drain in `WaterMoveManager` on speed events and lane-manager 5s tick).

Village percent is a **scene constant** (33.5 / 67 / 80), not driven by water delivered.

### GDD requirement
Hydration as a survival meter plus delivering water as the restoration verb (“drop by drop”).

### Difference
Fantasy is present. Drain is coupled to obstacle spawn timers. Bucket fill does not raise village %. Two water concepts are easy to confuse on HUD (“Water” vs “Bucket”).

### Required modification
Keep both meters (they match collect water vs fill well). Decouple drain from `Lanemanager*` spawn loops. Optionally tick village % from bucket at end-of-level only — extend `HUDControls.LevelProgress`, do not add `HydrationManager`.

### Existing files to modify
`HUDControls.cs`, `Lanemanager2.cs`, `Lanemanager3.cs`, `PlayerHUDBase.cs`.

### New files required
None.

### Risk of modification
**Medium.** Changing drain rate changes difficulty of all runner levels.

---

## 16. Resources (materials / village restoration)

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
`materialLevel` 0–100 via `SystemBuild()` on `Materials` tag (random 10–25). HUD text `Material: x/100`.  
Level complete requires health > 0, player water > 0, materials max, bucket max (`HUDControls.LevelProgress`).  
L3 uses materials to repair tanks (`TankMaterialDEC` exists but is not called from triggers).

### GDD requirement
Collect bricks/sand/materials and water to restore the village; visible restoration progress per biome.

### Difference
Materials exist. Village % is cosmetic. No crafting graph. `TankMaterialDEC` is unused. Mission UI in MainGame is static instruction text.

### Required modification
Extend `HUDControls` / `PipeControlslevel3` so restoration numbers are the win condition already used by `LevelProgress`. Wire `TankMaterialDEC` if repairs should cost materials. Do not add an inventory system.

### Existing files to modify
`HUDControls.cs`, `PipeControlslevel3.cs`, `PlayerHUDBase.cs`.

### New files required
None.

### Risk of modification
**Low–medium.**

---

## 17. Pipe puzzles

**Verdict:** PARTIALLY IMPLEMENTED

### Current implementation
`PipeControlslevel3.cs` + tags `PipeFix1/2/3`, `PipeFix`, `PipeHit`. Tanks +25 / +17 / +13 toward 100. Win if all tanks ≥ 100 (`LevelProgress` logs only; scene load commented). `hasHit` blocks after first trigger. Level 3 scenes contain many `PipeFix*` objects.

### GDD requirement
Industrial Zone climax: repair pipes/tanks as the restoration puzzle, not only dodge.

### Difference
Core loop exists. Puzzle is “run into tagged volumes,” not rotate/connect pipe pieces. Victory does not call `RunStateManager`. Duplicate Heat&Disease handling. Speed timer broken.

### Required modification
Complete this script: reset or per-collider `hasHit`, notify `RunStateManager` on win/fail, spend materials on repair if GDD wants cost. Do not replace with a new puzzle framework unless the GDD specifies actual pipe-piece rotation — that would be new and should wait for written GDD.

### Existing files to modify
`PipeControlslevel3.cs`, `RunStateManager.cs`, `Level3` / `Level3End` trigger objects (already tagged).

### New files required
None for completing the current tank-fill design.

### Risk of modification
**Medium.** `hasHit` change will make tanks fill faster (currently almost stuck after one hit).

---

## 18. Crafting

**Verdict:** MISSING

### Current implementation
No recipes. `SystemBuild` is a random material increment.

### GDD requirement
If the GDD includes crafting (tools, well parts), it is not in code.

### Difference
Materials are a score, not ingredients.

### Required modification
Only if the written GDD requires it. Then extend `HUDControls` material counts into a recipe check on an existing end-of-level or pause UI. Do not add `CraftingManager` until recipes exist on paper.

### Existing files to modify
`HUDControls.cs` (if recipes are simple).

### New files required
Only a data asset for recipes, if GDD specifies more than one craft.

### Risk of modification
**Low** if deferred. **High** if a full craft UI is invented without GDD.

---

## 19. Upgrades

**Verdict:** MISSING

### Current implementation
Temporary speed mods only. No persistent bucket size, jump height, or lane upgrades.

### GDD requirement
Meta progression between runs/levels (better container, cooler clothing, etc.) if the GDD includes it.

### Difference
Nothing persists between scenes except what is rebuilt in `Start`.

### Required modification
Depends on save (section 23). Extend `PlayerController` / `HUDControls` max values from a small persistent blob. Do not add `UpgradeManager` until save exists.

### Existing files to modify
`HUDControls.cs`, `PlayerController.cs`.

### New files required
None initially (PlayerPrefs keys on an existing bootstrapper/run state).

### Risk of modification
**Low** until persistence is real.

---

## 20. Skills

**Verdict:** MISSING

### Current implementation
No skill tree, no skill input beyond move/jump/speed.

### GDD requirement
Only if the GDD lists skills (dash, extra jump, water burst).

### Difference
Not in the project.

### Required modification
Do not invent. If GDD lists one or two verbs, add them to `PlayerController`.

### Existing files to modify
`PlayerController.cs`.

### New files required
None.

### Risk of modification
**Low** if skipped.

---

## 21. Bosses

**Verdict:** MISSING

### Current implementation
No boss actor, health, or phase. End of level is a tagged volume + resource check.

### GDD requirement
If the GDD has a drought / industrial boss, it is not built.

### Difference
Climax is restoration checklist, not a boss fight.

### Required modification
Do not add a boss until the GDD specifies one. Completing `LevelProgress` + victory panel is the existing climax — extend that first.

### Existing files to modify
`HUDControls.cs`, `PipeControlslevel3.cs`, `RunStateManager.cs` (victory).

### New files required
None for the current win condition.

### Risk of modification
**High** if a boss is invented; **low** if victory is completed on existing end triggers.

---

## 22. Scene transitions

**Verdict:** EXISTS BUT NEEDS REFINEMENT

### Current implementation
Hard `SceneManager.LoadScene` from:

- `PauseMenu`: `StartScreen`, `StarterInfor`, `Level2`, **`Level3`**
- `HUDControls.SceneChange`: `Level2`, **`Level3End`**
- `RunStateManager`: `mainMenuScene`, `nextScene` (`SceneBootstrapper` maps MainGame→Level2, Level2→**Level3End**)
- `EditorBuildSettings`: StartScreen, MainGame, Level2, **Level3End** (path). `Level3.unity` and `Level3End.unity.meta` share GUID `38edd36fe2e0b974fa3c29abd4c831d2`

End triggers call `LevelProgress` but **no longer auto-load** the next scene from `PlayerHUDBase` (scene change removed in Phase 1). Victory freezes time and shows `VictoryPanel2` if found.

### GDD requirement
Keep `MainGame` → `Level2` → `Level3` as Dry Bushlands → Mudlands → Industrial Zone. Do not rename those scenes.

### Difference
Code disagrees on Level 3’s scene name (`Level3` vs `Level3End`). Build list uses `Level3End`. Win does not always advance. `StarterInfor` may be missing from build.

### Required modification
Unify **load target** to the GDD name `Level3` **without deleting** `Level3End` until Unity GUID/path is verified in Editor. Point `SceneBootstrapper.NextSceneMap`, `HUDControls.SceneChange`, and build settings at the same file. After victory, `GoToNextScene` from the existing victory button.

### Existing files to modify
`SceneBootstrapper.cs`, `HUDControls.cs`, `PauseMenu.cs`, `RunStateManager.cs`, `ProjectSettings/EditorBuildSettings.asset` (path only), victory button OnClick in scenes.

### New files required
None. **Do not create new scenes.**

### Risk of modification
**High.** Duplicate GUID on `Level3` / `Level3End` can corrupt references. Resolve in Unity Editor, not by deleting YAML blindly.

---

## 23. Saving

**Verdict:** MISSING

### Current implementation
No `PlayerPrefs`, no save file, no DontDestroyOnLoad run payload. Village % is recomputed from scene name.

### GDD requirement
Persist unlocked levels, restoration %, upgrades, settings.

### Difference
Every Play starts from script defaults (health 100, materials 0, bucket 0).

### Required modification
Genuine gap. Smallest extension: persist on `RunStateManager` or `SceneBootstrapper` (unlocked scene name, village %). Do not add a second save service later.

### Existing files to modify
`RunStateManager.cs` or `SceneBootstrapper.cs`, `HUDControls.cs` (apply loaded village %).

### New files required
None for PlayerPrefs-scale save. A `SaveData` POCO in an existing file is enough.

### Risk of modification
**Low** for PlayerPrefs. **Medium** if mid-run checkpoint save is added (see §11).

---

## 24. Audio

**Verdict:** MISSING (as a game system)

### Current implementation
No `AudioSource` / clips under `Assets/Game`. Gameplay scripts play no SFX. Scenes have default `AudioListener` only. Third-party demo scenes have listeners; not the game loop.

### GDD requirement
Biome music, pickup/hit/jump/UI feedback, pause ducking.

### Difference
Silent gameplay.

### Required modification
Genuine gap. Add `AudioSource` on existing `gameManager` / Player and call it from `PlayerHUDBase` / `PlayerController` / `PauseMenu`. Do not add a large audio framework.

### Existing files to modify
`PlayerHUDBase.cs`, `PlayerController.cs`, `PauseMenu.cs`, `gameManager` or Canvas in existing scenes.

### New files required
Audio clips (assets), optionally one `GameAudio.cs` **only if** hooking many one-shots from existing scripts becomes messy. Prefer one source on `gameManager` first.

### Risk of modification
**Low.**

---

## Cross-cutting: Phase 1 runtime wiring

These are **not** GDD features, but they affect every system above.

| Piece | Role | Refinement note |
|---|---|---|
| `RunStateManager` | Playing / Paused / Dead / Victory | Keep. Wire death panel. Use for L3 win/fail. |
| `SceneBootstrapper` | Auto-adds controller + HUD wiring | Keep for L1/L2. Restrict on Level 3 if OG+pipe HUD is the live setup. |
| Duplicate movement | `PlayerController` vs `PlayerMovement` vs `PlayerMovementOG` | Do not delete. Disable unused on the Player object per scene. |

---

## Recommended refinement order (no implementation in this pass)

1. **Scene identity:** confirm in Editor which file is Industrial Zone (`Level3` vs `Level3End`) and align load paths **without** deleting scenes.  
2. **Stabilize runner:** keep `PlayerController`; lane-align `SpawnObjects`; fix `Spawner` if enabled.  
3. **HUD / fail / win:** death panel, `LevelProgress` → `RunStateManager`, reduce log spam.  
4. **Complete L3 pipes:** `hasHit`, victory, materials cost.  
5. **Slide** on `PlayerController` if GDD requires it for industrial hazards.  
6. **Hydration drain** moved off lane-manager timers.  
7. **Save + audio** as true missing systems, smallest possible extensions.  
8. Defer crafting, skills, bosses, stamina until a written GDD confirms them.

---

## Explicit non-actions (this phase)

- Do not rebuild `MainGame`, `Level2`, or `Level3`.  
- Do not replace `PlayerController`.  
- Do not add a second HUD manager, inventory, spawner, or save singleton.  
- Do not delete `PlayerMovement`, `PlayerMovementOG`, `Lanemanager2/3`, or `PipeControlslevel3`.  
- Do not implement the modifications listed above in this pass.

**Stop.** Implementation waits for approval of this gap analysis.
