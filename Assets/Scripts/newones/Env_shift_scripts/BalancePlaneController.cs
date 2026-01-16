// using UnityEngine;

// public class BalancePlaneController : MonoBehaviour
// {
//     [Header("Tilt Settings")]
//     public float maxTiltAngle = 15f;
//     public float tiltSpeed = 1.5f;

//     float currentStability = 0f;

//     public void SetStability(float stability)
//     {
//         currentStability = stability;
//     }

//     void Update()
//     {
//         // Instability decreases as stability increases
//         float targetTilt = Mathf.Lerp(maxTiltAngle, 0f, currentStability);

//         float xTilt = Mathf.Sin(Time.time) * targetTilt;
//         float zTilt = Mathf.Cos(Time.time * 0.7f) * targetTilt;

//         Quaternion targetRot =
//             Quaternion.Euler(xTilt, 0f, zTilt);

//         transform.rotation =
//             Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * tiltSpeed);
//     }
// }
// using UnityEngine;

// public class BalancePlaneController : MonoBehaviour
// {
//     public enum TiltAxis
//     {
//         X,
//         Z
//     }

//     [Header("Tilt Setup")]
//     public TiltAxis tiltAxis = TiltAxis.X;
//     public float unstableAngle = 12f;   // degrees
//     public float rotationSpeed = 2f;    // smoothing

//     float stability = 0f;

//     Quaternion unstableRotation;
//     Quaternion stableRotation;

//     void Start()
//     {
//         stableRotation = Quaternion.identity;

//         if (tiltAxis == TiltAxis.X)
//             unstableRotation = Quaternion.Euler(unstableAngle, 0f, 0f);
//         else
//             unstableRotation = Quaternion.Euler(0f, 0f, unstableAngle);
//     }

//     public void SetStability(float value)
//     {
//         stability = Mathf.Clamp01(value);
//     }

//     void Update()
//     {
//         // 0 = unstable, 1 = stable
//         Quaternion target =
//             Quaternion.Slerp(unstableRotation, stableRotation, stability);

//         transform.rotation = Quaternion.Slerp(
//             transform.rotation,
//             target,
//             Time.deltaTime * rotationSpeed
//         );
//     }
// }

using UnityEngine;

public class BalancePlaneController : MonoBehaviour
{
    public enum TiltAxis { X, Z }

    [Header("Tilt Setup")]
    public TiltAxis tiltAxis = TiltAxis.X;
    public float unstableAngle = 12f;
    public float rotationSpeed = 2f;

    float stability = 0f;     // 0 = unstable, 1 = stable
    int direction = 1;        // -1 = left, +1 = right

    Quaternion stableRotation;

    void Start()
    {
        // Use editor rotation as stable reference
        stableRotation = transform.rotation;
    }

    public void SetStability(float value, int tiltDirection)
    {
        stability = Mathf.Clamp01(value);
        direction = Mathf.Clamp(tiltDirection, -1, 1);
    }

    void Update()
    {
        float signedAngle = unstableAngle * direction;

        Quaternion unstableRotation;

        if (tiltAxis == TiltAxis.X)
            unstableRotation = Quaternion.Euler(signedAngle, 0f, 0f);
        else
            unstableRotation = Quaternion.Euler(0f, 0f, signedAngle);

        Quaternion target =
            Quaternion.Slerp(unstableRotation, stableRotation, stability);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target,
            Time.deltaTime * rotationSpeed
        );
    }
}
