using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("How fast it spins. Higher = Faster.")]
    public float speed = 50f;

    [Tooltip("Check this to spin on X. Uncheck to use Z (or others).")]
    public bool spinOnX = true;

    [Tooltip("Check this to spin on Z.")]
    public bool spinOnZ = false;

    void Update()
    {
        // 1. Determine which direction to spin
        Vector3 direction = Vector3.zero;

        if (spinOnX) direction.x = 1;
        if (spinOnZ) direction.z = 1;

        // 2. Apply the rotation
        // This runs every frame, adding rotation incrementally
        transform.Rotate(direction * speed * Time.deltaTime);
    }
}