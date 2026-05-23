using UnityEngine;

namespace UAPObservationMod
{
    public class UAPSpawnManager
    {
        private UAPManager manager;
        private float spawnTimer;

        public UAPSpawnManager(UAPManager manager)
        {
            this.manager = manager;
        }

        public void Tick(float deltaTime)
        {
            spawnTimer += deltaTime;

            // Throttle Activity Check (Engine firing actively)
            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.ctrlState.mainThrottle > 0.1f)
            {
                spawnTimer += deltaTime * 2f; // Timer accelerates drastically when flying actively
            }

            // Standard Background Spawns (Changed from 10s to a long 60s baseline timer)
            if (spawnTimer >= 60f)
            {
                spawnTimer = 0f;
                EvaluateSpawnConditions(manager.Settings.SpawnProbability);
            }
        }

        public void TriggerEventDrivenSpawn(float chanceOverride)
        {
            // Forces an immediate contextual check using high probability bounds upon dramatic gameplay events.
            EvaluateSpawnConditions(chanceOverride);
            spawnTimer = 0f; // Reset the tick timer so we don't naturally double-spawn right after
        }

        private void EvaluateSpawnConditions(float probabilityWeight)
        {
            if (FlightGlobals.ActiveVessel == null)
                return;

            if (TimeWarp.CurrentRate > 1f)
                return;

            if (manager.ActiveEntities.Count >= manager.Settings.MaxActiveUaps)
                return;

            float chance = UnityEngine.Random.value;
            // Weighted random spawn chance
            if (chance > probabilityWeight)
                return;

            SpawnRandomEntity();
        }

        private void SpawnRandomEntity()
        {
            Debug.Log("[UAPObservation] Spawning anomaly from pool.");
            
            UAPEntity entity = manager.Pool.GetEntity();

            // Phase 4: Pick a random behavior
            int behaviorRoll = UnityEngine.Random.Range(0, 5);
            switch (behaviorRoll)
            {
                case 0: entity.CurrentBehavior = new InterceptorBehavior(); break;
                case 1: entity.CurrentBehavior = new OrbiterBehavior(); break;
                case 2: entity.CurrentBehavior = new ShadowBehavior(); break;
                case 3: entity.CurrentBehavior = new BlinkBehavior(); break;
                default: entity.CurrentBehavior = new ObserverBehavior(); break;
            }
            
            // Smart Spawning Logic
            Vessel vessel = FlightGlobals.ActiveVessel;
            Vector3 vesselPos = vessel.transform.position;
            CelestialBody body = vessel.mainBody;
            
            // "Up" direction relative to the planet surface
            Vector3 upDir = (vesselPos - body.position).normalized;
            
            float spawnDist = manager.Settings.DefaultSpawnDistance;
            // 20% chance to spawn very far away (up to 50km)
            if (UnityEngine.Random.value < 0.2f)
                spawnDist = UnityEngine.Random.Range(15000f, 50000f);
            else
                spawnDist = UnityEngine.Random.Range(spawnDist * 0.5f, spawnDist * 2f);

            Vector3 spawnDir = upDir; // fallback
            for (int i = 0; i < 15; i++)
            {
                Vector3 candidateDir = UnityEngine.Random.onUnitSphere;
                
                // Bias upwards if inside the atmosphere
                if (vessel.atmDensity > 0)
                {
                    float dot = Vector3.Dot(candidateDir, upDir);
                    if (dot < 0.2f)
                    {
                        candidateDir = (candidateDir - 2 * dot * upDir).normalized;
                    }
                }
                
                Vector3 candidatePos = vesselPos + (candidateDir * spawnDist);
                if (!IsOccludedByBody(vesselPos, candidatePos, body))
                {
                    spawnDir = candidateDir;
                    break;
                }
            }

            entity.Transform.position = vesselPos + (spawnDir * spawnDist);
            entity.Reset();
            entity.CurrentBehavior.Initialize(entity);
            manager.RegisterSpawnedEntity(entity);
            
            // Notify persistent scenario module about the sighting
            if (UAPScenarioModule.Instance != null)
            {
                UAPScenarioModule.Instance.RecordSighting();
            }
        }

        private bool IsOccludedByBody(Vector3 start, Vector3 end, CelestialBody body)
        {
            Vector3 bodyCenter = body.position;
            float bodyRadius = (float)body.Radius;
            
            Vector3 lineDir = (end - start);
            float lineLen = lineDir.magnitude;
            lineDir.Normalize();
            
            Vector3 vStartToBody = bodyCenter - start;
            float t = Vector3.Dot(vStartToBody, lineDir);
            
            // If the body is fully behind us or beyond the end point, not occluded
            if (t < 0 || t > lineLen)
            {
                return false; 
            }
            
            Vector3 closestPoint = start + lineDir * t;
            float distToCenter = Vector3.Distance(closestPoint, bodyCenter);
            
            // Require it to clear the body surface and a 500m atmospheric/ground buffer
            return distToCenter < (bodyRadius + 500f);
        }
    }
}