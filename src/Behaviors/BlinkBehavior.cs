using UnityEngine;

namespace UAPObservationMod
{
    public class BlinkBehavior : IUAPBehavior
    {
        private float timer;
        private float nextStateChange;
        private bool isVisible = true;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[UAPObservation] BlinkBehavior Initialized.");
            isVisible = true;
            timer = 0f;
            SetNextChangeTime();
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) return;
            
            timer += deltaTime;
            if (timer >= nextStateChange)
            {
                timer = 0f;
                isVisible = !isVisible;
                SetNextChangeTime();

                if (entity.Renderer != null)
                {
                    entity.Renderer.enabled = isVisible;
                }

                // If it just became invisible, jump to a new position immediately
                if (!isVisible)
                {
                    Vector3 spawnDir = UnityEngine.Random.onUnitSphere;
                    // Bias spawn to be slightly above the player
                    if (spawnDir.y < -0.2f) spawnDir.y = -spawnDir.y;
                    
                    entity.Transform.position = FlightGlobals.ActiveVessel.transform.position + (spawnDir * UnityEngine.Random.Range(1000f, 4000f));
                }
            }
            
            // While visible, slowly drift and observe player
            if (isVisible)
            {
                entity.Transform.LookAt(FlightGlobals.ActiveVessel.transform.position);
                entity.Transform.position += entity.Transform.right * (15f * deltaTime);
            }
        }

        private void SetNextChangeTime()
        {
            if (isVisible)
            {
                // Stays visible for 2 to 6 seconds
                nextStateChange = UnityEngine.Random.Range(2f, 6f);
            }
            else
            {
                // Disappears for a short blink (0.2 to 1.5 seconds)
                nextStateChange = UnityEngine.Random.Range(0.2f, 1.5f);
            }
        }

        public void Shutdown(UAPEntity entity)
        {
            if (entity.Renderer != null)
            {
                // Ensure rendered stays enabled when returned to pool
                entity.Renderer.enabled = true; 
            }
        }
    }
}
