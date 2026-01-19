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

using UnityEngine;

public class HeadBendZoneController_Part2 : MonoBehaviour
{
    [Header("References")]
    public Transform head;                 // Main Camera
    public Transform cameraOffset;         // Camera Offset
    public HeadZoneDetector zoneDetector;
    public PlaneBendController_Part2 planeController;
    public static HeadBendZoneController_Part2 Instance;

    [Header("Bend Thresholds")]
    public float bendThreshold = 0.02f;        // meters DOWN
    public float upperBendThreshold = 0.005f;  // meters UP

    [Header("UI Feedback")]
    public GameObject successUI;

    float baselineHeight;
    bool calibrated;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Calibrate();
    }

    public void Calibrate()
    {
        baselineHeight = head.position.y;
        calibrated = true;

        Debug.Log($"[Part2] Baseline height recorded: {baselineHeight:F3}");
    }

    void Update()
    {
        if (!calibrated || planeController == null || zoneDetector == null)
            return;

        float currentHeight = head.position.y;

        /* ==============================
         * 1. UPPER THRESHOLD (SUCCESS UI)
         * ============================== */
        bool aboveUpper =
            currentHeight > baselineHeight + upperBendThreshold;

        if (successUI != null && successUI.activeSelf != aboveUpper)
            successUI.SetActive(aboveUpper);

        /* ==============================
         * 2. LOWER THRESHOLD (BEND LOGIC)
         * ============================== */
        if (currentHeight > baselineHeight - bendThreshold)
        {
            // Not bent enough → reset plane
            planeController.SetBend(0f, 0);
            return;
        }

        // float bendAmount =
        //     Mathf.Clamp01((baselineHeight - currentHeight) / bendThreshold);
        float downwardDisplacement =
    Mathf.Max(0f, baselineHeight - currentHeight);

float bendAmount =
    Mathf.Clamp01(downwardDisplacement / bendThreshold);


        // --- ZONE RULES ---
        if (zoneDetector.ActiveZoneCount() != 1)
        {
            planeController.SetBend(0f, 0);
            return;
        }

        if (zoneDetector.inLeftZone)
        {
            planeController.SetBend(bendAmount, +1);
        }
        else if (zoneDetector.inRightZone)
        {
            planeController.SetBend(bendAmount, -1);
        }
    }
}

