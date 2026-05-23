using UnityEngine;

namespace MTSkies
{
    /// <summary>
    /// Mimics the Gimbal / GoFast videos. The anomaly perfectly locks onto the vessel's 
    /// velocity vector and paces it parallel, rotating freely against aerodynamic logic.
    /// </summary>
    public class MirrorBehavior : IUAPBehavior
    {
        private Vector3 formationOffset;
        private float rollAngle = 0f;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[MTSkies] MirrorBehavior Initialized.");
            // Pick a fixed coordinate in local vessel space
            float dist = UnityEngine.Random.Range(1000f, 3000f);
            formationOffset = UnityEngine.Random.onUnitSphere * dist;
            // Elevate slightly so it's usually visible in horizon
            formationOffset.y = Mathf.Abs(formationOffset.y);
            rollAngle = 0f;
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            Vessel v = FlightGlobals.ActiveVessel;
            if (v == null) return;
            
            // Treat the offset as a strict delta from the craft, effectively inheriting 100% of its velocity
            Vector3 desiredPosition = v.transform.position + formationOffset;
            
            // Hard lock the position. We don't lerp the pacing ship. It possesses identical velocity.
            entity.Transform.position = Vector3.Lerp(entity.Transform.position, desiredPosition, 1f - Mathf.Exp(-deltaTime * 10f));
            
            // Eerie rotation: Gimbal video style rotation against the wind
            rollAngle += deltaTime * 15f; 
            
            // Constantly stare at the vessel but gradually roll on its own Z axis
            Vector3 relativePos = v.transform.position - entity.Transform.position;
            if (relativePos.sqrMagnitude > 1f)
            {
                Quaternion lookRot = Quaternion.LookRotation(relativePos.normalized);
                entity.Transform.rotation = lookRot * Quaternion.Euler(0, 0, rollAngle);
            }
        }

        public void Shutdown(UAPEntity entity) {}
    }
}