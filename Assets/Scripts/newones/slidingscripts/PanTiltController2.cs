// using UnityEngine;
// using System.Collections;

// public class PanTiltController2 : MonoBehaviour
// {
//     public enum Axis { LocalX, LocalZ }

//     [Header("Tilt Settings")]
//     public Axis tiltAxis = Axis.LocalX;
//     public float startAngle = 0f;          // initial rotation angle (degrees)
//     public float maxAngle = 30f;           // maximum tilt (degrees)
//     public float stepAngle = 5f;           // how much to increase each step (degrees)
//     public float stepInterval = 20f;       // seconds between each step
//     public float rotationSmoothTime = 0.5f;// smooth interpolation time
//     public bool autoStart = true;          // start auto-increasing at Start

//     // internal
//     float targetAngle;
//     float currentVelocity = 0f; // for SmoothDampAngle

//     void Start()
//     {
//         targetAngle = Mathf.Clamp(startAngle, -maxAngle, maxAngle);
//         SetAngleInstant(targetAngle);
//         if (autoStart) StartCoroutine(AutoStepRoutine());
//     }

//     void Update()
//     {
//         // Smoothly interpolate current angle towards targetAngle
//         Vector3 localEuler = transform.localEulerAngles;
//         float current = GetAxisAngle(localEuler);
//         float newAngle = Mathf.SmoothDampAngle(current, targetAngle, ref currentVelocity, rotationSmoothTime);
//         SetAxisAngle(ref localEuler, newAngle);
//         transform.localEulerAngles = localEuler;
//     }

//     IEnumerator AutoStepRoutine()
//     {
//         // Wait full interval before first increase (optional)
//         while (Mathf.Abs(targetAngle) < Mathf.Abs(maxAngle))
//         {
//             yield return new WaitForSeconds(stepInterval);
//             float next = targetAngle + Mathf.Sign(stepAngle) * Mathf.Abs(stepAngle);
//             // clamp toward maxAngle keeping sign of stepAngle if user used negative steps
//             if (stepAngle >= 0)
//                 targetAngle = Mathf.Clamp(next, -Mathf.Abs(maxAngle), Mathf.Abs(maxAngle));
//             else
//                 targetAngle = Mathf.Clamp(next, -Mathf.Abs(maxAngle), Mathf.Abs(maxAngle));
//             // if reached max, break
//             if (Mathf.Approximately(Mathf.Abs(targetAngle), Mathf.Abs(maxAngle))) break;
//         }
//     }

//     // Public method to set target angle directly
//     public void SetTargetAngle(float angleDegrees)
//     {
//         targetAngle = Mathf.Clamp(angleDegrees, -Mathf.Abs(maxAngle), Mathf.Abs(maxAngle));
//     }

//     void SetAngleInstant(float angleDegrees)
//     {
//         Vector3 e = transform.localEulerAngles;
//         SetAxisAngle(ref e, angleDegrees);
//         transform.localEulerAngles = e;
//     }

//     float GetAxisAngle(Vector3 euler)
//     {
//         if (tiltAxis == Axis.LocalX) return NormalizeAngle(euler.x);
//         return NormalizeAngle(euler.z);
//     }

//     void SetAxisAngle(ref Vector3 euler, float angle)
//     {
//         if (tiltAxis == Axis.LocalX) euler.x = angle;
//         else euler.z = angle;
//     }

//     float NormalizeAngle(float a)
//     {
//         // returns angle in -180..180
//         a = Mathf.Repeat(a + 180f, 360f) - 180f;
//         return a;
//     }
// }
// using UnityEngine;
// using System.Collections;

// public class PanTiltController2 : MonoBehaviour
// {
//     public enum Axis { LocalX, LocalZ }

//     [Header("Tilt Settings")]
//     public Axis tiltAxis = Axis.LocalX;

//     [Tooltip("Starting amplitude in degrees (absolute).")]
//     public float startAmplitude = 0f;   // absolute amplitude
//     [Tooltip("Maximum amplitude allowed (absolute).")]
//     public float maxAmplitude = 45f;    // absolute cap
//     [Tooltip("How much amplitude increases each step (degrees).")]
//     public float stepAngle = 5f;
//     [Tooltip("Seconds between each tilt change.")]
//     public float stepInterval = 20f;
//     [Tooltip("Time (s) it takes to smoothly move to the new target angle.")]
//     public float rotationSmoothTime = 0.6f;

//     [Header("Behaviour")]
//     [Tooltip("If true, first step goes to the left (negative), otherwise right (positive).")]
//     public bool startLeft = true;
//     [Tooltip("If true, the pan will keep alternating forever (amplitude increases each step until capped).")]
//     public bool loopForever = true;
//     [Tooltip("If true, first step happens immediately on Start; otherwise waits stepInterval.")]
//     public bool firstStepImmediate = false;

//     // internal state
//     float currentAmplitude;
//     bool currentIsNegative; // direction of next target (true => negative/left)
//     float targetAngle;      // signed target angle
//     float smoothVelocity = 0f;

//     void Start()
//     {
//         currentAmplitude = Mathf.Clamp(startAmplitude, 0f, Mathf.Abs(maxAmplitude));
//         currentIsNegative = startLeft;
//         // set initial rotation to startAmplitude but neutral direction (0) or start direction?
//         // we'll set initial to zero-angle or to start amplitude depending on firstStepImmediate:
//         if (!firstStepImmediate)
//         {
//             SetAngleInstant(0f);
//             if (loopForever) StartCoroutine(AutoStepRoutine());
//         }
//         else
//         {
//             // do first step immediately (will also start coroutine for subsequent steps)
//             StepOnce();
//             if (loopForever) StartCoroutine(AutoStepRoutine());
//         }
//     }

//     void Update()
//     {
//         // Smoothly move current local Euler towards targetAngle
//         Vector3 localEuler = transform.localEulerAngles;
//         float current = GetAxisAngle(localEuler);
//         float newAngle = Mathf.SmoothDampAngle(current, targetAngle, ref smoothVelocity, rotationSmoothTime);
//         SetAxisAngle(ref localEuler, newAngle);
//         transform.localEulerAngles = localEuler;
//     }

//     IEnumerator AutoStepRoutine()
//     {
//         // If firstStepImmediate was true, we've already done a step; otherwise wait before first step
//         if (!firstStepImmediate) yield return new WaitForSeconds(stepInterval);

//         while (true)
//         {
//             StepOnce();

//             // if amplitude reached maxAmplitude and no more increase possible, we still alternate but amplitude will be clamped
//             if (!loopForever)
//             {
//                 // If not looping forever, stop after one full left+right cycle (optional behaviour).
//                 // For simplicity here, stop after one step when loopForever is false.
//                 yield break;
//             }

//             yield return new WaitForSeconds(stepInterval);
//         }
//     }

//     void StepOnce()
//     {
//         // Increase amplitude (but ensure we don't exceed maxAmplitude)
//         currentAmplitude = Mathf.Clamp(currentAmplitude + stepAngle, 0f, Mathf.Abs(maxAmplitude));

//         // Determine signed target for this step (positive = right, negative = left)
//         float signedAmplitude = currentIsNegative ? -currentAmplitude : currentAmplitude;
//         targetAngle = signedAmplitude;

//         // Toggle direction for next step
//         currentIsNegative = !currentIsNegative;
//     }

//     // Public method to manually trigger a step (useful for testing)
//     public void TriggerStep()
//     {
//         StepOnce();
//     }

//     // Set target angle directly (clamped by maxAmplitude)
//     public void SetTargetAngle(float angleDegrees)
//     {
//         float clamped = Mathf.Clamp(angleDegrees, -Mathf.Abs(maxAmplitude), Mathf.Abs(maxAmplitude));
//         targetAngle = clamped;
//     }

//     // Instantly set local rotation on the chosen axis
//     void SetAngleInstant(float angleDegrees)
//     {
//         Vector3 e = transform.localEulerAngles;
//         SetAxisAngle(ref e, angleDegrees);
//         transform.localEulerAngles = e;
//         targetAngle = angleDegrees;
//     }

//     float GetAxisAngle(Vector3 euler)
//     {
//         if (tiltAxis == Axis.LocalX) return NormalizeAngle(euler.x);
//         return NormalizeAngle(euler.z);
//     }

//     void SetAxisAngle(ref Vector3 euler, float angle)
//     {
//         if (tiltAxis == Axis.LocalX) euler.x = angle;
//         else euler.z = angle;
//     }

//     float NormalizeAngle(float a)
//     {
//         a = Mathf.Repeat(a + 180f, 360f) - 180f;
//         return a;
//     }
// }
using UnityEngine;
using System.Collections;

public class PanTiltController2 : MonoBehaviour
{
    public enum Axis { LocalX, LocalZ }

    [Header("Tilt Settings")]
    public Axis tiltAxis = Axis.LocalX;

    [Tooltip("Starting amplitude in degrees.")]
    public float startAmplitude = 0f;

    [Tooltip("Maximum amplitude allowed.")]
    public float maxAmplitude = 45f;

    [Tooltip("Degrees added to amplitude every step.")]
    public float stepAngle = 5f;

    [Tooltip("Seconds between tilt direction changes.")]
    public float stepInterval = 20f;

    [Header("Transition Speed")]
    [Tooltip("How fast the pan tilts toward the new angle (degrees per second).")]
    public float tiltSpeed = 10f; // NEW — controls how slow/smooth transitions are

    [Header("Behaviour")]
    public bool startLeft = true;
    public bool loopForever = true;
    public bool firstStepImmediate = false;

    float currentAmplitude;
    bool currentIsNegative;
    float targetAngle;

    void Start()
    {
        currentAmplitude = Mathf.Clamp(startAmplitude, 0f, Mathf.Abs(maxAmplitude));
        currentIsNegative = startLeft;

        if (!firstStepImmediate)
        {
            SetAngleInstant(0f);
            if (loopForever) StartCoroutine(AutoStepRoutine());
        }
        else
        {
            StepOnce();
            if (loopForever) StartCoroutine(AutoStepRoutine());
        }
    }

    void Update()
    {
        // --- Slow & smooth rotation ---
        Vector3 euler = transform.localEulerAngles;
        float current = GetAxisAngle(euler);

        float newAngle = Mathf.MoveTowardsAngle(
            current,
            targetAngle,
            tiltSpeed * Time.deltaTime         // <-- controlled speed
        );

        SetAxisAngle(ref euler, newAngle);
        transform.localEulerAngles = euler;
    }

    IEnumerator AutoStepRoutine()
    {
        if (!firstStepImmediate)
            yield return new WaitForSeconds(stepInterval);

        while (true)
        {
            StepOnce();
            yield return new WaitForSeconds(stepInterval);
        }
    }

    void StepOnce()
    {
        // Increase amplitude but clamp
        currentAmplitude = Mathf.Clamp(currentAmplitude + stepAngle, 0f, Mathf.Abs(maxAmplitude));

        // Choose direction
        float signed = currentIsNegative ? -currentAmplitude : currentAmplitude;
        targetAngle = signed;

        currentIsNegative = !currentIsNegative; // flip for next step
    }

    // Utility helpers
    void SetAngleInstant(float angle)
    {
        Vector3 e = transform.localEulerAngles;
        SetAxisAngle(ref e, angle);
        transform.localEulerAngles = e;
        targetAngle = angle;
    }

    float GetAxisAngle(Vector3 e)
    {
        return NormalizeAngle(tiltAxis == Axis.LocalX ? e.x : e.z);
    }

    void SetAxisAngle(ref Vector3 e, float angle)
    {
        if (tiltAxis == Axis.LocalX) e.x = angle;
        else e.z = angle;
    }

    float NormalizeAngle(float a)
    {
        return Mathf.Repeat(a + 180f, 360f) - 180f;
    }
}

