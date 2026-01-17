using UnityEngine;

public class HeadTracker : MonoBehaviour
{
    [Header("References")]
    public Transform headTransform;

    [Header("Calibration")]
    public float centerX;
    public float centerY;

    public static HeadTracker Instance;

    void Awake()
    {
        Instance = this;
    }

    public void Calibrate()
    {
        centerX = headTransform.position.x;
        centerY = headTransform.position.y;
    }

    public float GetHeadX()
    {
        return headTransform.position.x - centerX;
    }

    public float GetHeadY()
    {
        return headTransform.position.y;
    }
}
