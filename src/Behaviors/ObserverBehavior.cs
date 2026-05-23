using UnityEngine;

namespace MTSkies
{
    public class ObserverBehavior : IUAPBehavior
    {
        private Vector3 targetOffset;
        private float hoverTimer = 0f;
        private float nextDartTime = 5f;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[MTSkies] ObserverBehavior Initialized.");
            PickNewOffset();
        }

        private void PickNewOffset()
        {
            // Pick a random offset distance to maintain around the player
            float dist = UnityEngine.Random.Range(2000f, 6000f);
            targetOffset = UnityEngine.Random.onUnitSphere * dist;
            targetOffset.y = Mathf.Abs(targetOffset.y) + 500f; // Bias upward to avoid terrain
            
            hoverTimer = 0f;
            nextDartTime = UnityEngine.Random.Range(4f, 12f);
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) 
                return;

            hoverTimer += deltaTime;
            if (hoverTimer > nextDartTime)
            {
                PickNewOffset(); // Sudden right-angle darting
            }

            // The observer tries to maintain a standoff distance
            Vector3 desiredPosition = FlightGlobals.ActiveVessel.transform.position + targetOffset;
            
            // Erratic micro-jitter hovering (real-world signature)
            Vector3 jitter = new Vector3(
                (Mathf.PerlinNoise(Time.time * 3f, 0) - 0.5f),
                (Mathf.PerlinNoise(0, Time.time * 3f) - 0.5f),
                (Mathf.PerlinNoise(Time.time * 3f, Time.time * 3f) - 0.5f)
            ) * 60f;
            
            desiredPosition += jitter;
            
            // Move towards desired position smoothly but with extreme speed, matching non-inertial behavior
            float distance = Vector3.Distance(entity.Transform.position, desiredPosition);
            if (distance > 1f)
            {
                // Unnaturally massive acceleration and perfect stopping
                entity.Transform.position = Vector3.Lerp(entity.Transform.position, desiredPosition, 1f - Mathf.Exp(-deltaTime * 5.0f));
            }

            // Always orient toward the player
            entity.Transform.LookAt(FlightGlobals.ActiveVessel.transform.position);
        }

        public void Shutdown(UAPEntity entity)
        {
            Debug.Log("[MTSkies] ObserverBehavior Shutdown.");
        }
    }
}
