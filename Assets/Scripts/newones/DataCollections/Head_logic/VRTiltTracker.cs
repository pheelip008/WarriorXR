using UnityEngine;

public class VRTiltTracker : MonoBehaviour
{
    [Tooltip("The VR camera (usually the Main Camera inside XR Origin)")]
    public Transform headTransform;
    
    [Tooltip("Tilt angle threshold in degrees")]
    public float tiltThreshold = 45f;

    void Update()
    {
        if (headTransform == null) return;

        // Get the local rotation on the Z-axis (sideways tilt/roll)
        float currentTilt = headTransform.localEulerAngles.z;

        // Unity eulerAngles range from 0 to 360. 
        // We convert this to a -180 to 180 range for easier threshold checking.
        if (currentTilt > 180) currentTilt -= 360;

        // Check if the absolute tilt is above the 45-degree threshold
        if (Mathf.Abs(currentTilt) > tiltThreshold)
        {
            Debug.Log($"Head tilted sideways! Current Angle: {currentTilt:F2}°");
        }
    }
}
