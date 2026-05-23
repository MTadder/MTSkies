using UnityEngine;

namespace MTSkies
{
    public class UAPEntity
    {
        public GameObject Root { get; set; }
        public Transform Transform { get; set; }
        public MeshRenderer Renderer { get; set; }
        public IUAPBehavior CurrentBehavior { get; set; }

        public float Age { get; private set; }
        public float Lifetime { get; set; } = 300f; // Default 5 minutes
        
        private float behaviorChangeTimer = 0f;
        private float nextBehaviorChange = 30f;

        public void Reset()
        {
            Age = 0f;
            Lifetime = UnityEngine.Random.Range(180f, 600f); // 3 to 10 minutes
            behaviorChangeTimer = 0f;
            nextBehaviorChange = UnityEngine.Random.Range(30f, 60f);
        }

        public void Tick(float deltaTime)
        {
            Age += deltaTime;
            behaviorChangeTimer += deltaTime;

            if (behaviorChangeTimer > nextBehaviorChange)
            {
                ForceSwitchBehavior();
            }

            if (CurrentBehavior != null)
            {
                CurrentBehavior.Tick(this, deltaTime);
            }
        }
        
        public void ForceSwitchBehavior()
        {
            behaviorChangeTimer = 0f;
            nextBehaviorChange = UnityEngine.Random.Range(30f, 60f);
            SwitchBehavior();
        }

        private void SwitchBehavior()
        {
            if (CurrentBehavior != null)
                CurrentBehavior.Shutdown(this);
            
            int rand = UnityEngine.Random.Range(0, 6);
            switch (rand)
            {
                case 0: CurrentBehavior = new ObserverBehavior(); break;
                case 1: CurrentBehavior = new OrbiterBehavior(); break;
                case 2: CurrentBehavior = new ShadowBehavior(); break;
                case 3: CurrentBehavior = new HighGDartBehavior(); break;
                case 4: CurrentBehavior = new PendulumBehavior(); break;
                case 5: CurrentBehavior = new MirrorBehavior(); break;
            }
            if (CurrentBehavior == null) CurrentBehavior = new ObserverBehavior();
            
            CurrentBehavior.Initialize(this);
        }
        
        public bool IsExpired => Age > Lifetime;
    }

    public class FleeBehavior : IUAPBehavior
    {
        private Vector3 fleeDirection;
        public void Initialize(UAPEntity entity)
        {
            if (FlightGlobals.ActiveVessel != null)
                fleeDirection = (entity.Transform.position - FlightGlobals.ActiveVessel.transform.position).normalized;
            
            if (fleeDirection == Vector3.zero) 
                fleeDirection = Vector3.up;
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            entity.Transform.position += fleeDirection * 20000f * deltaTime;
        }

        public void Shutdown(UAPEntity entity) {}
    }

    public class ApproachBehavior : IUAPBehavior
    {
        private Vector3 targetOffset;
        
        public void Initialize(UAPEntity entity)
        {
            // Pick an arrival position near the vessel
            float dist = UnityEngine.Random.Range(4000f, 8000f);
            targetOffset = UnityEngine.Random.onUnitSphere * dist;
            targetOffset.y = Mathf.Abs(targetOffset.y) + 500f;
        }

        public void Tick(UAPEntity entity, float deltaTime)
        {
            if (FlightGlobals.ActiveVessel == null) return;
            Vector3 desiredPosition = FlightGlobals.ActiveVessel.transform.position + targetOffset;
            float distance = Vector3.Distance(entity.Transform.position, desiredPosition);

            // Hypersonic transit speed, decelerates cleanly
            entity.Transform.position = Vector3.Lerp(entity.Transform.position, desiredPosition, 1f - Mathf.Exp(-deltaTime * 0.8f));
            entity.Transform.LookAt(FlightGlobals.ActiveVessel.transform.position);

            // When close enough to the arrival point, hand off to standard behaviors
            if (distance < 500f)
            {
                entity.ForceSwitchBehavior();
            }
        }

        public void Shutdown(UAPEntity entity) {}
    }
}
