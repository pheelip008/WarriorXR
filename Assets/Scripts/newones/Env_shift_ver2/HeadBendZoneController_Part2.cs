// using UnityEngine;

// public class HeadBendZoneController_Part2 : MonoBehaviour
// {
//     [Header("References")]
//     public Transform head;                 // Main Camera
//     public Transform cameraOffset;         // Camera Offset
//     public HeadZoneDetector zoneDetector;
//     public PlaneBendController_Part2 planeController;
//     public static HeadBendZoneController_Part2 Instance;

//     [Header("Upper Threshold")]
//     public float upperBendThreshold = 0.005f; // meters (e.g., deeper bend)

//     [Header("UI Feedback")]
// public GameObject successUI;               // assign in Inspector


//     [Header("Bend Threshold")]
//     public float bendThreshold = 0.02f;    // meters

//     float baselineHeight;
//     bool calibrated;

//     void Start()
//     {
//         Calibrate();
//     }
//     void Awake()
//     {
//         Instance = this;
//     }

//     public void Calibrate()
//     {
//         baselineHeight = head.position.y - cameraOffset.position.y;
//         calibrated = true;
//         Debug.Log($"[Part2] Baseline height: {baselineHeight:F3}");

//     }
//     void UpdateUpperThresholdUI(float delta)
//     {
//     if (successUI == null)
//         return;

//     bool crossedUpper = delta <= -upperBendThreshold;

//     if (successUI.activeSelf != crossedUpper)
//         successUI.SetActive(crossedUpper);
//     }

    

//     void Update()
//     {
//         if (!calibrated || planeController == null || zoneDetector == null)
//             return;
//         float currentHeight = head.position.y - cameraOffset.position.y;
//         float delta = baselineHeight - currentHeight;

//         float bendAmount = Mathf.Clamp01(delta / bendThreshold);
//         // UpdateUpperThresholdUI(delta);


//         // --- ZONE RULES ---
//         if (zoneDetector.ActiveZoneCount() != 1)
//         {
//             // No zone or both zones → no bending
//             planeController.SetBend(0f, 0);
//             return;
//         }

//         if (zoneDetector.inLeftZone)
//         {
//             // Left zone → +X bend
//             planeController.SetBend(bendAmount, +1);
//         }
//         else if (zoneDetector.inRightZone)
//         {
//             // Right zone → -X bend
//             planeController.SetBend(bendAmount, -1);
//         }
//     }
// }

// using UnityEngine;
// using System.Collections;

// public class HeadBendZoneController_Part2 : MonoBehaviour
// {
//     [Header("References")]
//     public Transform head;                 // Main Camera
//     public Transform cameraOffset;         // Camera Offset (optional, can be null)
//     public HeadZoneDetector zoneDetector;
//     public PlaneBendController_Part2 planeController;
//     public static HeadBendZoneController_Part2 Instance;

//     [Header("Calibration Settings")]
//     [Tooltip("Total time to make the user stand straight.")]
//     public float totalCalibrationTime = 5.0f;
//     [Tooltip("The specific second at which to snap the height.")]
//     public float captureTime = 3.0f;

//     [Header("Triggers")]
//     [Tooltip("Height DROP required to start bending (meters).")]
//     public float bendThreshold = 0.20f;    
    
//     [Tooltip("Height RISE above baseline to trigger Success UI (meters).")]
//     public float upperBendThreshold = 0.05f; 

//     [Header("UI Feedback")]
//     public GameObject successUI;

//     // Internal State
//     private float baselineHeight;
//     private bool calibrated = false;

//     void Awake()
//     {
//         Instance = this;
//     }

//     void Start()
//     {
//         StartCoroutine(CalibrateHeight());
//     }

//     /// <summary>
//     /// Waits for 'captureTime' (3s) to snap height, then waits until 'totalCalibrationTime' (5s) is done.
//     /// </summary>
//     private IEnumerator CalibrateHeight()
//     {
//         calibrated = false;
//         float timer = 0f;
//         bool hasCaptured = false;

//         Debug.Log($"[Part2] Starting Calibration ({totalCalibrationTime}s)... Stand straight!");

//         while (timer < totalCalibrationTime)
//         {
//             timer += Time.deltaTime;

//             // Snap height exactly at the 3-second mark
//             if (timer >= captureTime && !hasCaptured)
//             {
//                 float yPos = head.position.y;
//                 // Subtract offset if it exists
//                 if (cameraOffset != null) yPos -= cameraOffset.position.y;

//                 baselineHeight = yPos;
//                 hasCaptured = true;
//                 Debug.Log($"[Part2] Snapshot taken at {timer:F2}s. Baseline Height: {baselineHeight:F3}");
//             }

//             yield return null;
//         }

//         // Failsafe: If frame timing missed the capture, take it now
//         if (!hasCaptured)
//         {
//             float yPos = head.position.y;
//             if (cameraOffset != null) yPos -= cameraOffset.position.y;
//             baselineHeight = yPos;
//         }

//         calibrated = true;
//         Debug.Log("[Part2] Calibration Complete. Game Active.");
//     }

//     void Update()
//     {
//         // 1. Safety Checks
//         if (!calibrated || planeController == null || zoneDetector == null)
//             return;

//         // 2. Calculate Current Height
//         float currentY = head.position.y;
//         if (cameraOffset != null) currentY -= cameraOffset.position.y;

//         // Calculate difference (Positive = standing taller, Negative = crouching)
//         float diff = currentY - baselineHeight;

//         // =========================================================
//         // LOGIC A: UPPER THRESHOLD (Standing Tall / Success UI)
//         // =========================================================
//         // If current height is ABOVE (baseline + upperThreshold)
//         bool isAboveUpper = diff > upperBendThreshold;

//         if (successUI != null)
//         {
//             if (successUI.activeSelf != isAboveUpper)
//                 successUI.SetActive(isAboveUpper);
//         }

//         // =========================================================
//         // LOGIC B: LOWER THRESHOLD (Bending the Plane)
//         // =========================================================
//         // We only care about how much we have DROPPED below baseline
//         float dropAmount = -diff; // Invert so positive means we are down

//         // If we haven't dropped enough (dropAmount < bendThreshold), reset and exit
//         if (dropAmount < bendThreshold)
//         {
//             planeController.SetBend(0f, 0);
//             return;
//         }

//         // Calculate bend intensity (0 to 1)
//         // Example: If threshold is 0.2 and we dropped 0.4, result is (0.4 / 0.2) = 2.0 -> Clamped to 1.0
//         // You might want to offset this so it starts bending AFTER the threshold.
//         // Current formula: Intensity scales based on total drop relative to threshold.
//         float bendAmount = Mathf.Clamp01(dropAmount / (bendThreshold * 2f)); 
//         // Note: I multiplied threshold by 2f to make the bend smoother (full bend at 2x threshold). 
//         // Remove "* 2f" if you want instant full bend at threshold.

//         // --- ZONE RULES ---
//         if (zoneDetector.ActiveZoneCount() != 1)
//         {
//             // No zone or both zones → no bending
//             planeController.SetBend(0f, 0);
//         }
//         else if (zoneDetector.inLeftZone)
//         {
//             // Left zone → +X bend
//             planeController.SetBend(bendAmount, +1);
//         }
//         else if (zoneDetector.inRightZone)
//         {
//             // Right zone → -X bend
//             planeController.SetBend(bendAmount, -1);
//         }
//     }
// }
// using UnityEngine;
// using System.Collections;

// public class HeadBendZoneController_Part2 : MonoBehaviour
// {
//     [Header("References")]
//     public Transform head;                 
//     public Transform cameraOffset;         
//     public PlaneBendController_Part2 planeController;
//     public static HeadBendZoneController_Part2 Instance;

//     [Header("Calibration Settings")]
//     public float totalCalibrationTime = 5.0f;
//     public float captureTime = 3.0f;

//     [Header("Controls")]
//     [Tooltip("Check this if leaning Left makes the plane go Right.")]
//     public bool invertSteering = true; // Set to true by default based on your issue

//     [Header("Height Triggers")]
//     public float bendHeightThreshold = 0.20f;    
//     public float upperBendThreshold = 0.05f; 

//     [Header("Lateral (X-Axis) Triggers")]
//     public float lateralThreshold = 0.15f; 

//     [Header("UI Feedback")]
//     public GameObject successUI;

//     // Internal State
//     private float baselineHeight;
//     private float baselineX;
//     private bool calibrated = false;

//     void Awake()
//     {
//         Instance = this;
//     }

//     void Start()
//     {
//         StartCoroutine(Calibrate());
//     }

//     private IEnumerator Calibrate()
//     {
//         calibrated = false;
//         float timer = 0f;
//         bool hasCaptured = false;

//         Debug.Log($"[Part2] Calibrating... Stand straight and CENTERED.");

//         while (timer < totalCalibrationTime)
//         {
//             timer += Time.deltaTime;

//             if (timer >= captureTime && !hasCaptured)
//             {
//                 float yPos = head.position.y;
//                 float xPos = head.position.x;
                
//                 if (cameraOffset != null) 
//                 {
//                     yPos -= cameraOffset.position.y;
//                     xPos -= cameraOffset.position.x;
//                 }

//                 baselineHeight = yPos;
//                 baselineX = xPos;
//                 hasCaptured = true;
//                 Debug.Log($"[Part2] Snapshot. Base Y: {baselineHeight:F3}, Base X: {baselineX:F3}");
//             }

//             yield return null;
//         }

//         // Failsafe
//         if (!hasCaptured)
//         {
//             baselineHeight = head.position.y;
//             baselineX = head.position.x;
//         }

//         calibrated = true;
//     }

//     void Update()
//     {
//         if (!calibrated || planeController == null) return;

//         // 1. Calculate Current Positions
//         float currentY = head.position.y;
//         float currentX = head.position.x;

//         if (cameraOffset != null)
//         {
//             currentY -= cameraOffset.position.y;
//             currentX -= cameraOffset.position.x;
//         }

//         // 2. Calculate Deltas
//         float heightDiff = currentY - baselineHeight;
//         float lateralDiff = currentX - baselineX;     

//         // 3. UI Logic
//         bool isAboveUpper = heightDiff > upperBendThreshold;
//         if (successUI != null && successUI.activeSelf != isAboveUpper)
//             successUI.SetActive(isAboveUpper);

//         // 4. Bend Logic
//         float dropAmount = -heightDiff; 

//         // Check if Squatting AND Leaning
//         if (dropAmount < bendHeightThreshold || Mathf.Abs(lateralDiff) < lateralThreshold)
//         {
//             planeController.SetBend(0f, 0);
//             return;
//         }

//         // Calculate Intensity
//         float bendAmount = Mathf.Clamp01(dropAmount / (bendHeightThreshold * 2f));

//         // 5. Direction Logic (With Invert Fix)
//         // Standard: +Diff is Right (+1), -Diff is Left (-1)
//         int direction = (lateralDiff > 0) ? 1 : -1;

//         // Apply Inversion if needed
//         if (invertSteering)
//         {
//             direction = -direction;
//         }

//         planeController.SetBend(bendAmount, direction);
//     }
// }

// using UnityEngine;
// using System.Collections;

// public class HeadBendZoneController_Part2 : MonoBehaviour
// {
//     [Header("References")]
//     public Transform head;                 
//     public Transform cameraOffset;         
//     public PlaneBendController_Part2 planeController;
//     public static HeadBendZoneController_Part2 Instance;

//     [Header("Calibration Settings")]
//     public float totalCalibrationTime = 5.0f;
//     public float captureTime = 3.0f;

//     [Header("Controls")]
//     [Tooltip("If your controls are reversed, toggle this.")]
//     public bool invertSteering = false; // CHANGED TO FALSE

//     [Header("Dynamic Thresholds (Read Only)")]
//     public float bendHeightThreshold; 
//     public float realHeightDebug;     

//     [Header("Fixed Thresholds")]
//     public float upperBendThreshold = 0.05f; 
//     public float lateralThreshold = 0.15f; 

//     [Header("Debug")]
//     [Tooltip("Shows current steering direction in Inspector")]
//     public string steeringDebug = "None"; // NEW: Visual debugger

//     [Header("UI Feedback")]
//     public GameObject successUI;

//     // Internal State
//     private float baselineHeight;
//     private float baselineX;
//     private bool calibrated = false;

//     void Awake()
//     {
//         Instance = this;
//     }

//     void Start()
//     {
//         StartCoroutine(Calibrate());
//     }

//     private IEnumerator Calibrate()
//     {
//         calibrated = false;
//         float timer = 0f;
//         bool hasCaptured = false;

//         Debug.Log($"[Part2] Calibrating... Stand straight and CENTERED.");

//         while (timer < totalCalibrationTime)
//         {
//             timer += Time.deltaTime;

//             if (timer >= captureTime && !hasCaptured)
//             {
//                 // 1. Capture Raw Data
//                 float yPos = head.position.y;
//                 float xPos = head.position.x;
//                 if (cameraOffset != null) 
//                 {
//                     yPos -= cameraOffset.position.y;
//                     xPos -= cameraOffset.position.x;
//                 }

//                 baselineHeight = yPos;
//                 baselineX = xPos;

//                 // =========================================================
//                 // YOUR FORMULA IMPLEMENTATION
//                 // =========================================================
                
//                 // Step 1: Calculate "Real" Height (H - 1.03m)
//                 float h_calc = baselineHeight - 1.03f;
//                 realHeightDebug = h_calc; 

//                 // Step 2: Leg Length
//                 float l = h_calc * 0.45f;

//                 // Step 3: Squat Height (Pythagoras)
//                 float legHeightSquatted = 0f;
//                 if (l > 0.5f)
//                     legHeightSquatted = Mathf.Sqrt((l * l) - (0.5f * 0.5f));
//                 else
//                     legHeightSquatted = 0.1f; 

//                 // Step 4: Target Squat Height
//                 float targetSquatHeight = (h_calc / 2.0f) + legHeightSquatted;

//                 // Step 5: Set Threshold
//                 bendHeightThreshold = h_calc - targetSquatHeight;

//                 if (bendHeightThreshold < 0.05f) bendHeightThreshold = 0.05f;

//                 Debug.Log($"[Part2] Dynamic Threshold: {bendHeightThreshold:F3}m");
//                 hasCaptured = true;
//             }

//             yield return null;
//         }

//         if (!hasCaptured)
//         {
//             baselineHeight = head.position.y;
//             baselineX = head.position.x;
//             bendHeightThreshold = 0.20f; 
//         }

//         calibrated = true;
//     }

//     void Update()
//     {
//         if (!calibrated || planeController == null) return;

//         // 1. Calculate Current Positions
//         float currentY = head.position.y;
//         float currentX = head.position.x;

//         if (cameraOffset != null)
//         {
//             currentY -= cameraOffset.position.y;
//             currentX -= cameraOffset.position.x;
//         }

//         // 2. Calculate Deltas
//         float heightDiff = currentY - baselineHeight;
//         float lateralDiff = currentX - baselineX;     

//         // 3. UI Logic
//         bool isAboveUpper = -heightDiff < upperBendThreshold;
//         if (successUI != null && successUI.activeSelf != isAboveUpper)
//             successUI.SetActive(isAboveUpper);

//         // 4. Bend Logic
//         float dropAmount = -heightDiff; 

//         // Check if Squatting AND Leaning
//         if (dropAmount < bendHeightThreshold || Mathf.Abs(lateralDiff) < lateralThreshold)
//         {
//             planeController.SetBend(0f, 0);
//             steeringDebug = "Idle (Deadzone/High)";
//             return;
//         }

//         // Calculate Intensity
//         float bendAmount = Mathf.Clamp01(dropAmount / (bendHeightThreshold * 2f));

//         // 5. Direction Logic
//         int direction = (lateralDiff > 0) ? 1 : -1;
        
//         if (invertSteering) 
//             direction = -direction;

//         // --- Visual Debugging ---
//         if (direction == 1) steeringDebug = "RIGHT >>";
//         else steeringDebug = "<< LEFT";

//         planeController.SetBend(bendAmount, direction);
//     }
// }
using UnityEngine;
using System.Collections;

public class HeadBendZoneController_Part2 : MonoBehaviour
{
    [Header("References")]
    public Transform head;                 
    public Transform cameraOffset;         
    public PlaneBendController_Part2 planeController;
    public static HeadBendZoneController_Part2 Instance;

    [Header("Calibration Settings")]
    public float totalCalibrationTime = 5.0f;

    [Header("Controls")]
    public bool invertSteering = false; 

    [Header("Dynamic Thresholds (Read Only)")]
    public float bendHeightThreshold; 
    public float realHeightDebug;     

    [Header("Fixed Thresholds")]
    public float upperBendThreshold = 0.05f; 
    public float lateralThreshold = 0.15f; 
    [Tooltip("Angle in degrees to trigger tilt warning")]
    public float maxHeadTiltAngle = 45f; // NEW: 45 Degree Limit

    [Header("Debug")]
    public string steeringDebug = "None"; 
    public float currentLateralDiff = 0f;

    [Header("UI Feedback")]
    public GameObject successUI;
    public GameObject extremeTiltUI; // NEW: Assign this in Inspector

    // Internal State
    private float baselineHeight;
    private float baselineLateral; 
    private bool useXAxis = true;  
    private bool calibrated = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(Calibrate());
    }

    private IEnumerator Calibrate()
    {
        calibrated = false;
        float timer = 0f;

        Debug.Log($"[Part2] Calibrating... Stand straight and CENTERED.");

        while (timer < totalCalibrationTime)
        {
            timer += Time.deltaTime;

            // 1. Height
            float yPos = head.position.y;
            if (cameraOffset != null) yPos -= cameraOffset.position.y;
            baselineHeight = yPos;

            // 2. Lateral
            baselineLateral = head.position.x;

            yield return null;
        }

        // --- AUTO-DETECT ROTATION ---
        Vector3 forwardDir = head.forward;
        if (Mathf.Abs(forwardDir.z) > Mathf.Abs(forwardDir.x))
        {
            useXAxis = true; 
            baselineLateral = head.position.x;
            Debug.Log("[Part2] Rotation Detected: Facing Z. Steering with X-Axis.");
        }
        else
        {
            useXAxis = false; 
            baselineLateral = head.position.z;
            Debug.Log("[Part2] Rotation Detected: Facing X. Steering with Z-Axis.");
        }

        // --- CALCULATE DYNAMIC THRESHOLD ---
        float h_calc = baselineHeight - 1.03f;
        realHeightDebug = h_calc; 

        float l = h_calc * 0.45f;
        float legHeightSquatted = 0f;
        
        if (l > 0.5f) legHeightSquatted = Mathf.Sqrt((l * l) - (0.5f * 0.5f));
        else legHeightSquatted = 0.1f; 

        float targetSquatHeight = (h_calc / 2.0f) + legHeightSquatted;
        bendHeightThreshold = h_calc - targetSquatHeight;

        if (bendHeightThreshold < 0.05f) bendHeightThreshold = 0.05f;
        Debug.Log($"[Part2] Threshold Set: {bendHeightThreshold:F3}m");

        calibrated = true;
    }

    void Update()
    {
        if (!calibrated || planeController == null) return;

        // =========================================================
        // 1. HEAD TILT CHECK (NEW LOGIC)
        // =========================================================
        // Convert 0-360 angle to -180 to 180 format
        float currentTilt = head.eulerAngles.z;
        if (currentTilt > 180) currentTilt -= 360;

        // Check if tilt exceeds 45 degrees (positive or negative)
        bool isExtremeTilt = Mathf.Abs(currentTilt) > maxHeadTiltAngle;

        if (extremeTiltUI != null)
        {
            if (extremeTiltUI.activeSelf != isExtremeTilt)
                extremeTiltUI.SetActive(isExtremeTilt);
        }

        // =========================================================
        // 2. POSITION CALCULATIONS
        // =========================================================
        float currentY = head.position.y;
        if (cameraOffset != null) currentY -= cameraOffset.position.y;
        float heightDiff = currentY - baselineHeight;
        float dropAmount = -heightDiff; 

        float currentLateral = useXAxis ? head.position.x : head.position.z;
        float lateralDiff = currentLateral - baselineLateral;
        currentLateralDiff = lateralDiff; 

        // =========================================================
        // 3. UPPER HEIGHT UI
        // =========================================================
        bool isAboveUpper = heightDiff > upperBendThreshold;
        if (successUI != null && successUI.activeSelf != isAboveUpper)
            successUI.SetActive(isAboveUpper);

        // =========================================================
        // 4. BEND PHYSICS LOGIC
        // =========================================================
        if (dropAmount < bendHeightThreshold || Mathf.Abs(lateralDiff) < lateralThreshold)
        {
            planeController.SetBend(0f, 0);
            steeringDebug = "Idle";
            return;
        }

        float bendAmount = Mathf.Clamp01(dropAmount / (bendHeightThreshold * 2f));
        int direction = (lateralDiff > 0) ? 1 : -1;
        if (invertSteering) direction = -direction;

        steeringDebug = (direction == 1) ? "RIGHT >>" : "<< LEFT";
        planeController.SetBend(bendAmount, direction);
    }
}