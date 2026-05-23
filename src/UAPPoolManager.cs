using System.Collections.Generic;
using UnityEngine;

namespace UAPObservationMod
{
    public class UAPPoolManager
    {
        private Stack<UAPEntity> pool = new Stack<UAPEntity>();

        private UAPManager manager;

        public UAPPoolManager(UAPManager manager)
        {
            this.manager = manager;
        }

        public UAPEntity GetEntity()
        {
            if (pool.Count > 0)
            {
                UAPEntity entity = pool.Pop();
                entity.Root.SetActive(true);
                return entity;
            }

            return CreateNewEntity();
        }

        public void ReturnEntity(UAPEntity entity)
        {
            if (entity.CurrentBehavior != null)
            {
                entity.CurrentBehavior.Shutdown(entity);
                entity.CurrentBehavior = null;
            }
            
            if (entity.Root != null)
            {
                entity.Root.SetActive(false);
            }
            
            pool.Push(entity);
        }

        private UAPEntity CreateNewEntity()
        {
            GameObject root = new GameObject("UAP_Entity");
            
            // Reusing the simple sphere primitive for testing
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(10f, 10f, 10f); 

            // Destroy the collider as we don't want physics simulations
            UnityEngine.Object.Destroy(visual.GetComponent<Collider>());
            
            MeshRenderer mr = visual.GetComponent<MeshRenderer>();

            // Attach and initialize the visual controller for Phase 5
            UAPVisualController visualController = root.AddComponent<UAPVisualController>();
            visualController.Initialize(mr, manager.Settings);

            return new UAPEntity
            {
                Root = root,
                Transform = root.transform,
                Renderer = mr
            };
        }

        public void Clear()
        {
            while (pool.Count > 0)
            {
                UAPEntity entity = pool.Pop();
                if (entity.Root != null)
                {
                    UnityEngine.Object.Destroy(entity.Root);
                }
            }
        }
    }
}
