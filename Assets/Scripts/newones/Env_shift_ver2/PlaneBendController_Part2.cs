using UnityEngine;

public class PlaneBendController_Part2 : MonoBehaviour
{
    [Header("Bend Settings")]
    public float maxBendAngle = 12f;   // degrees
    public float rotationSpeed = 2f;   // smoothing

    float bendAmount = 0f;  // 0 → 1
    int bendDirection = 0;  // -1, 0, +1

    Quaternion neutralRotation;

    void Start()
    {
        neutralRotation = transform.rotation;
    }

    public void SetBend(float amount, int direction)
    {
        bendAmount = Mathf.Clamp01(amount);
        bendDirection = Mathf.Clamp(direction, -1, 1);
    }

    void Update()
    {
        float targetAngle = maxBendAngle * bendAmount * bendDirection;

        Quaternion targetRotation =
            Quaternion.Euler(targetAngle, 0f, 0f) * neutralRotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}
