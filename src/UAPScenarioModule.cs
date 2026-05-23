using UnityEngine;

namespace MTSkies
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    public class UAPScenarioModule : ScenarioModule
    {
        public static UAPScenarioModule Instance { get; private set; }

        public int TotalSightings { get; private set; } = 0;
        public bool SensorUnlocked { get; private set; } = true;

        public override void OnAwake()
        {
            base.OnAwake();
            Instance = this;
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            
            if (node.HasValue("TotalSightings"))
            {
                int.TryParse(node.GetValue("TotalSightings"), out int sightings);
                TotalSightings = sightings;
            }

            if (node.HasValue("SensorUnlocked"))
            {
                bool.TryParse(node.GetValue("SensorUnlocked"), out bool unlocked);
                SensorUnlocked = unlocked;
            }

            Debug.Log($"[MTSkies] Scenario data loaded. Sightings: {TotalSightings}");
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            node.SetValue("TotalSightings", TotalSightings.ToString(), true);
            node.SetValue("SensorUnlocked", SensorUnlocked.ToString(), true);
            
            Debug.Log("[MTSkies] Scenario data saved.");
        }

        public void RecordSighting()
        {
            TotalSightings++;
        }
    }
}
