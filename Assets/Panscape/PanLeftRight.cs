using UnityEngine;

public class PanLeftRight : MonoBehaviour
{
    public float rotationAngle = 20f;   // How far left/right it rotates
    public float speed = 2f;            // Speed of the motion

    private float startRotationY;

    void Start()
    {
        startRotationY = transform.localEulerAngles.y;
    }

    void Update()
    {
        float rotation = Mathf.Sin(Time.time * speed) * rotationAngle;
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            startRotationY + rotation,
            transform.localEulerAngles.z
        );
    }
}
