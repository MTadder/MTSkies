using UnityEngine;

namespace MTSkies
{
    /// <summary>
    /// Mimics the common "Falling Leaf" UFO report: The object descends while swinging
    /// side to side like a pendulum, bleeding energy before shooting off or hovering.
    /// </summary>
    public class PendulumBehavior : IUAPBehavior
    {
        private Vector3 startOffset;
        private Vector3 endOffset;
        private Vector3 perpendicularAxis;
        private float phaseDuration = 20f;
        private float timePassed = 0f;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[MTSkies] PendulumBehavior Initialized.");
            timePassed = 0f;

            // Start high up and far away
            startOffset = UnityEngine.Random.onUnitSphere * 8000f;
            startOffset.y = Mathf.Abs(startOffset.y) + 4000f;

            // End lower down and closer
            endOffset = UnityEngine.Random.onUnitSphere * 2000f;
            endOffset.y = UnityEngine.Random.Range(-500f, 500f);

            // Generate an arbitrary axis perpendicular to the travel path for the swing
            Vector3 path = endOffset - startOffset;
            perpendicularAxis = Vector3.Cross(path.normalized, Vector3.up).normalized;
            
            if (perpendicularAxis == Vector3.zero)
                perpendicularAxis = Vector3.forward;
                
            if (FlightGlobals.ActiveVessel != null)
                entity.Transform.position = FlightGlobals.ActiveVessel.transform.position + startOffset;
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) return;
            
            timePassed += deltaTime;
            float t = timePassed / phaseDuration;
            
            if (t > 1f) t = 1f;

            // Base linear movement path
            Vector3 centerPath = Vector3.Lerp(startOffset, endOffset, t);
            
            // Apply pendulum swing based on a sine wave that gets tighter as it descends
            float swingAmplitude = Mathf.Lerp(1500f, 200f, t);
            float swingFrequency = Mathf.Lerp(1f, 4f, t);
            Vector3 swingOffset = perpendicularAxis * (Mathf.Sin(timePassed * swingFrequency) * swingAmplitude);

            Vector3 desiredPosition = FlightGlobals.ActiveVessel.transform.position + centerPath + swingOffset;
            
            // Smoothly track to it
            entity.Transform.position = Vector3.Lerp(entity.Transform.position, desiredPosition, 1f - Mathf.Exp(-deltaTime * 4f));
            entity.Transform.LookAt(FlightGlobals.ActiveVessel.transform.position);
        }

        public void Shutdown(UAPEntity entity) {}
    }
}