using UnityEngine;

namespace MTSkies
{
    /// <summary>
    /// Mimics the famous Princeton / Nimitz radar data where UAPs dropped from 80,000 feet 
    /// to sea level in a matter of seconds, hovered, and shot back up.
    /// </summary>
    public class HighGDartBehavior : IUAPBehavior
    {
        private Vector3 targetAltitudePos;
        private float stateTimer = 0f;
        private int dropState = 0; // 0 = high altitude, 1 = dropping, 2 = hovering low, 3 = ascending
        
        private Vector3 highAltitudeOffset;
        private Vector3 lowAltitudeOffset;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[MTSkies] HighGDartBehavior Initialized.");
            stateTimer = 0f;
            dropState = 0;
            
            // Set up a high stationary point and a direct point directly below it near the vessel.
            float horizDist = UnityEngine.Random.Range(2000f, 6000f);
            Vector3 randomDir2D = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
            
            lowAltitudeOffset = (randomDir2D * horizDist);
            lowAltitudeOffset.y = UnityEngine.Random.Range(-500f, 500f); // Roughly vessel altitude
            
            highAltitudeOffset = lowAltitudeOffset;
            highAltitudeOffset.y += 24000f; // Extremely high above vessel
            
            // Instantly snap to high altitude to begin the cycle
            if (FlightGlobals.ActiveVessel != null)
                entity.Transform.position = FlightGlobals.ActiveVessel.transform.position + highAltitudeOffset;
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) return;
            
            stateTimer += deltaTime;
            
            Vector3 targetHigh = FlightGlobals.ActiveVessel.transform.position + highAltitudeOffset;
            Vector3 targetLow = FlightGlobals.ActiveVessel.transform.position + lowAltitudeOffset;
            
            switch(dropState)
            {
                case 0: // Hovering High
                    entity.Transform.position = Vector3.Lerp(entity.Transform.position, targetHigh, 1f - Mathf.Exp(-deltaTime * 2f));
                    if (stateTimer > 4f) { dropState = 1; }
                    break;
                    
                case 1: // Dropping
                    // Extremely violent acceleration
                    entity.Transform.position = Vector3.Lerp(entity.Transform.position, targetLow, 1f - Mathf.Exp(-deltaTime * 15f));
                    if (Vector3.Distance(entity.Transform.position, targetLow) < 50f) 
                    {
                        dropState = 2;
                        stateTimer = 0f;
                    }
                    break;
                    
                case 2: // Hovering Low
                    entity.Transform.position = Vector3.Lerp(entity.Transform.position, targetLow, 1f - Mathf.Exp(-deltaTime * 2f));
                    if (stateTimer > 5f) { dropState = 3; }
                    break;
                    
                case 3: // Ascending
                    entity.Transform.position = Vector3.Lerp(entity.Transform.position, targetHigh, 1f - Mathf.Exp(-deltaTime * 15f));
                    if (Vector3.Distance(entity.Transform.position, targetHigh) < 50f) 
                    {
                        dropState = 0;
                        stateTimer = 0f;
                    }
                    break;
            }

            entity.Transform.LookAt(FlightGlobals.ActiveVessel.transform.position);
        }

        public void Shutdown(UAPEntity entity) {}
    }
}