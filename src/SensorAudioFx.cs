using UnityEngine;

namespace UAPObservationMod
{
    public class SensorAudioFx : MonoBehaviour
    {
        private AudioSource staticSource;
        private SensorCameraController cameraController;
        private UAPManager manager;

        public void Initialize(SensorCameraController camCtrl, UAPManager mgr)
        {
            this.cameraController = camCtrl;
            this.manager = mgr;

            staticSource = gameObject.AddComponent<AudioSource>();
            staticSource.loop = true;
            staticSource.playOnAwake = false;
            staticSource.volume = 0f;
            staticSource.spatialBlend = 0f; // 2D sound for HUD UI

            // In a real mod, you'd load an AudioClip bundle here.
            // For now, we will rely on dynamically generating a simple noise if needed, 
            // or just leave it wired for future clip injection.
        }

        private void Update()
        {
            if (manager == null || !manager.Settings.EnableAudioArtifacts)
            {
                if (staticSource.isPlaying) staticSource.Stop();
                return;
            }

            if (cameraController.IsActive)
            {
                if (!staticSource.isPlaying) staticSource.Play();

                // Increase static volume if we lose lock, decrease if locked
                float targetVol = cameraController.HasLock ? 0.05f : 0.2f;
                staticSource.volume = Mathf.Lerp(staticSource.volume, targetVol, Time.deltaTime * 5f);
            }
            else
            {
                if (staticSource.isPlaying) staticSource.Stop();
            }
        }
    }
}