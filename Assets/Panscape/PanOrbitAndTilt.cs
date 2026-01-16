// using UnityEngine;

// // [DisallowMultipleComponent]
// public class PanOrbitAndTilt : MonoBehaviour
// {
//     // [Header("References")]
//     // [Tooltip("The pan plate that orbits and tilts. If left empty, the first child is used.")]
//     public Transform panPlate;

//     // [Header("Orbit (motion around rod)")]
//     public float orbitRadius = 0.5f;          // distance from rod axis
//     public float orbitRadiusZ = 0.5f;         // use different X/Z to create an ellipse
//     public float orbitSpeedDegrees = 60f;     // degrees per second around rod
//     public float startOrbitAngleDeg = 0f;     // starting angle (deg)

//     // [Header("Tilt (butter-spreading)")]
//     public float tiltAngleDegrees = 20f;      // max tilt angle (degrees)
//     public float tiltSpeed = 2f;              // how fast it tilts (Hz-like, higher = faster)

//     // [Header("Pan self-rotation")]
//     public float selfSpinDegreesPerSecond = 0f; // rotate pan around its local up axis (optional)

//     // internal
//     float orbitAngleRad;

//     void Start()
//     {
//         if (panPlate == null && transform.childCount > 0)
//             panPlate = transform.GetChild(0);

//         orbitAngleRad = startOrbitAngleDeg * Mathf.Deg2Rad;

//         // If panPlate had a local position, use that radius as default
//         if (panPlate != null)
//         {
//             Vector3 local = panPlate.localPosition;
//             if (Mathf.Approximately(orbitRadius, 0f) && Mathf.Approximately(orbitRadiusZ, 0f))
//             {
//                 orbitRadius = Mathf.Abs(local.x);
//                 orbitRadiusZ = Mathf.Abs(local.z);
//             }
//         }
//     }

//     void Update()
//     {
//         if (panPlate == null) return;

//         // advance orbit angle
//         float orbitDeltaRad = orbitSpeedDegrees * Mathf.Deg2Rad * Time.deltaTime;
//         orbitAngleRad += orbitDeltaRad;

//         // compute elliptical orbit local position (rod's local space)
//         float x = Mathf.Cos(orbitAngleRad) * orbitRadius;
//         float z = Mathf.Sin(orbitAngleRad) * orbitRadiusZ;
//         panPlate.localPosition = new Vector3(x, panPlate.localPosition.y, z);

//         // tilt: oscillate around pan's local X axis (or whichever axis you prefer)
//         float tilt = Mathf.Sin(Time.time * tiltSpeed) * tiltAngleDegrees;
//         Quaternion tiltRot = Quaternion.Euler(tilt, 0f, 0f);

//         // optionally add self spin about pan's up (local Y) axis
//         float spin = selfSpinDegreesPerSecond * Time.time;
//         Quaternion spinRot = Quaternion.Euler(0f, 0f, spin);

//         // combine rotations:
//         // - keep pan's "up" aligned with world/rod orientation as needed
//         // - apply tilt first, then spin (order can be swapped for different feel)
//         panPlate.localRotation = tiltRot * spinRot;
//     }
// }
using UnityEngine;

[DisallowMultipleComponent]
public class PanTiltController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The actual pan plate to tilt. If empty, the first child is used.")]
    public Transform panPlate;

    [Header("Tilt motion")]
    [Tooltip("Max tilt angle in degrees (positive means tilt right, negative left)")]
    public float maxTiltAngle = 20f;
    [Tooltip("How fast it completes one back-and-forth cycle (cycles per second)")]
    public float cyclesPerSecond = 0.25f; // 0.25 -> 4s per cycle
    [Tooltip("If true the pan continuously tilts back and forth. If false, use TriggerTilt() or ToggleTilt().")]
    public bool continuous = true;

    [Header("Timing (for non-continuous)")]
    [Tooltip("If not continuous: how long to play the tilt cycle then stop (seconds).")]
    public float oneShotDuration = 2.0f;

    [Header("Physics")]
    [Tooltip("If the pan plate has a kinematic Rigidbody, enable this so motion uses MoveRotation in FixedUpdate.")]
    public bool useRigidbodyMove = true;

    // internals
    Quaternion initialLocalRot;
    Rigidbody panRb;
    float timeSinceStart = 0f;
    bool running = false;
    float manualStartTime = 0f;

    void Reset()
    {
        // sensible defaults
        maxTiltAngle = 20f;
        cyclesPerSecond = 0.25f;
        continuous = true;
        oneShotDuration = 2f;
        useRigidbodyMove = true;
    }

    void Start()
    {
        if (panPlate == null && transform.childCount > 0)
            panPlate = transform.GetChild(0);

        if (panPlate == null)
        {
            Debug.LogWarning("[PanTiltController] No panPlate assigned or found as child.");
            enabled = false;
            return;
        }

        initialLocalRot = panPlate.localRotation;
        panRb = panPlate.GetComponent<Rigidbody>();
        if (panRb != null && !panRb.isKinematic)
        {
            Debug.LogWarning("[PanTiltController] Detected a non-kinematic Rigidbody on panPlate. For scripted motion, set isKinematic = true.");
        }

        running = continuous;
        timeSinceStart = 0f;
        manualStartTime = 0f;
    }

    void Update()
    {
        if (!running) return;
        // accumulate time for the cycle. Use unscaled time so it ignores timeScale if you want real-time feel.
        timeSinceStart += Time.deltaTime;

        if (!continuous)
        {
            if (timeSinceStart - manualStartTime >= oneShotDuration)
            {
                running = false;
                // ensure we return to rest position
                ApplyTilt(0f);
            }
        }

        // compute tilt angle using sine wave for smooth back-and-forth:
        float t = timeSinceStart * Mathf.PI * 2f * cyclesPerSecond; // angle in radians for sin
        float tilt = Mathf.Sin(t) * maxTiltAngle; // -max..+max degrees
        ApplyTilt(tilt);
    }

    void FixedUpdate()
    {
        // if using RigidbodyMove and a kinematic rigidbody exists, also update in FixedUpdate
        if (!useRigidbodyMove) return;
        if (panRb == null || !panRb.isKinematic) return;

        // replicate the same calculation but using fixed time if running
        if (!running)
        {
            // ensure final resting rotation is applied with physics
            ApplyTiltWithRigidbody(0f);
            return;
        }

        float tFixed = timeSinceStart * Mathf.PI * 2f * cyclesPerSecond;
        float tiltFixed = Mathf.Sin(tFixed) * maxTiltAngle;
        ApplyTiltWithRigidbody(tiltFixed);
    }

    void ApplyTilt(float tiltDegrees)
    {
        // tilt around local X axis (change if your pan faces different axis)
        Quaternion tiltRot = Quaternion.Euler(tiltDegrees, 0f, 0f);
        panPlate.localRotation = initialLocalRot * tiltRot;
    }

    void ApplyTiltWithRigidbody(float tiltDegrees)
    {
        // compute target world rotation and move with Rigidbody
        Quaternion targetLocal = initialLocalRot * Quaternion.Euler(tiltDegrees, 0f, 0f);
        Quaternion worldRot = panPlate.parent != null ? panPlate.parent.rotation * targetLocal : transform.rotation * targetLocal;
        Vector3 worldPos = panPlate.parent != null ? panPlate.parent.TransformPoint(panPlate.localPosition) : transform.TransformPoint(panPlate.localPosition);

        panRb.MovePosition(worldPos);   // keeps position identical
        panRb.MoveRotation(worldRot);
    }

    /// <summary>Trigger one tilt cycle (works only when continuous=false)</summary>
    public void TriggerTilt()
    {
        if (continuous)
        {
            running = true; // will stay running
            return;
        }
        running = true;
        manualStartTime = timeSinceStart;
    }

    /// <summary>Toggle continuous tilting on/off</summary>
    public void ToggleTilt()
    {
        continuous = !continuous;
        running = continuous;
        if (running)
        {
            manualStartTime = timeSinceStart;
        }
        else
        {
            // stop and reset rotation
            ApplyTilt(0f);
        }
    }

    /// <summary>Stop tilting and reset to rest position immediately.</summary>
    public void StopAndReset()
    {
        running = false;
        continuous = false;
        timeSinceStart = 0f;
        ApplyTilt(0f);
    }
}
