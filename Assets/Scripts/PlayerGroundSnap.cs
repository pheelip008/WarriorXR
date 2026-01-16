using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerGroundSnap : MonoBehaviour
{
    public LayerMask groundLayer;
    public float raycastDistance = 2f;
    public float groundOffset = 0.02f;

    CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        SnapToGround();
    }

    void SnapToGround()
    {
        RaycastHit hit;

        // Cast from capsule center, not transform origin
        Vector3 origin = controller.bounds.center + Vector3.up * 0.1f;

        if (Physics.Raycast(origin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            // Bottom of capsule in world space
            float capsuleBottomY = controller.bounds.min.y;

            float desiredBottomY = hit.point.y + groundOffset;

            float delta = desiredBottomY - capsuleBottomY;

            // Only correct DOWNWARD penetration
            if (delta > 0f)
            {
                transform.position += Vector3.up * delta;
            }
        }
    }
}
