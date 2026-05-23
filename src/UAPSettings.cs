using UnityEngine;

namespace UAPObservationMod
{
    public class UAPSettings
    {
        public int MaxActiveUaps = 3;
        public float SpawnProbability = 0.15f;
        public bool EnableSensorCamera = true;
        public bool EnableAudioArtifacts = true;
        public bool EnableScreenNoise = true;
        public float DefaultGlowIntensity = 4.0f;
        public float DefaultSpawnDistance = 2500f;
        
        // Expanded Setting variables
        public float JitterIntensityMultiplier = 1.0f;
        public float PulseSpeedMultiplier = 1.0f;
        public float BaseScienceReward = 5.0f;
        public float BaseFundsReward = 5000f;
        public float MaxDespawnDistance = 100000f;

        public void Load()
        {
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("UAP_SETTINGS");
            if (nodes != null && nodes.Length > 0)
            {
                ConfigNode node = nodes[0];
                
                if (node.HasValue("MaxActiveUaps")) int.TryParse(node.GetValue("MaxActiveUaps"), out MaxActiveUaps);
                if (node.HasValue("SpawnProbability")) float.TryParse(node.GetValue("SpawnProbability"), out SpawnProbability);
                if (node.HasValue("EnableSensorCamera")) bool.TryParse(node.GetValue("EnableSensorCamera"), out EnableSensorCamera);
                if (node.HasValue("EnableAudioArtifacts")) bool.TryParse(node.GetValue("EnableAudioArtifacts"), out EnableAudioArtifacts);
                if (node.HasValue("EnableScreenNoise")) bool.TryParse(node.GetValue("EnableScreenNoise"), out EnableScreenNoise);
                if (node.HasValue("DefaultGlowIntensity")) float.TryParse(node.GetValue("DefaultGlowIntensity"), out DefaultGlowIntensity);
                if (node.HasValue("DefaultSpawnDistance")) float.TryParse(node.GetValue("DefaultSpawnDistance"), out DefaultSpawnDistance);
                
                if (node.HasValue("JitterIntensityMultiplier")) float.TryParse(node.GetValue("JitterIntensityMultiplier"), out JitterIntensityMultiplier);
                if (node.HasValue("PulseSpeedMultiplier")) float.TryParse(node.GetValue("PulseSpeedMultiplier"), out PulseSpeedMultiplier);
                if (node.HasValue("BaseScienceReward")) float.TryParse(node.GetValue("BaseScienceReward"), out BaseScienceReward);
                if (node.HasValue("BaseFundsReward")) float.TryParse(node.GetValue("BaseFundsReward"), out BaseFundsReward);
                if (node.HasValue("MaxDespawnDistance")) float.TryParse(node.GetValue("MaxDespawnDistance"), out MaxDespawnDistance);
                
                Debug.Log("[UAPObservation] Settings loaded successfully.");
            }
            else
            {
                Debug.LogWarning("[UAPObservation] UAP_SETTINGS node not found in GameDatabase! Using defaults for now.");
            }
        }
    }
}
