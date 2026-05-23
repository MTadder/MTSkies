using UnityEngine;

namespace MTSkies
{
    public class UAPVisualController : MonoBehaviour
    {
        private MeshRenderer renderer;
        private UAPSettings settings;
        private float pulseTimer;
        private Color baseColor;
        private Light uapLight;
        private ParticleSystem trailParticles;

        public void Initialize(MeshRenderer mr, UAPSettings uapSettings)
        {
            this.renderer = mr;
            this.settings = uapSettings;
            this.pulseTimer = UnityEngine.Random.Range(0f, 10f);

            // Add a dynamic light component for intense close-proximity illumination
            uapLight = mr.gameObject.AddComponent<Light>();
            uapLight.type = LightType.Point;
            uapLight.range = 200f;
            uapLight.intensity = 0f;
            uapLight.enabled = true;

            // Attempt to find a suitable stock additive shader
            Shader glowShader = Shader.Find("KSP/Particles/Additive") ?? Shader.Find("Legacy Shaders/Particles/Additive");
            if (glowShader != null)
            {
                Material mat = new Material(glowShader);
                
                // Create soft particle texture to fix box-like artifacts
                Texture2D tex = new Texture2D(32, 32, TextureFormat.ARGB32, false);
                for (int x = 0; x < 32; x++) {
                    for (int y = 0; y < 32; y++) {
                        float dx = x - 15.5f; float dy = y - 15.5f;
                        float d = Mathf.Sqrt(dx*dx + dy*dy) / 15.5f;
                        float a = Mathf.Clamp01(1f - d);
                        tex.SetPixel(x, y, new Color(1, 1, 1, a));
                    }
                }
                tex.Apply();
                mat.mainTexture = tex;

                baseColor = new Color(1f, 1f, 1f, 1f); // White-hot intense glow
                mat.SetColor("_TintColor", baseColor);
                renderer.material = mat;
                uapLight.color = baseColor;
                
                // Randomize formations
                Vector3[] formationOffsets = new Vector3[0];
                int formationType = UnityEngine.Random.Range(0, 5);
                float spacing = UnityEngine.Random.Range(30f, 60f);

                switch (formationType)
                {
                    case 0: // Triangular
                        formationOffsets = new Vector3[] {
                            new Vector3(spacing, 0, 0),
                            new Vector3(spacing / 2f, 0, spacing * 0.866f)
                        };
                        break;
                    case 1: // Quad / Square
                        formationOffsets = new Vector3[] {
                            new Vector3(spacing, 0, 0),
                            new Vector3(0, 0, spacing),
                            new Vector3(spacing, 0, spacing)
                        };
                        break;
                    case 2: // Line (3 trailing)
                        formationOffsets = new Vector3[] {
                            new Vector3(spacing, 0, 0),
                            new Vector3(-spacing, 0, 0),
                            new Vector3(spacing * 2f, 0, 0)
                        };
                        break;
                    case 3: // V-Formation / Arrow
                        formationOffsets = new Vector3[] {
                            new Vector3(-spacing, 0, -spacing),
                            new Vector3(spacing, 0, -spacing),
                            new Vector3(-spacing * 2f, 0, -spacing * 2f),
                            new Vector3(spacing * 2f, 0, -spacing * 2f)
                        };
                        break;
                    case 4: // Circular / Hexagon
                        formationOffsets = new Vector3[] {
                            new Vector3(spacing, 0, 0),
                            new Vector3(spacing * 0.5f, 0, spacing * 0.866f),
                            new Vector3(-spacing * 0.5f, 0, spacing * 0.866f),
                            new Vector3(-spacing, 0, 0),
                            new Vector3(-spacing * 0.5f, 0, -spacing * 0.866f),
                            new Vector3(spacing * 0.5f, 0, -spacing * 0.866f)
                        };
                        break;
                }

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

                // Inject a Particle System to create the requested "dripping/trailing energy" visual spectacle
                GameObject particleObj = new GameObject("UAP_Trail_FX");
                particleObj.transform.SetParent(mr.transform, false);
                trailParticles = particleObj.AddComponent<ParticleSystem>();
                
                var main = trailParticles.main;
                main.duration = 1f;
                main.loop = true;
                main.startLifetime = 4f;        // Linger long enough to form a tail
                main.startSpeed = 0f;           // Particles don't shoot out, they get left behind in world space
                main.startSize = 25f;           // Quite large blobs that will shrink
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.gravityModifier = 0.02f;   // The crucial "drip" effect (particles slowly sink/fall as they're left behind)
                
                var emission = trailParticles.emission;
                emission.rateOverTime = 5f + (formationOffsets.Length * 5f); // Scale particles with formation size

                var shape = trailParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = spacing + 10f; // Match rough size of the formation bounds

                var psRenderer = trailParticles.GetComponent<ParticleSystemRenderer>();
                psRenderer.material = mat;      // Shares same high-intensity additive material
                
                // Color fade over lifetime
                var col = trailParticles.colorOverLifetime;
                col.enabled = true;
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(baseColor, 0f), new GradientColorKey(Color.white, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                col.color = grad;

                // Size fade over lifetime
                var size = trailParticles.sizeOverLifetime;
                size.enabled = true;
                size.size = new ParticleSystem.MinMaxCurve(1f, 0f);
            }
        }

        private void Update()
        {
            if (renderer == null || settings == null || FlightGlobals.ActiveVessel == null) return;

            pulseTimer += TimeWarp.deltaTime;
            
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

            // Sync trail emission/color with current distance fade & pulsation
            if (trailParticles != null)
            {
                var main = trailParticles.main;
                main.startColor = new Color(finalColor.r, finalColor.g, finalColor.b, distAlpha); // Fade out transparency along with entity
            }
        }
    }
}
