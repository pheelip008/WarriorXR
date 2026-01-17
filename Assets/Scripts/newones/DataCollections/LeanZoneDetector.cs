using UnityEngine;

public class LeanZoneDetector : MonoBehaviour
{
    [Header("Thresholds")]
    public float xThreshold = 0.15f;
    public float heightThreshold = 0.05f;

    public string GetActiveZone()
    {
        float headX = HeadTracker.Instance.GetHeadX();
        float headY = HeadTracker.Instance.GetHeadY();
        float centerY = HeadTracker.Instance.centerY;

        if (headY < centerY - heightThreshold)
        {
            if (headX > xThreshold)
                return "Right";
            else if (headX < -xThreshold)
                return "Left";
        }

        return "None";
    }
}
