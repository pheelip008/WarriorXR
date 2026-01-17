using UnityEngine;

public class HeadBendZoneController_Part2 : MonoBehaviour
{
    [Header("References")]
    public Transform head;                 // Main Camera
    public Transform cameraOffset;         // Camera Offset
    public HeadZoneDetector zoneDetector;
    public PlaneBendController_Part2 planeController;
    public static HeadBendZoneController_Part2 Instance;

    [Header("Bend Threshold")]
    public float bendThreshold = 0.02f;    // meters

    float baselineHeight;
    bool calibrated;

    void Start()
    {
        Calibrate();
    }
    void Awake()
    {
        Instance = this;
    }

    public void Calibrate()
    {
        baselineHeight = head.position.y - cameraOffset.position.y;
        calibrated = true;
        Debug.Log($"[Part2] Baseline height: {baselineHeight:F3}");
    }
    

    void Update()
    {
        if (!calibrated || planeController == null || zoneDetector == null)
            return;

        float currentHeight = head.position.y - cameraOffset.position.y;
        float delta = baselineHeight - currentHeight;

        float bendAmount = Mathf.Clamp01(delta / bendThreshold);

        // --- ZONE RULES ---
        if (zoneDetector.ActiveZoneCount() != 1)
        {
            // No zone or both zones → no bending
            planeController.SetBend(0f, 0);
            return;
        }

        if (zoneDetector.inLeftZone)
        {
            // Left zone → +X bend
            planeController.SetBend(bendAmount, +1);
        }
        else if (zoneDetector.inRightZone)
        {
            // Right zone → -X bend
            planeController.SetBend(bendAmount, -1);
        }
    }
}
