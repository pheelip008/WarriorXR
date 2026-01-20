// // using UnityEngine;
// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine.Events;

// // public class HeadLevelDetector : MonoBehaviour
// // {
// //     [Header("Settings")]
// //     [Tooltip("The VR Camera (Head) transform.")]
// //     public Transform headCamera;
    
// //     [Tooltip("How much lower (in meters) the head must be to trigger the 'Lowered' state.")]
// //     public float lowerThreshold = 0.30f; // 30cm drop

// //     [Tooltip("Time in seconds for the calibration phase.")]
// //     public float calibrationDuration = 5.0f;

// //     [Header("Debug / Read-Only")]
// //     public float initialHeight = 0.0f;
// //     public float currentHeight = 0.0f;
// //     public bool isHeadLowered = false;
// //     public bool isCalibrating = false;

// //     // Events to trigger logic in other scripts (optional)
// //     public UnityEvent OnHeadLowered;
// //     public UnityEvent OnHeadReturned;
// //     public UnityEvent OnCalibrationComplete;

// //     private void Start()
// //     {
// //         if (headCamera == null)
// //         {
// //             // Auto-find the Main Camera if not assigned
// //             headCamera = Camera.main.transform;
// //         }

// //         StartCoroutine(CalibrateHeight());
// //     }

// //     private void Update()
// //     {
// //         if (isCalibrating) return;

// //         // 1. Get current height
// //         currentHeight = headCamera.position.y;

// //         // 2. Check logic
// //         bool isCurrentlyLow = currentHeight < (initialHeight - lowerThreshold);

// //         // 3. State change detection (prevents firing events every frame)
// //         if (isCurrentlyLow && !isHeadLowered)
// //         {
// //             isHeadLowered = true;
// //             Debug.Log("Player is lowering head!");
// //             OnHeadLowered.Invoke();
// //         }
// //         else if (!isCurrentlyLow && isHeadLowered)
// //         {
// //             isHeadLowered = false;
// //             Debug.Log("Player returned to normal height.");
// //             OnHeadReturned.Invoke();
// //         }
// //     }

// //     private IEnumerator CalibrateHeight()
// //     {
// //         isCalibrating = true;
// //         List<float> heightSamples = new List<float>();
// //         float timer = 0f;

// //         Debug.Log("Starting Calibration... Stand straight!");

// //         while (timer < calibrationDuration)
// //         {
// //             // Capture height sample
// //             heightSamples.Add(headCamera.position.y);
            
// //             timer += Time.deltaTime;
// //             yield return null; // Wait for next frame
// //         }

// //         // Calculate Average Height
// //         float sum = 0f;
// //         foreach (float h in heightSamples) sum += h;
        
// //         if (heightSamples.Count > 0)
// //             initialHeight = sum / heightSamples.Count;
// //         else
// //             initialHeight = headCamera.position.y; // Fallback

// //         isCalibrating = false;
// //         Debug.Log($"Calibration Complete. Initial Height: {initialHeight}");
// //         OnCalibrationComplete.Invoke();
// //     }
// // }

// using UnityEngine;
// using System.Collections;
// using UnityEngine.Events;

// public class HeadLevelDetector : MonoBehaviour
// {
//     [Header("Settings")]
//     public Transform headCamera;
    
//     [Tooltip("How much lower (in meters) the head must be to trigger the 'Lowered' state.")]
//     public float lowerThreshold = 0.30f; 

//     [Tooltip("Total time to make the user stand straight.")]
//     public float totalCalibrationTime = 5.0f;

//     [Tooltip("The specific second at which to snap the height.")]
//     public float captureTime = 3.0f;

//     [Header("Debug / Read-Only")]
//     public float initialHeight = 0.0f;
//     public float currentHeight = 0.0f;
    
//     // NEW: X Coordinate variables to see side-to-side position
//     public float initialX = 0.0f; 
//     public float currentX = 0.0f;

//     public bool isHeadLowered = false;
//     public bool isCalibrating = false;

//     public UnityEvent OnHeadLowered;
//     public UnityEvent OnHeadReturned;
//     public UnityEvent OnCalibrationComplete;

//     private void Start()
//     {
//         if (headCamera == null) headCamera = Camera.main.transform;
//         StartCoroutine(CalibrateHeight());
//     }

//     private void Update()
//     {
//         if (isCalibrating) return;

//         // 1. Update Current Values
//         currentHeight = headCamera.position.y;
//         currentX = headCamera.position.x; // Update X for debugging

//         // 2. Check Height Logic
//         bool isCurrentlyLow = currentHeight < (initialHeight - lowerThreshold);

//         if (isCurrentlyLow && !isHeadLowered)
//         {
//             isHeadLowered = true;
//             Debug.Log("Player is lowering head!");
//             OnHeadLowered.Invoke();
//         }
//         else if (!isCurrentlyLow && isHeadLowered)
//         {
//             isHeadLowered = false;
//             Debug.Log("Player returned to normal height.");
//             OnHeadReturned.Invoke();
//         }
//     }

//     private IEnumerator CalibrateHeight()
//     {
//         isCalibrating = true;
//         float timer = 0f;
//         bool hasCaptured = false;

//         Debug.Log("Starting Calibration... Stand straight!");

//         while (timer < totalCalibrationTime)
//         {
//             timer += Time.deltaTime;

//             // Check if we hit the 3-second mark to take the snapshot
//             if (timer >= captureTime && !hasCaptured)
//             {
//                 initialHeight = headCamera.position.y;
//                 initialX = headCamera.position.x; // Capture Initial X
                
//                 hasCaptured = true;
//                 Debug.Log($"Snapshot taken at {timer:F2} seconds. Height: {initialHeight}, X: {initialX}");
//             }

//             yield return null;
//         }

//         // Failsafe: If captureTime > totalCalibrationTime, capture at the very end
//         if (!hasCaptured)
//         {
//             initialHeight = headCamera.position.y;
//             initialX = headCamera.position.x;
//             Debug.Log("Capture time was longer than duration. Taking values at end.");
//         }

//         isCalibrating = false;
//         OnCalibrationComplete.Invoke();
//     }
// }
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class HeadLevelDetector : MonoBehaviour
{
    [Header("Settings")]
    public Transform headCamera;
    
    [Tooltip("If checked, calculates threshold using your (H - 1.03) formula.")]
    public bool useDynamicThreshold = true;

    [Tooltip("Manual threshold (used if Dynamic is unchecked).")]
    public float lowerThreshold = 0.30f; 

    [Tooltip("Total calibration time.")]
    public float totalCalibrationTime = 5.0f;
    public float captureTime = 3.0f;

    [Header("Debug / Read-Only")]
    public float initialHeight = 0.0f; // Actual Camera Height (e.g., 1.75)
    public float currentHeight = 0.0f;
    
    [Tooltip("Calculated using the formula: Threshold = H_real - SquatHeight")]
    public float calculatedThreshold = 0.0f; 
    
    public float initialX = 0.0f; 
    public float currentX = 0.0f;

    public bool isHeadLowered = false;
    public bool isCalibrating = false;

    public UnityEvent OnHeadLowered;
    public UnityEvent OnHeadReturned;
    public UnityEvent OnCalibrationComplete;

    private void Start()
    {
        if (headCamera == null) headCamera = Camera.main.transform;
        StartCoroutine(CalibrateHeight());
    }

    private void Update()
    {
        if (isCalibrating) return;

        currentHeight = headCamera.position.y;
        currentX = headCamera.position.x; 

        // Choose which threshold to use
        float effectiveThreshold = useDynamicThreshold ? calculatedThreshold : lowerThreshold;

        // Check Logic: Uses raw initialHeight - calculatedThreshold
        bool isCurrentlyLow = currentHeight < (initialHeight - effectiveThreshold);

        if (isCurrentlyLow && !isHeadLowered)
        {
            isHeadLowered = true;
            Debug.Log($"Lowered! Current: {currentHeight:F2} < Target: {(initialHeight - effectiveThreshold):F2}");
            OnHeadLowered.Invoke();
        }
        else if (!isCurrentlyLow && isHeadLowered)
        {
            isHeadLowered = false;
            OnHeadReturned.Invoke();
        }
    }

    private IEnumerator CalibrateHeight()
    {
        isCalibrating = true;
        float timer = 0f;
        bool hasCaptured = false;

        Debug.Log("Starting Calibration... Stand straight!");

        while (timer < totalCalibrationTime)
        {
            timer += Time.deltaTime;

            if (timer >= captureTime && !hasCaptured)
            {
                // 1. Capture ACTUAL Camera Height (Do not change this)
                initialHeight = headCamera.position.y;
                initialX = headCamera.position.x; 
                hasCaptured = true;

                // --- YOUR FORMULA LOGIC START ---
                if (useDynamicThreshold)
                {
                    // A. Apply the offset strictly for calculation
                    float h_calc = initialHeight - 1.03f; 

                    // B. Leg Length is 45% of this calculated height
                    float l = h_calc * 0.45f;

                    // C. Calculate Squatted Leg Height (Pythagoras)
                    // We need a tiny check just to prevent crashing if l < 0.5 (Sqrt of negative)
                    float legSquatHeight = 0f;
                    if (l > 0.5f)
                    {
                        legSquatHeight = Mathf.Sqrt((l * l) - (0.5f * 0.5f));
                    }
                    else
                    {
                        // Fallback only if the math is impossible (leg shorter than 0.5m)
                        legSquatHeight = 0.1f; 
                    }

                    // D. Calculate Target Squat Position
                    // (h_calc / 2) + legSquatHeight
                    float targetSquatPos = (h_calc / 2.0f) + legSquatHeight;

                    // E. Final Threshold = The difference between h_calc and targetSquatPos
                    calculatedThreshold = h_calc - targetSquatPos;

                    // Optional: Prevent negative threshold if the math gets weird
                    if (calculatedThreshold < 0.01f) calculatedThreshold = 0.05f;

                    Debug.Log($"[Dynamic] Raw Height: {initialHeight:F2} | Calc Height (H-1.03): {h_calc:F2} | Threshold: {calculatedThreshold:F3}");
                }
                // --- YOUR FORMULA LOGIC END ---
            }

            yield return null;
        }

        if (!hasCaptured)
        {
            initialHeight = headCamera.position.y;
            initialX = headCamera.position.x;
        }

        isCalibrating = false;
        OnCalibrationComplete.Invoke();
    }
}