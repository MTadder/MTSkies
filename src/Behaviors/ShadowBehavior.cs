using UnityEngine;

namespace UAPObservationMod
{
    public class ShadowBehavior : IUAPBehavior
    {
        private float followDistance;
        private float lagSeconds = 2.5f;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[UAPObservation] ShadowBehavior Initialized.");
            followDistance = UnityEngine.Random.Range(1500f, 4000f);
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) return;
            
            Vessel v = FlightGlobals.ActiveVessel;
            
            // Determine the direction the vessel is moving. If stationary, use vessel's forward vector.
            Vector3 movementVector = v.srf_velocity;
            if (movementVector.sqrMagnitude < 1f)
            {
                movementVector = v.transform.up; // KSP forward is often transform.up for rockets, but srf_velocity is better
            }

            Vector3 moveDir = movementVector.normalized;
            
            // Desired position is behind the vessel relative to its movement
            Vector3 desiredPosition = v.transform.position - (moveDir * followDistance);
            
            // Add a slight vertical offset so it's not perfectly in line
            desiredPosition += Vector3.up * 500f;

            // Lerp towards the desired position to create a smooth "lag" effect
            entity.Transform.position = Vector3.Lerp(entity.Transform.position, desiredPosition, deltaTime / lagSeconds);
            
            // Always observe the vessel
            entity.Transform.LookAt(v.transform.position);
        }

        public void Shutdown(UAPEntity entity)
        {
            Debug.Log("[UAPObservation] ShadowBehavior Shutdown.");
        }
    }
}
