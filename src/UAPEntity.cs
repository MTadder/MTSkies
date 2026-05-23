using UnityEngine;

namespace UAPObservationMod
{
    public class UAPEntity
    {
        public GameObject Root { get; set; }
        public Transform Transform { get; set; }
        public MeshRenderer Renderer { get; set; }
        public IUAPBehavior CurrentBehavior { get; set; }

        public float Age { get; private set; }
        public float Lifetime { get; set; } = 300f; // Default 5 minutes

        public void Reset()
        {
            Age = 0f;
            Lifetime = UnityEngine.Random.Range(180f, 600f); // 3 to 10 minutes
        }

        public void Tick(float deltaTime)
        {
            Age += deltaTime;

            if (CurrentBehavior != null)
            {
                CurrentBehavior.Tick(this, deltaTime);
            }
        }
        
        public bool IsExpired => Age > Lifetime;
    }
}
