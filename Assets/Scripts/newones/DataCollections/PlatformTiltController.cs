using UnityEngine;

public class PlatformTiltController : MonoBehaviour
{
    public float tiltAngle = 4f;
    public float smoothSpeed = 5f;

    float currentTilt = 0f;

    void Update()
    {
        if (ExperimentConfig.Instance.condition == "Static")
        {
            SetTilt(0f);
            return;
        }

        string zone = FindObjectOfType<LeanZoneDetector>().GetActiveZone();

        if (zone == "Left")
            SetTilt(-tiltAngle);
        else if (zone == "Right")
            SetTilt(tiltAngle);
        else
            SetTilt(0f);
    }

    void SetTilt(float target)
    {
        currentTilt = Mathf.Lerp(currentTilt, target, Time.deltaTime * smoothSpeed);
        transform.localRotation = Quaternion.Euler(0, 0, currentTilt);
    }

    public float GetCurrentTilt()
    {
        return currentTilt;
    }
}
