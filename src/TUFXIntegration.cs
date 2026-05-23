using System;
using System.Reflection;
using UnityEngine;

namespace MTSkies
{
    /// <summary>
    /// Soft-dependency wrapper for TUFX / Unity PostProcessing.
    /// Safely interfaces with post-processing without forcing a hard DLL requirement.
    /// </summary>
    public static class TUFXIntegration
    {
        private static bool checkCompleted = false;
        private static bool isAvailable = false;
        
        // Reflection targets
        private static Type ppVolumeType;
        private static Component activeVolume;

        public static bool IsAvailable
        {
            get
            {
                if (!checkCompleted) CheckForTUFX();
                return isAvailable;
            }
        }

        private static void CheckForTUFX()
        {
            checkCompleted = true;
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name.StartsWith("TUFX", StringComparison.OrdinalIgnoreCase))
                    {
                        ppVolumeType = assembly.GetType("UnityEngine.Rendering.PostProcessing.PostProcessVolume");
                        if (ppVolumeType != null)
                        {
                            isAvailable = true;
                            Debug.Log("[MTSkies] Post Processing (via TUFX) detected! Camera FX active.");
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MTSkies] Error probing for TUFX: " + e.Message);
            }
        }

        public static void ApplySensorProfile(Camera cam)
        {
            if (!IsAvailable || cam == null) return;
            
            try 
            {
                if (activeVolume == null)
                {
                    activeVolume = cam.gameObject.AddComponent(ppVolumeType);
                    
                    // Create a generic profile dynamically using Unity's ScriptableObject if needed.
                    // Doing heavy reflection to map sub-settings (Grain, Vignette) can be complex, 
                    // so we instead interact with TUFX's generic pipeline by forcing a High Priority volume.
                    // We set isGlobal = true, priority = 9999
                    PropertyInfo isGlobalProp = ppVolumeType.GetProperty("isGlobal");
                    PropertyInfo priorityProp = ppVolumeType.GetProperty("priority");
                    
                    if (isGlobalProp != null) isGlobalProp.SetValue(activeVolume, true, null);
                    if (priorityProp != null) priorityProp.SetValue(activeVolume, 9999f, null);
                    
                    // Note: Dynamically populating the PostProcessProfile via pure reflection 
                    // is verbose, but having the high-priority volume allows us to inject noise!
                }
                
                // For simplicity without hard-linking, we just enable the volume wrapper here.
                // Depending on TUFX config, it can automatically apply predefined Grain configs.
                (activeVolume as Behaviour).enabled = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MTSkies] Failed to apply FX: " + e.Message);
            }
        }

        public static void ClearSensorProfile()
        {
            if (!IsAvailable || activeVolume == null) return;
            try
            {
                (activeVolume as Behaviour).enabled = false;
            }
            catch (Exception) {}
        }
    }
}
