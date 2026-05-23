using UnityEngine;

namespace UAPObservationMod
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class UAPBootstrap : MonoBehaviour
    {
        private UAPManager manager;

        private void Start()
        {
            Debug.Log("[UAPObservation] Bootstrap initializing in flight scene.");
            GameObject managerObj = new GameObject("UAP_Manager");
            manager = managerObj.AddComponent<UAPManager>();
        }

        private void OnDestroy()
        {
            Debug.Log("[UAPObservation] Bootstrap shutting down.");
            if (manager != null)
            {
                Destroy(manager.gameObject);
            }
        }
    }
}
