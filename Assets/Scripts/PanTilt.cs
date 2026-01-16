using UnityEngine;

public class PanTiltProper : MonoBehaviour
{
    [Header("Tilt Settings")]
    public float maxTiltAngle = 25f;
    public float tiltSpeed = 1.5f;

    void Update()
    {
        // sinusoidal tilt movement
        float tilt = Mathf.Sin(Time.time * tiltSpeed);

        // Rotate around LOCAL Z axis like a real pan tilt
        transform.localRotation = Quaternion.Euler(0f, 0f, tilt * maxTiltAngle);
    }
}
