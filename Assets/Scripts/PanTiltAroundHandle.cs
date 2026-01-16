using UnityEngine;

[DisallowMultipleComponent]
public class PanTiltHandleRigidSafe : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Tilt Settings")]
    public Axis rotateAxis = Axis.Z;
    public float maxTiltAngle = 25f;
    public float tiltSpeed = 1.2f;
    public bool autoTilt = true;

    [Header("Rigidbody Options")]
    [Tooltip("If true and no Rigidbody is found on this pivot, the script will add a kinematic Rigidbody to the pivot.")]
    public bool addKinematicRigidbodyIfMissing = true;

    // internals
    private Vector3 fixedWorldPos;
    private float timeCounter = 0f;
    private float currentAngle = 0f;

    // rigidbody we'll use (may be added by this script)
    private Rigidbody rb;
    private bool rbAddedByScript = false;

    void Start()
    {
        // store fixed world position
        fixedWorldPos = transform.position;

        // check up the parent chain for other rigidbodies
        Transform p = transform.parent;
        while (p != null)
        {
            if (p.GetComponent<Rigidbody>() != null)
            {
                Debug.LogWarning("[PanTiltHandleRigidSafe] Found Rigidbody on parent '" + p.name + "'. " +
                                 "Physics-driven parents can interfere with child Rigidbody or transform locking. " +
                                 "Consider placing the Rigidbody on the parent pivot instead or make parent kinematic.");
                break;
            }
            p = p.parent;
        }

        // find Rigidbody on this pivot
        rb = GetComponent<Rigidbody>();
        if (rb == null && addKinematicRigidbodyIfMissing)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rbAddedByScript = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            Debug.Log("[PanTiltHandleRigidSafe] Added a kinematic Rigidbody to pivot for physics-safe rotation.");
        }

        // If a Rigidbody exists but is non-kinematic, recommend making it kinematic for scripted control
        if (rb != null && !rb.isKinematic)
        {
            Debug.LogWarning("[PanTiltHandleRigidSafe] This pivot's Rigidbody is non-kinematic. For scripted MoveRotation/MovePosition control, set Rigidbody.isKinematic = true or use physics-based forces. The script will still attempt to use MoveRotation, but conflicts may occur.");
        }
    }

    void Update()
    {
        if (autoTilt)
        {
            timeCounter += Time.deltaTime;
            float t = Mathf.Sin(timeCounter * tiltSpeed);
            float angle = t * maxTiltAngle;
            SetTiltAngle(angle);
        }
    }

    /// <summary>
    /// Set tilt angle in degrees. Works with both Rigidbody and transform paths.
    /// </summary>
    public void SetTiltAngle(float angleDegrees)
    {
        currentAngle = Mathf.Clamp(angleDegrees, -maxTiltAngle, maxTiltAngle);
    }

    void FixedUpdate()
    {
        // If we have a Rigidbody on this pivot, apply rotation/position using Rigidbody API (physics-safe)
        if (rb != null)
        {
            // ensure pivot stays at fixed world pos
            rb.MovePosition(fixedWorldPos);

            // compute desired local rotation quaternion from currentAngle
            Quaternion localRot = Quaternion.identity;
            switch (rotateAxis)
            {
                case Axis.X: localRot = Quaternion.Euler(currentAngle, 0f, 0f); break;
                case Axis.Y: localRot = Quaternion.Euler(0f, currentAngle, 0f); break;
                case Axis.Z: localRot = Quaternion.Euler(0f, 0f, currentAngle); break;
            }

            // convert to desired world rotation (parent rotation * localRot) if there is a parent
            Quaternion desiredWorldRot = (transform.parent != null) ? transform.parent.rotation * localRot : localRot;

            // use MoveRotation to avoid teleporting the Rigidbody and to play nice with physics
            rb.MoveRotation(desiredWorldRot);
        }
    }

    void LateUpdate()
    {
        // If no rigidbody, fallback: directly set local rotation then restore world position
        if (rb == null)
        {
            // apply local rotation
            Vector3 euler = Vector3.zero;
            switch (rotateAxis)
            {
                case Axis.X: euler = new Vector3(currentAngle, 0f, 0f); break;
                case Axis.Y: euler = new Vector3(0f, currentAngle, 0f); break;
                case Axis.Z: euler = new Vector3(0f, 0f, currentAngle); break;
            }
            transform.localRotation = Quaternion.Euler(euler);

            // restore world position to prevent any translation caused by parenting/animations/etc.
            transform.position = fixedWorldPos;
        }
    }

    void OnDestroy()
    {
        // If we added a Rigidbody at runtime, optionally remove it to clean up (commented out by default)
        // if (rbAddedByScript && rb != null) Destroy(rb);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // keep fixedWorldPos updated in editor for convenience (only while editing)
        if (!Application.isPlaying && transform != null)
        {
            fixedWorldPos = transform.position;
        }
    }
#endif
}
