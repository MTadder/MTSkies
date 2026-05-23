# KSP UAP Observation Mod — Design Document

---

# PART I — CORE DESIGN (Sections 1–10)

## 1. Overview

**Working Title:** UAP Observation System

**Game:** Kerbal Space Program 1 (latest stable steam release)

**Goal:** Add lightweight, non-physical unidentified aerial phenomena to the game that behave like distant observed objects. The mod focuses on visual deception, procedural movement, and camera-based observation rather than full physics simulation.

**Primary Experience:**
- Player spots fast-moving luminous objects at distance.
- Objects appear to track, observe, evade, or reposition.
- A separate tracking camera can mimic declassified sensor footage.
- The mod emphasizes atmosphere, uncertainty, and visual realism.
- Players can attempt to photograph the UAP for rewards.
 - Taking images of the UAP can give the player funds and science points - more if the player can get a good shot of it (high contrast, well-framed).
 - The player can also attempt to track the UAP with the sensor camera for additional rewards over time.
- The UAP will not interact with the player's vessel or parts, but may react to the player's presence and actions in a way that feels intentional and dynamic.
- The UAP should feel like a real, physical object in the world, but without the overhead of physics simulation or complex part models.
- The mod should create memorable moments of awe and mystery, rather than combat or direct interaction.
- The UAP should be a rare and special occurrence, not a constant presence. It should feel like a unique event when it appears, and players should be encouraged to stop and observe rather than ignore it.
- The UAP should be designed to be visually striking and easily identifiable, even at long distances. It should have a strong silhouette and bright emissive materials that make it stand out against the sky or space background.
- The UAP should have a variety of movement behaviors that make it feel intelligent and unpredictable, such as tracking the player, evading, orbiting, or blinking in and out of visibility.
- The sensor camera mode should provide a fun and immersive way for players to engage with the UAP, allowing them to track it, zoom in for a closer look, and capture footage that feels like it could be from a real-world sighting.
- The mod should be modular and extensible, allowing for new types of UAPs, behaviors, and visual effects to be added in the future without major refactoring.
- The mod should be designed with performance in mind, ensuring that it does not cause significant slowdowns or memory issues, even on lower-end systems. This means using techniques like object pooling, minimizing draw calls, and avoiding expensive physics simulations.
- The mod should be compatible with other popular mods and not interfere with core gameplay mechanics, such as vessel physics, map view, or scene transitions.
- The mod should be configurable, allowing players to adjust settings such as spawn frequency, visual effects intensity, and camera behavior to suit their preferences.
- UAP should appear in a variety of contexts, such as during atmospheric flight, orbital maneuvers, or even deep space exploration, to create a sense of ubiquity and mystery.
- Should be very rare, but should also be able to be "summoned" for player enjoyment.
- The mod should include a persistence system to track sightings and player interactions with the UAP over time, allowing for potential rewards or achievements based on player engagement.
- The mod should include a variety of visual and audio effects to enhance the atmosphere and immersion of UAP sightings, such as subtle noise, distortion, or interference effects when the UAP is nearby or being tracked.
 - The UAP could appear to "drip" or leave behind a faint trail of particles or energy as it moves, adding to the visual spectacle and making it easier for players to track its movement.
 - The UAP could also have a subtle audio presence, such as a faint hum or static that becomes more pronounced as the player gets closer or successfully tracks it with the sensor camera, further enhancing the sense of realism and immersion.

---

## 2. Design Goals

1. **Low runtime cost**
   - No physics bodies for the anomalous objects.
   - No part trees for the objects themselves.
   - Minimal draw calls and low GC pressure.

2. **Believable motion**
   - Smooth acceleration and deceleration.
   - Camera-aware behavior.
   - Occasional sudden repositioning or loss of lock.

3. **Strong visual identity**
   - Bright emissive bodies.
   - Bloom-friendly rendering.
   - Optional sensor-style camera mode.

4. **Modularity**
   - Spawning, behavior, rendering, and UI should be separated.
   - Easy to add new anomaly types later.

5. **Compatibility**
   - Avoid interference with vessel physics, map view, and scene transitions.
   - Keep behavior isolated to flight scenes.

---

## 3. Non-Goals

- No full aerodynamic simulation.
- No collision-based interactions.
- No combat system.
- No required custom vessel parts.
- No dependence on heavy post-processing frameworks unless optional.

---

## 4. Player-Facing Features

### 4.1 Anomaly Spawning
- Random or condition-based UAP appearances.
- Distant sightings near the active vessel.
- Rare, unpredictable, and configurable.

### 4.2 Behavior Types
- Observer: stays at distance and follows.
- Interceptor: closes in quickly, then vanishes.
- Orbiter: circles the player or target vessel.
- Shadow: trails behind and matches motion.
- Blink: intermittent visibility with short reposition jumps.
- Swarm: multiple coordinated objects.

### 4.3 Sensor Camera Mode
- Secondary camera that simulates a tracking platform.
- Zoom control.
- Smooth tracking motion.
- Stabilization jitter.
- HUD overlay with bearing, range, lock state, and zoom level.

### 4.4 Visual Effects
- Emissive glow.
- Additive bloom-friendly shader.
- Optional noise, blur, and compression artifacts.
- Fading at long range.

---

## 5. Technical Architecture

```text
[KSP GameEvents]
        ↓
[Scenario / Bootstrap]
        ↓
[UAP Manager]
        ├── Spawn Manager
        ├── Pool Manager
        ├── Behavior Scheduler
        ├── Sensor Camera Controller
        └── Persistence System
                ↓
         [UAP Entity Instances]
                ↓
         [Renderer + Shader]
```

---

## 6. Runtime Systems

### 6.1 Bootstrap System
Responsible for initializing the mod in flight scenes, registering events, and creating the global manager.

### 6.2 UAP Manager
Central controller that owns:
- active anomaly list,
- inactive pool,
- global spawn rules,
- scene cleanup,
- configuration data.

### 6.3 Spawn Manager
Decides when and where anomalies appear.

**Inputs:**
- vessel position,
- altitude,
- time of day,
- scene state,
- probability tables,
- biome or celestial body rules.

**Outputs:**
- spawn position,
- anomaly type,
- initial behavior,
- initial visibility state.

### 6.4 Behavior Controller
Updates movement and tracking logic for each active anomaly.

### 6.5 Render Controller
Handles object presentation:
- billboard or low-poly mesh,
- emissive material,
- glow intensity,
- optional camera-facing orientation.

### 6.6 Sensor Camera Controller
A separate camera rig used to emulate recorded footage.

### 6.7 Persistence
Stores settings and optional sighting history across sessions.

---

## 7. GameEvents Integration

Use KSP events to synchronize the mod with the game state.

### Suggested Events
- `GameEvents.onFlightReady` — initialize flight-scene systems.
- `GameEvents.onVesselChange` — retarget the active vessel.
- `GameEvents.onGameSceneLoadRequested` — cleanup and shutdown.
- `GameEvents.onVesselOrbitChanged` — optionally adjust spawn rules.
- `GameEvents.onCrash` — trigger emergency behavior or vanish.
- `GameEvents.onPartExplode` — optionally react to dramatic events.

### Event Handling Rules
- Register only in flight scenes.
- Unregister on scene exit.
- Avoid duplicated listeners.

---

## 8. UAP Entity Model

Each anomaly should be a lightweight entity with no physical simulation.

### Entity Properties
- Transform
- Renderer
- Material
- Behavior type
- Lifetime timer
- Visibility state
- Target reference
- Optional audio emitter

### Entity Lifecycle
1. Spawned from pool.
2. Initialized with behavior and position.
3. Updated at a fixed or staggered cadence.
4. Despawned or recycled.

---

## 9. Behavior System

### Behavior Interface
```csharp
public interface IUAPBehavior
{
    void Initialize(UAPEntity entity);
    void Tick(UAPEntity entity, float deltaTime);
    void Shutdown(UAPEntity entity);
}
```

### Behavior Examples

#### Observer
- Maintains standoff distance.
- Reorients toward the player.
- Adjusts speed based on visibility.

#### Interceptor
- Approaches rapidly.
- Overshoots slightly.
- Breaks away or disappears.

#### Orbiter
- Circles a target point.
- Changes radius slowly.
- Uses smooth curvature.

#### Blink
- Turns invisible for short intervals.
- Relocates during invisibility.
- Reappears at a new bearing.

#### Shadow
- Trails active vessel movement.
- Matches speed changes with delay.

---

## 10. Spawn Rules

### Suggested Spawn Conditions
- Flight scenes only.
- Prefer atmospheric or orbital gameplay.
- Higher chance at night.
- Lower chance during map view.
- Reduced or disabled during timewarp.

### Suggested Spawn Distances
- Atmospheric: 500 m to 3000 m.
- Orbital: 1000 m to 10 km.
- Deep space: 5 km to 50 km.

### Spawn Logic
- Evaluate interval-based checks instead of per-frame rolls.
- Use weighted randomness.
- Bias toward distant side or rear quadrants.
- Avoid spawning directly inside the camera frustum every time.

---

---

# PART II — CAMERA, RENDERING, AND PERFORMANCE (Sections 11–20)

## 11. Sensor Camera Mode

A second camera can emulate release footage without interfering with the main gameplay camera.

### Camera Rig
```text
TrackingRig
├── Yaw Pivot
│   └── Pitch Pivot
│       └── Sensor Camera
```

### Camera Behavior
- Smooth tracking.
- Stepped zoom.
- Small rotational lag.
- Subtle jitter.
- Optional lock failure.

### UI Elements
- `REC` indicator
- bearing display
- elevation display
- zoom level
- lock state
- signal quality indicator

---

## 12. Rendering Approach

### Preferred Visual Method
- Low-poly mesh or billboard quad.
- Unlit additive shader.
- Bright emissive texture.
- Optional pulse animation.

### Why This Approach
- Low cost.
- Simple to author.
- Easy to scale.
- Can be made visually convincing with minimal geometry.

### Optional Enhancements
- Fresnel glow.
- Distance fade.
- Noise distortion.
- Motion streak illusion.
- HDR intensity tuning.

---

## 13. Performance Strategy

### Rules
- No Rigidbody.
- No Collider unless explicitly required.
- No per-frame object creation.
- No repeated Instantiate/Destroy loops.
- Use object pooling.
- Update AI at reduced frequency when possible.

### Performance Targets
- Active anomalies: 1–5 typical.
- Draw calls: as low as practical.
- GC allocations: near zero per frame.
- Behavior updates: staggered, not synchronized.

---

## 14. Data and Configuration

Use `ConfigNode` or similar config-driven settings.

### Configurable Values
- global spawn chance,
- per-body spawn weights,
- behavior weights,
- maximum active anomalies,
- camera sensitivity,
- zoom ranges,
- HUD opacity,
- glow intensity,
- audio intensity,
- persistence toggles.

---

## 15. Persistence

Store optional state in a scenario module.

### Persisted Data
- sightings count,
- enabled anomaly types,
- global difficulty or intensity,
- last known player settings,
- unlocked camera modes.

### Do Not Persist
- transient per-frame positions,
- temporary rendering state,
- live pool contents.

---

## 16. Audio Design

Optional audio should be subtle and limited.

### Audio Types
- servo hum,
- radio static,
- short lock tone,
- faint broadband hiss,
- brief interference bursts.

### Rules
- Never overpower vessel sound.
- Keep short loops and low-volume cues.
- Tie audio to sensor state and visibility.

---

## 17. UI / UX

### Main Objectives
- Keep the effect readable.
- Avoid clutter.
- Make the tracking system feel mechanical.

### HUD Guidelines
- Minimal text.
- Small central reticle.
- Optional diagnostic panel.
- Use low-opacity overlays.

### Accessibility
- Toggle sensor mode.
- Toggle screen shake.
- Toggle HUD.
- Toggle anomaly frequency.
- Toggle audio artifacts.

---

## 18. File and Folder Layout

```text
GameData/
└── UAPObservationMod/
    ├── Plugins/
    │   └── UAPObservationMod.dll
    ├── Assets/
    │   ├── uap_glow.png
    │   ├── sensor_hud.png
    │   └── uap_model.mu
    ├── Shaders/
    │   └── UAPGlow.shader
    ├── Config/
    │   ├── settings.cfg
    │   └── anomalies.cfg
    └── Localization/
        └── en-us.cfg
```

---

## 19. Minimal C# Class Layout

### Core Classes

```csharp
UAPBootstrap
UAPManager
UAPSpawnManager
UAPPoolManager
UAPEntity
IUAPBehavior
ObserverBehavior
InterceptorBehavior
OrbiterBehavior
BlinkBehavior
SensorCameraController
UAPScenarioModule
```

### Responsibilities Summary

| Class | Responsibility |
|---|---|
| `UAPBootstrap` | Scene startup and shutdown |
| `UAPManager` | Global orchestration |
| `UAPSpawnManager` | Spawn rules and creation |
| `UAPPoolManager` | Reuse of entity objects |
| `UAPEntity` | Active anomaly instance |
| `IUAPBehavior` | Shared behavior contract |
| `SensorCameraController` | Tracking camera and zoom |
| `UAPScenarioModule` | Persistence and scenario data |

---

## 20. Implementation Phases

### Phase 1 — Prototype
- Bootstrap mod in flight scenes.
- Spawn one simple emissive object.
- Move it with basic tracking behavior.

### Phase 2 — Pooling and Events
- Add GameEvent hooks.
- Add object pooling.
- Add cleanup on scene transitions.

### Phase 3 — Camera System
- Add separate sensor camera.
- Add zoom and tracking smoothing.
- Add HUD overlay.

### Phase 4 — Behavior Variety
- Add multiple anomaly behaviors.
- Add visibility logic and lock loss.
- Add spawn weighting.

### Phase 5 — Polish
- Add audio.
- Add shader tuning.
- Add config files and persistence.
- Add optional user settings.

---

---

# PART III — IMPLEMENTATION, TESTING, AND EXPANSION (Sections 21–35)

## 21. Risks and Mitigations

### Risk: Camera interference
**Mitigation:** Keep the sensor camera isolated from the main flight camera.

### Risk: Performance spikes
**Mitigation:** Pool entities and reduce AI update frequency.

### Risk: Visual ugliness at distance
**Mitigation:** Use strong emissive shaping, bloom-friendly materials, and distance fading.

### Risk: Behavior feels random rather than intentional
**Mitigation:** Use weighted, rule-based movement instead of pure randomness.

---

## 22. Definition of Done

The mod is complete when:
- anomalies spawn reliably in flight scenes,
- entities use no physics simulation,
- camera mode can track and zoom smoothly,
- HUD feedback is readable,
- entities can be pooled and despawned cleanly,
- settings persist correctly,
- performance remains stable during normal gameplay.

---

## 23. Next Technical Deliverables

1. Minimal `KSPAddon` bootstrap script.
2. `UAPManager` singleton.
3. `UAPSpawnManager` with weighted spawn logic.
4. `SensorCameraController` with zoom and smoothing.
5. Additive unlit shader for the anomaly object.
6. `ScenarioModule` for config persistence.

---

## 24. Example Update Loop

```csharp
public class UAPManager : MonoBehaviour
{
    private readonly List<UAPEntity> activeEntities =
        new List<UAPEntity>();

    private float spawnTimer;

    private void Update()
    {
        float dt = Time.deltaTime;

        spawnTimer += dt;

        if (spawnTimer >= 10f)
        {
            spawnTimer = 0f;
            EvaluateSpawnConditions();
        }

        for (int i = 0; i < activeEntities.Count; i++)
        {
            activeEntities[i].Tick(dt);
        }
    }
}
```

---

## 25. Example Spawn Evaluation

```csharp
private void EvaluateSpawnConditions()
{
    if (FlightGlobals.ActiveVessel == null)
        return;

    if (TimeWarp.CurrentRate > 1f)
        return;

    if (activeEntities.Count >= MaxActiveUaps)
        return;

    float chance = UnityEngine.Random.value;

    if (chance > SpawnProbability)
        return;

    SpawnRandomEntity();
}
```

---

## 26. Example Entity Construction

```csharp
public UAPEntity CreateEntity()
{
    GameObject root =
        new GameObject("UAP_Entity");

    MeshFilter mf =
        root.AddComponent<MeshFilter>();

    MeshRenderer mr =
        root.AddComponent<MeshRenderer>();

    mr.material = UAPMaterials.DefaultGlow;

    return new UAPEntity
    {
        Root = root,
        Renderer = mr,
        Transform = root.transform
    };
}
```

---

## 27. Suggested Shader Parameters

| Parameter | Purpose |
|---|---|
| `_EmissionIntensity` | Glow brightness |
| `_PulseSpeed` | Pulse animation speed |
| `_NoiseStrength` | Surface instability |
| `_EdgeFade` | Soft silhouette fading |
| `_DistanceFade` | Long-range visibility control |
| `_FresnelPower` | Rim lighting effect |
| `_VelocityStretch` | Motion smear effect |

---

## 28. Sensor Camera Recording System

### Purpose
Allow players to replay sightings as archived footage.

### Recommended Data Capture

| Data | Frequency |
|---|---|
| Camera transform | Every frame |
| UAP transform | Every frame |
| Zoom level | On change |
| Lock state | On change |
| HUD state | On change |

### Playback Features
- pause,
- rewind,
- free camera,
- export screenshots,
- cinematic replay.

---

## 29. Future Expansion Possibilities

### Potential Features
- radar spoofing,
- atmospheric ionization effects,
- vessel instrument interference,
- procedural audio synthesis,
- synchronized swarm intelligence,
- orbital anomaly encounters,
- hidden lore transmissions,
- contract integration.

---

## 30. Mod Compatibility Notes

### Recommended Compatibility Targets

| Mod | Integration Idea |
|---|---|
| `SCANsat` | Fake anomaly tracks |
| `RasterPropMonitor` | Sensor camera feeds |
| `Kerbalism` | Signal interference |
| `Environmental Visual Enhancements` | Better atmospheric visuals |
| `TUFX` | Sensor post-processing presets |

### Stability Rules
- Avoid patching stock flight camera directly.
- Avoid modifying vessel physics.
- Keep all anomaly systems isolated.
- Fail safely during scene transitions.

---

## 31. Development Environment Setup

### Recommended Tools

| Tool | Purpose |
|---|---|
| Visual Studio 2022 | Main IDE |
| dnSpy / ILSpy | KSP assembly inspection |
| Unity Asset Bundle Extractor | Asset debugging |
| ModuleManager | Config integration |
| Git | Version control |

### Build Targets
- .NET Framework 4.x
- x64 Windows
- KSP 1.x compatible assemblies

---

## 32. Testing Plan

### Functional Tests
- Spawn/despawn validation.
- Scene transition cleanup.
- Multiple vessel switching.
- Camera stability during high acceleration.
- Long-duration memory leak testing.

### Performance Tests
- 5 simultaneous active anomalies.
- High part-count vessels.
- Atmospheric launch stress test.
- Orbital tracking stress test.
- Extended gameplay sessions.

### Visual Validation
- Night visibility.
- Daylight visibility.
- High-altitude bloom.
- Distance fade quality.
- Sensor HUD readability.

---

## 33. Example Config File

```cfg
UAP_SETTINGS
{
    MaxActiveUaps = 3

    SpawnProbability = 0.15

    EnableSensorCamera = true

    EnableAudioArtifacts = true

    EnableScreenNoise = true

    DefaultGlowIntensity = 4.0

    DefaultSpawnDistance = 2500
}
```

---

## 34. Naming Conventions

### Prefixes

| Prefix | Meaning |
|---|---|
| `UAP_` | Runtime systems |
| `OBS_` | Sensor camera systems |
| `FX_` | Visual effects |
| `CFG_` | Configuration systems |

### Example Object Names

```text
UAP_Manager
UAP_Entity_01
OBS_SensorCamera
FX_GlowPulse
```

---

## 35. Final Notes

The effectiveness of the mod depends more on:
- timing,
- movement language,
- stabilization behavior,
- and visual framing

than on raw graphical complexity.

Small luminous objects with intelligent movement and convincing camera behavior will appear significantly more believable than highly detailed physical craft.

The architecture should therefore prioritize:
- stable runtime behavior,
- visual consistency,
- low overhead,
- and modular extensibility.

