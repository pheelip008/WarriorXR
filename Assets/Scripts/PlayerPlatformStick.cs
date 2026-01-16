using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerPlatformStick : MonoBehaviour
{
    [Header("Ground Check")]
    public LayerMask platformLayer;
    public float groundCheckDistance = 0.3f;

    Transform currentPlatform;
    CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        HandlePlatformStick();
    }

    void HandlePlatformStick()
    {
        RaycastHit hit;

        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        bool groundedOnPlatform = Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out hit,
            groundCheckDistance,
            platformLayer
        );

        if (groundedOnPlatform)
        {
            if (currentPlatform != hit.transform)
            {
                AttachToPlatform(hit.transform);
            }
        }
        else
        {
            DetachFromPlatform();
        }
    }

    void AttachToPlatform(Transform platform)
    {
        currentPlatform = platform;
        transform.SetParent(platform, true);
    }

    void DetachFromPlatform()
    {
        if (currentPlatform != null)
        {
            transform.SetParent(null, true);
            currentPlatform = null;
        }
    }
}

