using UnityEngine;

namespace UAPObservationMod
{
    public class ObserverBehavior : IUAPBehavior
    {
        private Vector3 targetOffset;
        private float speed = 100f;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[UAPObservation] ObserverBehavior Initialized.");
            // Pick a random offset distance to maintain around the player
            targetOffset = new Vector3(
                UnityEngine.Random.Range(-2500f, 2500f), 
                UnityEngine.Random.Range(500f, 2500f), 
                UnityEngine.Random.Range(1000f, 4000f)
            );
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) 
                return;

            // The observer tries to maintain a standoff distance
            Vector3 desiredPosition = FlightGlobals.ActiveVessel.transform.position + targetOffset;
            float distance = Vector3.Distance(entity.Transform.position, desiredPosition);
            
            // Move towards desired position smoothly
            if (distance > 1f)
            {
                float step = (speed * deltaTime) / distance;
                entity.Transform.position = Vector3.Lerp(entity.Transform.position, desiredPosition, step);
            }

            // Always orient toward the player (useful if we use a billboard or directional mesh later)
            entity.Transform.LookAt(FlightGlobals.ActiveVessel.transform.position);
        }

        public void Shutdown(UAPEntity entity)
        {
            Debug.Log("[UAPObservation] ObserverBehavior Shutdown.");
        }
    }
}
