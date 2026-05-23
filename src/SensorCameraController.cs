using UnityEngine;

namespace UAPObservationMod
{
    public class SensorCameraController : MonoBehaviour
    {
        public bool IsActive { get; private set; }
        public bool HasLock { get { return LockedTarget != null && LockedTarget.Root.activeInHierarchy; } }
        private UAPManager manager;
        public UAPEntity LockedTarget { get; private set; }
        
        // Emulated jitter and tracking
        private float zoomLevel = 1.0f;
        private Vector2 trackingError = Vector2.zero;
        
        // Post-processing and camera manipulation
        private float originalFOV;
        private Camera flightCam;
        private Texture2D noiseTexture;
        private float jitterTime;

        public void Initialize(UAPManager uapManager)
        {
            this.manager = uapManager;
            IsActive = false;
            
            // Generate a simple noise texture for compression artifacts
            noiseTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    float val = UnityEngine.Random.value;
                    noiseTexture.SetPixel(x, y, new Color(val, val, val, val * 0.15f));
                }
            }
            noiseTexture.Apply();
        }

        public void ToggleCamera()
        {
            IsActive = !IsActive;
            Debug.Log("[UAPObservation] Sensor Camera Mode: " + IsActive);
            
            flightCam = FlightCamera.fetch?.mainCamera;
            if (IsActive && flightCam != null)
            {
                originalFOV = flightCam.fieldOfView;
                if (manager.Settings.EnableScreenNoise)
                {
                    TUFXIntegration.ApplySensorProfile();
                }
            }
            else if (!IsActive && flightCam != null)
            {
                flightCam.fieldOfView = originalFOV;
                // Reset transform/pixel offset to remove jitter
                flightCam.transform.localRotation = Quaternion.identity;
                
                TUFXIntegration.ClearSensorProfile();
            }
        }

        private void Update()
        {
            // Toggle tracking mode with Alt+U (stand-in hotkey for now)
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.U))
            {
                ToggleCamera();
            }

            if (!IsActive) return;

            if (LockedTarget == null || !LockedTarget.Root.activeInHierarchy)
            {
                FindTarget();
            }

            if (LockedTarget != null)
            {
                jitterTime += Time.deltaTime;
                
                // Emulate tracking jitter
                trackingError = Vector2.Lerp(trackingError, new Vector2(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f)), Time.deltaTime * 2f);
                
                // Zoom controls
                if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) zoomLevel = Mathf.Min(8f, zoomLevel * 2f);
                if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) zoomLevel = Mathf.Max(1f, zoomLevel / 2f);

                // Photography feature
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    TakePhotograph();
                }
                
                // Apply visual distortion to actual flight camera (Deferred safe)
                if (flightCam != null)
                {
                    float targetFOV = originalFOV / zoomLevel;
                    flightCam.fieldOfView = Mathf.Lerp(flightCam.fieldOfView, targetFOV, Time.deltaTime * 10f);
                    
                    // Shake and jitter via transform rotation instead of projection matrix to preserve Deferred Rendering compatibility
                    float jitterMultiplier = manager.Settings.JitterIntensityMultiplier;
                    float jitterX = (Mathf.PerlinNoise(jitterTime * 50f, 0) - 0.5f) * 1.5f * zoomLevel * jitterMultiplier;
                    float jitterY = (Mathf.PerlinNoise(0, jitterTime * 50f) - 0.5f) * 1.5f * zoomLevel * jitterMultiplier;
                    
                    // Add occasional heavy jitter (frame stutter)
                    if (UnityEngine.Random.value > 0.95f)
                    {
                        jitterX *= 5f;
                        jitterY *= 5f;
                    }

                    // Apply slight rotational offsets to emulate the panning track error/shake
                    flightCam.transform.localRotation = Quaternion.Euler(jitterX, jitterY, 0f);
                }
            }
            else if (flightCam != null)
            {
                flightCam.fieldOfView = Mathf.Lerp(flightCam.fieldOfView, originalFOV, Time.deltaTime * 5f);
                flightCam.transform.localRotation = Quaternion.Slerp(flightCam.transform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            }
        }

        private void TakePhotograph()
        {
            if (LockedTarget != null && FlightGlobals.ActiveVessel != null)
            {
                Debug.Log("[UAPObservation] Photograph taken!");
                
                // Reward logic
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                {
                    // Give more science if zoomed in properly
                    float scienceReward = manager.Settings.BaseScienceReward * zoomLevel;
                    ResearchAndDevelopment.Instance?.AddScience(scienceReward, TransactionReasons.ScienceTransmission);
                    
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        Funding.Instance?.AddFunds(manager.Settings.BaseFundsReward * zoomLevel, TransactionReasons.Progression);
                    }
                    
                    Debug.Log($"[UAPObservation] Captured sensor data transmitted! (+{scienceReward:F1} Science)");
                }
            }
        }

        private void FindTarget()
        {
            LockedTarget = manager.GetClosestEntity();
        }

        private void OnGUI()
        {
            if (!IsActive || !manager.Settings.EnableSensorCamera) return;
            
            // Render noise/compression overlay
            if (manager.Settings.EnableScreenNoise && noiseTexture != null)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.4f); // Semi-transparent
                
                // Draw multiple scaled versions for a blocky compression artifact look
                int offset = Mathf.FloorToInt(UnityEngine.Random.value * 20);
                GUI.DrawTexture(new Rect(0, offset, Screen.width, Screen.height), noiseTexture, ScaleMode.StretchToFill);
                
                if (UnityEngine.Random.value > 0.8f) // Glitch bands
                {
                    GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                    GUI.DrawTexture(new Rect(0, UnityEngine.Random.Range(0, Screen.height), Screen.width, 20), noiseTexture, ScaleMode.StretchToFill);
                    
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    GUI.DrawTexture(new Rect(UnityEngine.Random.Range(-50, 50), 0, Screen.width, Screen.height), noiseTexture, ScaleMode.StretchToFill);
                }
            }

            // Draw a mock HUD overlay representing the "tracking rig view"
            string stateText = LockedTarget == null ? "NO SIGNAL" : "TRK LOCK";
            
            // Cyan HUD based on real world system references
            Color hudColor = LockedTarget == null ? Color.red : new Color(0.0f, 0.8f, 1.0f, 0.8f);
            
            GUI.color = (UnityEngine.Random.value > 0.95f) ? new Color(hudColor.r, hudColor.g, hudColor.b, 0.2f) : hudColor; // UI flicker
            
            // Mock Reticle logic (Center '+' and Tracking Brackets '[ ]')
            float cx = Screen.width / 2f + trackingError.x;
            float cy = Screen.height / 2f + trackingError.y;
            
            // Center Reticle
            GUI.Label(new Rect(cx - 5, cy - 10, 20, 20), "+");
            
            // Framing Brackets
            float bracketSize = 60f;
            GUI.Label(new Rect(cx - bracketSize, cy - bracketSize, 20, 20), "r"); // Top Left
            GUI.Label(new Rect(cx + bracketSize - 5, cy - bracketSize, 20, 20), "n"); // Top Right
            GUI.Label(new Rect(cx - bracketSize, cy + bracketSize - 5, 20, 20), "L"); // Bottom Left
            GUI.Label(new Rect(cx + bracketSize - 5, cy + bracketSize - 5, 20, 20), "j"); // Bottom Right
            
            // Compass N Indicator pointing North relative to the camera
            if (FlightGlobals.ActiveVessel != null && flightCam != null)
            {
                // Get the North pole direction of the current planet
                Vector3 northWorld = FlightGlobals.ActiveVessel.mainBody.transform.up;
                
                // Convert that direction to local camera space
                Vector3 localNorth = flightCam.transform.InverseTransformDirection(northWorld);
                
                // Calculate 2D direction on the screen (GUI Y is inverted from world Y)
                Vector2 screenNorthDir = new Vector2(localNorth.x, -localNorth.y).normalized;
                
                float nRadius = bracketSize + 20f;
                float nx = cx + (screenNorthDir.x * nRadius);
                float ny = cy + (screenNorthDir.y * nRadius);
                
                GUI.Label(new Rect(nx - 5, ny - 10, 20, 20), "N");
            }
            
            // Information block
            GUI.Label(new Rect(20, 20, 200, 20), $"FLIR TRK SYS [REC]");
            GUI.Label(new Rect(20, 40, 200, 20), $"STATE: {stateText}");
            GUI.Label(new Rect(20, 60, 200, 20), $"ZOOM: {zoomLevel}x");
            
            if (LockedTarget != null && FlightGlobals.ActiveVessel != null)
            {
                float dist = Vector3.Distance(LockedTarget.Transform.position, FlightGlobals.ActiveVessel.transform.position);
                GUI.Label(new Rect(20, 80, 200, 20), $"RNG: {dist:F1}m");
                GUI.Label(new Rect(20, 100, 200, 20), "[SPACE] CAPTURE DATA");
            }
        }
    }
}