using UnityEngine;
using KSP.UI.Screens;

namespace UAPObservationMod
{
    public class UAPWindow : MonoBehaviour
    {
        private UAPManager manager;
        private ApplicationLauncherButton appLauncherButton;
        private bool isWindowOpen = false;
        private Rect windowRect = new Rect(300, 300, 250, 300);

        public void Initialize(UAPManager uapManager)
        {
            this.manager = uapManager;
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
            OnGUIAppLauncherReady(); // In case it's already ready
        }

        private void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
            OnGUIAppLauncherDestroyed();
        }

        private void OnGUIAppLauncherReady()
        {
            if (appLauncherButton == null && ApplicationLauncher.Ready)
            {
                Texture2D icon = GameDatabase.Instance.GetTexture("UAPObservationMod/icon", false);
                if (icon == null) 
                {
                    icon = GameDatabase.Instance.GetTexture("Squad/PartList/SimpleIcons/R&D_node_icon_advexploration", false);
                }

                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToggleOn,
                    OnToggleOff,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                    icon
                );
            }
        }

        private void OnGUIAppLauncherDestroyed()
        {
            if (appLauncherButton != null && ApplicationLauncher.Instance != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
                appLauncherButton = null;
            }
        }

        private void OnToggleOn()
        {
            isWindowOpen = true;
        }

        private void OnToggleOff()
        {
            isWindowOpen = false;
        }

        private void OnGUI()
        {
            if (isWindowOpen)
            {
                GUI.skin = HighLogic.Skin;
                windowRect = GUILayout.Window(88319, windowRect, DrawWindow, "UAP Observation Mod");
            }
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("- Status -");
            GUILayout.Label($"Active UAPs: {manager.ActiveEntities.Count} / {manager.Settings.MaxActiveUaps}");
            if (UAPScenarioModule.Instance != null)
            {
                GUILayout.Label($"Total Sightings: {UAPScenarioModule.Instance.TotalSightings}");
            }
            GUILayout.Space(10);

            GUILayout.Label("- Controls -");
            if (manager.SensorCamera != null)
            {
                string camText = manager.SensorCamera.IsActive ? "Deactivate Sensor System" : "Activate Sensor System";
                if (GUILayout.Button(camText))
                {
                    manager.SensorCamera.ToggleCamera();
                }
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("- Settings -");
            bool changedNoise = GUILayout.Toggle(manager.Settings.EnableScreenNoise, "Camera Glitch / Noise");
            if (changedNoise != manager.Settings.EnableScreenNoise)
            {
                manager.Settings.EnableScreenNoise = changedNoise;
            }
            
            bool changedAudio = GUILayout.Toggle(manager.Settings.EnableAudioArtifacts, "Audio Artifacts");
            if (changedAudio != manager.Settings.EnableAudioArtifacts)
            {
                manager.Settings.EnableAudioArtifacts = changedAudio;
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Force Spawn UAP"))
            {
                manager.SpawnManager.TriggerEventDrivenSpawn(1.0f);
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
