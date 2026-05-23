using UnityEngine;

namespace UAPObservationMod
{
    public class UAPVisualController : MonoBehaviour
    {
        private MeshRenderer renderer;
        private UAPSettings settings;
        private float pulseTimer;
        private Color baseColor;
        private Light uapLight;

        public void Initialize(MeshRenderer mr, UAPSettings uapSettings)
        {
            this.renderer = mr;
            this.settings = uapSettings;
            this.pulseTimer = UnityEngine.Random.Range(0f, 10f);

            // Add a dynamic light component for intense close-proximity illumination
            uapLight = mr.gameObject.AddComponent<Light>();
            uapLight.type = LightType.Point;
            uapLight.range = 2000f;
            uapLight.intensity = 0f;
            uapLight.enabled = true;

            // Attempt to find a suitable stock additive shader
            Shader glowShader = Shader.Find("KSP/Particles/Additive") ?? Shader.Find("Legacy Shaders/Particles/Additive");
            if (glowShader != null)
            {
                Material mat = new Material(glowShader);
                baseColor = new Color(1f, 1f, 1f, 1f); // White-hot intense glow
                mat.SetColor("_TintColor", baseColor);
                renderer.material = mat;
                uapLight.color = baseColor;
                
                // Form a quad-formation (4 emissive bodies flying in unison based on the study)
                Vector3[] formationOffsets = new Vector3[]
                {
                    new Vector3(40f, 0, 40f),
                    new Vector3(-40f, 0, 40f),
                    new Vector3(40f, 0, -40f)
                };

                foreach (Vector3 offset in formationOffsets)
                {
                    GameObject satellite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    satellite.transform.SetParent(mr.transform, false);
                    satellite.transform.localPosition = offset;
                    satellite.transform.localScale = Vector3.one; // Keep same scale relative to parent
                    
                    Destroy(satellite.GetComponent<Collider>());
                    
                    MeshRenderer satRenderer = satellite.GetComponent<MeshRenderer>();
                    if (satRenderer != null)
                    {
                        satRenderer.material = mat;
                    }
                }
            }
        }

        private void Update()
        {
            if (renderer == null || settings == null || FlightGlobals.ActiveVessel == null) return;

            pulseTimer += Time.deltaTime;
            
            float dist = Vector3.Distance(transform.position, FlightGlobals.ActiveVessel.transform.position);

            // Dramatic pulsation when close to the player/objects
            float pulse;
            float lightIntensity = 0f;

            if (dist < 5000f)
            {
                // Unsettling, rapid/dramatic pulse
                pulse = (Mathf.Sin(pulseTimer * 15f * settings.PulseSpeedMultiplier) * 0.4f) + 
                        (Mathf.PerlinNoise(pulseTimer * 8f * settings.PulseSpeedMultiplier, 0f) * 0.5f) + 0.5f;

                // Emits bright lighting physically reacting with nearby objects
                lightIntensity = Mathf.Lerp(8f, 0f, dist / 5000f) * pulse;
            }
            else
            {
                // Basic slow pulse animation (sine wave)
                pulse = (Mathf.Sin(pulseTimer * 2f * settings.PulseSpeedMultiplier) * 0.2f) + 0.8f;
            }

            if (uapLight != null)
            {
                uapLight.intensity = lightIntensity;
            }
            
            // Distance fade (fade out if too close or too far, just an example)
            // Allow UAPs to be visible from extremely far away, fading out past 'MaxDespawnDistance' / 2
            float fadeStartDist = settings.MaxDespawnDistance / 2f;
            float fadeRange = settings.MaxDespawnDistance - fadeStartDist;
            float distAlpha = Mathf.Clamp01(1f - (dist - fadeStartDist) / fadeRange); 

            Color finalColor = baseColor * pulse * distAlpha * settings.DefaultGlowIntensity;
            
            if (renderer.material.HasProperty("_TintColor"))
            {
                renderer.material.SetColor("_TintColor", finalColor);
            }
        }
    }
}
