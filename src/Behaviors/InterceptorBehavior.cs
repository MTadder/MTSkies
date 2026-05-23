using UnityEngine;

namespace MTSkies
{
    public class InterceptorBehavior : IUAPBehavior
    {
        private float speed = 500f;
        private bool isBreakingAway = false;
        private Vector3 breakawayDirection;
        private float escapeTimer = 0f;

        public void Initialize(UAPEntity entity)
        {
            Debug.Log("[MTSkies] InterceptorBehavior Initialized.");
            // Starts normal approach
            isBreakingAway = false;
            escapeTimer = 0f;
            entity.Renderer.enabled = true;
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) return;

            if (!isBreakingAway)
            {
                // Fast approach
                Vector3 targetPos = FlightGlobals.ActiveVessel.transform.position;
                float distance = Vector3.Distance(entity.Transform.position, targetPos);

                if (distance < 800f)
                {
                    // Too close! Break away rapidly
                    isBreakingAway = true;
                    breakawayDirection = (entity.Transform.position - targetPos).normalized + Vector3.up;
                    speed = 2500f; // Rapid acceleration on break
                }
                else
                {
                    // Move towards target
                    Vector3 moveDir = (targetPos - entity.Transform.position).normalized;
                    entity.Transform.position += moveDir * (speed * deltaTime);
                    entity.Transform.LookAt(targetPos);
                    
                    // Slightly increase speed as it closes in
                    speed += 100f * deltaTime;
                }
            }
            else
            {
                escapeTimer += deltaTime;
                if (escapeTimer > 3f)
                {
                    // Vanish after escaping for a bit
                    entity.Renderer.enabled = false;
                }
                else
                {
                    // High speed escape vector
                    entity.Transform.position += breakawayDirection * (speed * deltaTime);
                    entity.Transform.rotation = Quaternion.LookRotation(breakawayDirection);
                }
            }
        }

        public void Shutdown(UAPEntity entity)
        {
            entity.Renderer.enabled = true; // reset for pool
            Debug.Log("[MTSkies] InterceptorBehavior Shutdown.");
        }
    }
}
