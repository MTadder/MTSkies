using UnityEngine;

namespace MTSkies
{
    public class OrbiterBehavior : IUAPBehavior
    {
        private float orbitRadius;
        private float targetOrbitRadius;
        private float orbitSpeed;
        private float currentAngle;
        private float heightOffset;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[MTSkies] OrbiterBehavior Initialized.");
            orbitRadius = targetOrbitRadius = UnityEngine.Random.Range(1000f, 3000f);
            orbitSpeed = UnityEngine.Random.Range(0.1f, 0.4f);
            currentAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            heightOffset = UnityEngine.Random.Range(500f, 1500f);
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) return;

            // Randomly adjust the target radius to simulate changing behavior over time
            if (UnityEngine.Random.value < (0.05f * deltaTime))
            {
                targetOrbitRadius = UnityEngine.Random.Range(1000f, 4000f);
            }

            // Smoothly shift to new radius
            orbitRadius = Mathf.Lerp(orbitRadius, targetOrbitRadius, deltaTime * 0.2f);

            // Increment orbit angle
            currentAngle += orbitSpeed * deltaTime;

            // Calculate new position
            Vector3 vesselPos = FlightGlobals.ActiveVessel.transform.position;
            float x = Mathf.Cos(currentAngle) * orbitRadius;
            float z = Mathf.Sin(currentAngle) * orbitRadius;

            Vector3 desiredPosition = vesselPos + new Vector3(x, heightOffset, z);

            // Move the entity smoothly with exponential decay to avoid rigid snapping during time warp
            entity.Transform.position = Vector3.Lerp(entity.Transform.position, desiredPosition, 1f - Mathf.Exp(-deltaTime * 0.5f));
            
            // Always face the vessel
            entity.Transform.LookAt(vesselPos);
        }

        public void Shutdown(UAPEntity entity)
        {
            Debug.Log("[MTSkies] OrbiterBehavior Shutdown.");
        }
    }
}
