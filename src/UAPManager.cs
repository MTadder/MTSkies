using System.Collections.Generic;
using UnityEngine;

namespace MTSkies
{
    public class UAPManager : MonoBehaviour
    {
        public List<UAPEntity> ActiveEntities => activeEntities;
        private readonly List<UAPEntity> activeEntities = new List<UAPEntity>();

        public UAPSettings Settings { get; private set; }
        public UAPPoolManager Pool { get; private set; }
        public UAPSpawnManager SpawnManager { get; private set; }
        public SensorCameraController SensorCamera { get; private set; }
        public UAPWindow ModWindow { get; private set; }

        private void Awake()
        {
            Settings = new UAPSettings();
            Settings.Load();
            
            Pool = new UAPPoolManager(this);
            SpawnManager = new UAPSpawnManager(this);

            // GUI Window
            ModWindow = gameObject.AddComponent<UAPWindow>();
            ModWindow.Initialize(this);

            // Phase 3: Sensor Camera Controller
            GameObject camObj = new GameObject("OBS_SensorCamera");
            camObj.transform.SetParent(this.transform);
            SensorCamera = camObj.AddComponent<SensorCameraController>();
            SensorCamera.Initialize(this);

            // Phase 5: Audio Hook
            SensorAudioFx audioFx = camObj.AddComponent<SensorAudioFx>();
            audioFx.Initialize(SensorCamera, this);

            // Phase 2: GameEvent Hooks
            GameEvents.onGameSceneLoadRequested.Add(OnSceneChanged);
            GameEvents.onVesselChange.Add(OnVesselChanged);
            
            // Phase 4: Event-driven adaptations
            GameEvents.onStageActivate.Add(OnStageActivate);
            GameEvents.onCrash.Add(OnCrash);
            GameEvents.onCrewKilled.Add(OnCrewKilled);
        }

        private void Start()
        {
            Debug.Log("[MTSkies] UAPManager started.");
        }

        private void Update()
        {
            float dt = TimeWarp.deltaTime; // Use TimeWarp.deltaTime to scale with time warp

            SpawnManager.Tick(dt);

            for (int i = activeEntities.Count - 1; i >= 0; i--)
            {
                UAPEntity entity = activeEntities[i];
                entity.Tick(dt);

                // Prevent despawn if the player is actively tracking it
                bool isTracked = SensorCamera != null && SensorCamera.IsActive && SensorCamera.LockedTarget == entity;

                // Despawn if lifetime exceeded or extreme distance, but not when tracking
                if (!isTracked && (entity.IsExpired || IsTooFar(entity)))
                {
                    DespawnEntity(entity);
                }
            }
        }

        private bool IsTooFar(UAPEntity entity)
        {
            if (FlightGlobals.ActiveVessel == null) return true;
            return Vector3.Distance(entity.Transform.position, FlightGlobals.ActiveVessel.transform.position) > Settings.MaxDespawnDistance; // Replaces hardcoded 100km
        }

        public void RegisterSpawnedEntity(UAPEntity entity)
        {
            activeEntities.Add(entity);
        }

        public void DespawnEntity(UAPEntity entity)
        {
            activeEntities.Remove(entity);
            Pool.ReturnEntity(entity);
        }

        public UAPEntity GetClosestEntity()
        {
            if (activeEntities.Count == 0 || FlightGlobals.ActiveVessel == null) return null;

            UAPEntity closest = null;
            float minDistance = float.MaxValue;
            Vector3 vesselPos = FlightGlobals.ActiveVessel.transform.position;

            foreach (var entity in activeEntities)
            {
                float dist = Vector3.Distance(entity.Transform.position, vesselPos);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = entity;
                }
            }

            return closest;
        }

        private void OnVesselChanged(Vessel vessel)
        {
            Debug.Log("[MTSkies] Active vessel changed.");
            // Behavior could evaluate and despawn if the new vessel is invalid 
            // Currently our active ones will just retarget it locally next Tick()
        }

        private void OnSceneChanged(GameScenes scene)
        {
            Debug.Log("[MTSkies] Scene transition begun. Cleaning up logic.");
            Cleanup();
        }

        private void Cleanup()
        {
            // Reverse loop because we might be removing from the list
            for (int i = activeEntities.Count - 1; i >= 0; i--)
            {
                Pool.ReturnEntity(activeEntities[i]);
            }
            activeEntities.Clear();
            Pool.Clear();
        }

        private void OnDestroy()
        {
            GameEvents.onGameSceneLoadRequested.Remove(OnSceneChanged);
            GameEvents.onVesselChange.Remove(OnVesselChanged);
            GameEvents.onStageActivate.Remove(OnStageActivate);
            GameEvents.onCrash.Remove(OnCrash);
            GameEvents.onCrewKilled.Remove(OnCrewKilled);
            Cleanup();
        }

        private void OnStageActivate(int stage)
        {
            SpawnManager?.TriggerEventDrivenSpawn(0.2f);
        }

        private void OnCrash(EventReport data)
        {
            SpawnManager?.TriggerEventDrivenSpawn(0.6f);
            TriggerFleeBehavior();
        }

        private void OnCrewKilled(EventReport data)
        {
            if (SpawnManager != null) SpawnManager.TriggerEventDrivenSpawn(0.8f);
            TriggerFleeBehavior();
        }
        
        private void TriggerFleeBehavior()
        {
            foreach (var entity in activeEntities)
            {
                entity.CurrentBehavior?.Shutdown(entity);
                entity.CurrentBehavior = new FleeBehavior();
                entity.CurrentBehavior.Initialize(entity);
            }
        }
    }
}
