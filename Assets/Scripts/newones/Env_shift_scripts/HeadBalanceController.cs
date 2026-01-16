// using UnityEngine;

// public class HeadBalanceController : MonoBehaviour
// {
//     [Header("References")]
//     public Transform head;           // XR Camera
//     public BalancePlaneController planeController;

//     [Header("Thresholds")]
//     public float bendThreshold = 0.15f;   // meters below start
//     public float smoothTime = 1.0f;

//     float initialHeadY;
//     bool calibrated = false;

//     void Start()
//     {
//         if (head == null)
//             Debug.LogError("HeadBalanceController: Head not assigned!");
//     }

//     void Update()
//     {
//         if (!calibrated && head != null)
//         {
//             initialHeadY = head.position.y;
//             calibrated = true;
//             Debug.Log($"[Balance] Initial head height: {initialHeadY:F2}");
//         }

//         if (!calibrated) return;

//         float delta = initialHeadY - head.position.y;

//         // Normalize bend amount (0 → 1)
//         float bendAmount = Mathf.Clamp01(delta / bendThreshold);

//         // Inform plane controller
//         planeController.SetStability(bendAmount);
//     }
// }

// using UnityEngine;

// public class HeadBalanceController : MonoBehaviour
// {
//     [Header("References")]
//     public Transform head;          // XR Camera
//     public Transform xrOrigin;      // XR Origin
//     public BalancePlaneController planeController;

//     [Header("Calibration")]
//     public float bendThreshold = 0.06f; // meters
//     public bool autoCalibrateOnStart = true;

//     float baselineHeadHeight;
//     bool calibrated = false;

//     void Start()
//     {
//         if (autoCalibrateOnStart)
//             Calibrate();
//     }

//     public void Calibrate()
//     {
//         if (head == null || xrOrigin == null)
//         {
//             Debug.LogError("HeadBalanceController: Missing references!");
//             return;
//         }

//         baselineHeadHeight = head.position.y - xrOrigin.position.y;
//         calibrated = true;

//         Debug.Log($"[Calibration] Baseline head height: {baselineHeadHeight:F3} m");
//     }

//     void Update()
//     {
//         if (!calibrated || planeController == null) return;

//         float currentHeight = head.position.y - xrOrigin.position.y;

//         float delta = baselineHeadHeight - currentHeight;

//         float stability = Mathf.Clamp01(delta / bendThreshold);

//         planeController.SetStability(stability);
//     }
// }
// //////////////////////////real1///////////////////////////////
// using UnityEngine;

// public class HeadBalanceController : MonoBehaviour
// {
//     [Header("References")]
//     public Transform head;
//     public Transform xrOrigin;
//     public BalancePlaneController planeController;
//     public HeadZoneDetector zoneDetector;

//     [Header("Threshold")]
//     public float bendThreshold = 0.04f;

//     float baselineHeight;
//     bool calibrated;

//     void Start()
//     {
//         Calibrate();
//     }

//     public void Calibrate()
//     {
//         baselineHeight = head.position.y - xrOrigin.position.y;
//         calibrated = true;
//         Debug.Log($"[Calibration] Head baseline: {baselineHeight:F3}");
//     }

//     void Update()
//     {
//         if (!calibrated || planeController == null || zoneDetector == null)
//             return;

//         float currentHeight = head.position.y - xrOrigin.position.y;
//         float delta = baselineHeight - currentHeight;

//         float stability = Mathf.Clamp01(delta / bendThreshold);

//         // Only stabilize if exactly ONE zone is active
//         int direction = 1; // default tilt direction

// if (zoneDetector != null)
// {
//     if (zoneDetector.ActiveZoneCount() == 1)
//     {
//         direction = zoneDetector.inLeftZone ? -1 : +1;
//     }
//     else
//     {
//         // zones invalid → no stabilization, but KEEP tilt visible
//         planeController.SetStability(0f, direction);
//         return;
//     }
// }

// planeController.SetStability(stability, direction);

//     }
// }

//////////////////////////////////////////Initial///////////////////////////////
using UnityEngine;

public class HeadBalanceController : MonoBehaviour
{
    [Header("References")]
    public Transform head;                 // Main Camera
    public Transform cameraOffset;         // Camera Offset
    public BalancePlaneController planeController;
    public HeadZoneDetector zoneDetector;

    [Header("Threshold")]
    public float bendThreshold = 0.12f;    // meters

    float baselineHeight;
    bool calibrated;

    void Start()
    {
        Calibrate();
    }

    public void Calibrate()
    {
        baselineHeight = head.position.y - cameraOffset.position.y;
        calibrated = true;

        Debug.Log($"[Calibration] Baseline head height: {baselineHeight:F3}");
    }

    void Update()
{
    if (!calibrated || planeController == null)
        return;

    float currentHeight = head.position.y - cameraOffset.position.y;
    float delta = baselineHeight - currentHeight;
    float stability = Mathf.Clamp01(delta / bendThreshold);

    int direction = 1; // default (+X)

    if (zoneDetector != null && zoneDetector.ActiveZoneCount() == 1)
    {
        direction = zoneDetector.inLeftZone ? -1 : +1;
    }

    planeController.SetStability(stability, direction);
}

}



//////////////////////////////////////////only tilts when nin zone///// no reaacotn to bends///////////////////////////////
// using System.Collections;
// using UnityEngine;

// [DisallowMultipleComponent]
// public class HeadBalanceController : MonoBehaviour
// {
//     [Header("References")]
//     public Transform head;                       // Main Camera
//     public BalancePlaneController planeController;
//     public HeadZoneDetector zoneDetector;

//     [Header("Bend Detection")]
//     [Tooltip("Downward head movement (meters) required for full effect")]
//     public float bendThreshold = 0.12f;
//     [Header("Bend Reset")]
//     public float standUpTolerance = 0.04f; // meters

//     // Internal state
//     float baselineWorldY;
//     float lowestWorldY;
//     bool calibrated = false;

//     void Start()
//     {
//         // Delay calibration so XR tracking stabilizes
//         StartCoroutine(CalibrateAfterXRReady());
//     }

//     IEnumerator CalibrateAfterXRReady()
//     {
//         yield return new WaitForSeconds(1.0f);

//         if (head == null)
//         {
//             Debug.LogError("[HeadBalanceController] Head reference not set.");
//             yield break;
//         }

//         baselineWorldY = head.position.y;
//         lowestWorldY = baselineWorldY;
//         calibrated = true;

//         Debug.Log($"[HeadBalanceController] Calibrated baseline head Y = {baselineWorldY:F3}");
//     }

//     void Update()
//     {
//         if (!calibrated || planeController == null || head == null)
//             return;

//         // Track lowest head position reached
//         // float currentY = head.position.y;
//         // lowestWorldY = Mathf.Min(lowestWorldY, currentY);
//         float currentY = head.position.y;

// // If user has stood back up close to baseline → reset bend tracking
//         if (currentY > baselineWorldY - standUpTolerance)
// {
//         baselineWorldY = currentY;
//         lowestWorldY = baselineWorldY;
// }
// else
// {
//     // Track lowest head position reached during a bend
//     lowestWorldY = Mathf.Min(lowestWorldY, currentY);
// }

//         // Compute bend amount
//         float delta = baselineWorldY - lowestWorldY;
//         float stability = Mathf.Clamp01(delta / bendThreshold);

//         int direction = 1; // keep existing tilt direction logic

//         // ─────────────────────────────────────────────
//         // ZONE LOGIC (REVERSED BEHAVIOR)
//         // Inside ONE zone:
//         //   Standing → stable
//         //   Bending  → unstable (tilts)
//         // Outside zones:
//         //   Bending stabilizes (original behavior)
//         // ─────────────────────────────────────────────
//         if (zoneDetector != null && zoneDetector.ActiveZoneCount() == 1)
//         {
//             float invertedStability = 1f - stability;
//             planeController.SetStability(invertedStability, direction);
//             return;
//         }

//         // Default behavior (non-zone)
//         planeController.SetStability(stability, direction);
//     }

//     /// <summary>
//     /// Optional: Call this to reset bend baseline (e.g., at start of a new trial)
//     /// </summary>
//     public void Recalibrate()
//     {
//         if (head == null) return;

//         baselineWorldY = head.position.y;
//         lowestWorldY = baselineWorldY;

//         Debug.Log("[HeadBalanceController] Recalibrated baseline.");
//     }
// }
