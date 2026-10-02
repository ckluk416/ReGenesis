# Prototype Gameplay Setup

This page explains how to wire the first-playable gameplay scripts into `Assets/Scenes/Prototype_Scene01.unity`. It covers sections 7 to 13 of the [Beginner Implementation Guide](UNITY_BEGINNER_IMPLEMENTATION_GUIDE.md).

All scripts live in the `ReGenesis` namespace under `Assets/Scripts`.

## Script overview

| Script | Folder | Purpose |
|---|---|---|
| `GameManager` | Core | Game states (Preparing, WaveRunning, Victory, Defeat, Paused), pause, restart |
| `EnergyBank` | Core | Energy Resource balance used to build towers |
| `IDamageable` | Core | Shared damage interface |
| `UIPointer` | Core | Stops world clicks from passing through the HUD |
| `EnemyPath` | Enemies | Ordered waypoints with Scene view gizmos |
| `EnemyMovement` | Enemies | Follows the path and damages the Energy Core on arrival |
| `EnemyHealth` | Enemies | Health, death, energy reward, list of living enemies |
| `EnergyCoreHealth` | Gameplay | Core health and the defeat trigger |
| `WaveDefinition` | Gameplay | ScriptableObject describing one wave |
| `WaveSpawner` | Gameplay | Runs waves and reports when each is cleared |
| `Projectile` | Gameplay | Homing projectile with a limited lifetime |
| `BuildManager` | Gameplay/BuildSystem | Tower selection, slot limit, build and demolish |
| `PlacementNode` | Gameplay/BuildSystem | Clickable build spot with state colors |
| `TowerDefinition` | Towers | ScriptableObject with tower name, prefab, cost and refund |
| `TowerTargeting` | Towers | Picks the enemy furthest along the path within range |
| `SolarTurret` | Towers | Rotates its head and fires projectiles |
| `PrototypeHUD` | UI | Placeholder HUD that needs no Canvas or UI package |

## Default values

| Setting | Value |
|---|---|
| Energy Core health | 100 |
| Starting energy | 100 |
| Tower slots (Stage 1) | 4 |
| Solar Turret cost | 50 energy, 50 percent refund on demolition |
| Solar Turret range, rate, damage | 5 units, 1 shot per second, 10 damage |
| Malware Drone health, speed | 30, 2 units per second |
| Damage to core per enemy | 10 |
| Energy reward per kill | 5 |
| Prototype wave | 5 drones, 1.5 seconds apart, 2 second start delay |

All values are editable in the Inspector or on the data assets.

## 1. Systems

1. Select `_Systems` and create these empty children: `GameManager`, `EnergyBank`, `BuildManager`, `WaveSpawner`, `HUD`.
2. Add the component with the matching name to each one. Add `PrototypeHUD` to `HUD`.

## 2. Path

1. Select `EnemyPath/Waypoints` and add `EnemyPath`. The waypoint list fills itself from `WP_00` to `WP_05`.
2. If the list is empty or out of order, right click the component header and choose **Collect Child Waypoints**.
3. An orange line in the Scene view shows the route.

## 3. Energy Core

1. Select the `EnergyCore` object under `Gameplay` and add `EnergyCoreHealth`.

## 4. Placement nodes

For each placement node root:

1. Make sure the root has a Box Collider. Mouse clicks only reach `PlacementNode` when the collider is on the same GameObject.
2. Check `PlacementNode`. Its **Build Point** field should point to the `BuildPoint` child.
3. Optionally assign **Highlight Renderer** to the status ring renderer. Otherwise the first renderer in the children is tinted.

## 5. Malware Drone prefab

1. Create `Assets/Prefabs/Enemies/PF_Enemy_MalwareDrone.prefab` as described in section 6 of the guide.
2. Add `EnemyMovement` and `EnemyHealth` to the prefab root.
3. Set **Height Offset** on `EnemyMovement` if the drone should hover above the road.
4. Optionally assign **Aim Point** on `EnemyHealth` to a child at the drone's center.

## 6. Solar Turret prefab

1. Create `Assets/Prefabs/Towers/PF_Tower_Solar_L1.prefab`.
2. Add `SolarTurret`. This also adds `TowerTargeting`.
3. Assign **Rotating Head** to the yaw pivot and **Muzzle Point** to the barrel tip.
4. If the barrel points the wrong way while aiming, adjust **Head Yaw Offset** in steps of 90 degrees.
5. **Projectile Prefab** is optional. Without one, the turret fires small placeholder spheres.

## 7. Data assets

1. In `Assets/Data/Towers`, choose **Create > ReGenesis > Tower Definition**. Name it `Tower_SolarTurret` and assign `PF_Tower_Solar_L1` as its prefab.
2. In `Assets/Data/Waves`, choose **Create > ReGenesis > Wave Definition**. Name it `Wave_01`, add one spawn group and assign `PF_Enemy_MalwareDrone`.

## 8. Connect the systems

| Object | Field | Value |
|---|---|---|
| `WaveSpawner` | Spawn Point | `EnemyPath/EnemySpawnPoint` |
| `WaveSpawner` | Path | `EnemyPath/Waypoints` |
| `WaveSpawner` | Waves | `Wave_01` |
| `GameManager` | Energy Core | `EnergyCore` |
| `GameManager` | Wave Spawner | `WaveSpawner` |
| `HUD` | Buildable Towers | `Tower_SolarTurret` |
| `HUD` | Wave Spawner | `WaveSpawner` |

## 9. Build profile

Add the scene to **File > Build Profiles > Scene List**. The Restart button reloads the active scene by its build index.

## Controls

| Input | Action |
|---|---|
| HUD tower button | Select a tower |
| Left click an empty node | Build the selected tower |
| Escape or right click | Cancel the selection |
| Right click a tower node with nothing selected | Demolish the tower for a 50 percent refund |
| Start Wave button | Start the next wave |
| P | Pause or resume |

## Playtest checklist

- Clicking a node with no tower selected builds nothing.
- Building costs 50 energy, and a second tower on the same node is refused.
- Drones spawn 1.5 seconds apart and follow every waypoint.
- The turret ignores drones out of range, then turns and fires.
- Three hits destroy a drone and award 5 energy.
- A drone that reaches the core removes 10 core health once.
- Clearing every wave shows **SYSTEM RESTORED**. Core health at zero shows **CORE CORRUPTED**.
- Restart returns the scene to its initial state.

## Later replacements

- Replace `PrototypeHUD` with a Canvas HUD after adding the `com.unity.ugui` package. The HUD should set `UIPointer.IsOverUI` from the EventSystem so node clicks stay blocked.
- Replace the hard-coded `Pollution 0%` label once the pollution system exists.
