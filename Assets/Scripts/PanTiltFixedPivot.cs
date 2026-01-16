using UnityEngine;

/// <summary>
/// Attach this to the Pan GameObject (the mesh root).
/// It will create a pivot at the handle (or guess one), parent the pan to that pivot,
/// and rotate the pivot so the pan tilts without changing world height/position.
/// </summary>
[DisallowMultipleComponent]
public class PanTiltFixedPivot : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Handle / Pivot")]
    [Tooltip("Optional: assign a transform located at the handle. If empty, script will attempt to guess the handle location using renderers.")]
    public Transform handleTransform;

    [Tooltip("If true and handleTransform is null, script will try to auto-locate a handle using mesh bounds.")]
    public bool autoGuessHandle = true;

    [Header("Tilt Settings")]
    public Axis rotateAxis = Axis.Z;
    public float maxTiltAngle = 25f;
    public float tiltSpeed = 1.2f;
    [Tooltip("If true, pan will tilt automatically with a sine wave. If false, call SetTiltAngle externally.")]
    public bool autoTilt = true;

    // created pivot
    Transform pivot;
    float timeCounter = 0f;

    // store rigidbody settings so we can restore if needed
    Rigidbody panRb;
    bool changedKinematic = false;
    RigidbodyConstraints originalConstraints = RigidbodyConstraints.None;

    void Start()
    {
        // make sure the object has at least one renderer to guess bounds if needed
        Vector3 pivotWorldPos = Vector3.zero;
        bool pivotFound = false;

        if (handleTransform != null)
        {
            pivotWorldPos = handleTransform.position;
            pivotFound = true;
        }
        else if (autoGuessHandle)
        {
            // try to compute a reasonable handle location using renderers' bounds
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                // compute combined bounds
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

                // choose local axis direction to search for handle: user expects handle along local right (X) or forward (Z)
                // We try both: pick the extreme on local +X and local -X and assume handle is the furthest extent on X
                Vector3 localRight = transform.TransformDirection(Vector3.right);
                Vector3 localForward = transform.TransformDirection(Vector3.forward);

                // sample points on the mesh approx: take center plus extents along local axes
                Vector3 candidate1 = bounds.center + localRight * bounds.extents.x;
                Vector3 candidate2 = bounds.center - localRight * bounds.extents.x;
                Vector3 candidate3 = bounds.center + localForward * bounds.extents.z;
                Vector3 candidate4 = bounds.center - localForward * bounds.extents.z;

                // choose candidate that is farthest from the bounds center (heuristic for handle)
                float d1 = Vector3.Distance(candidate1, bounds.center);
                float d2 = Vector3.Distance(candidate2, bounds.center);
                float d3 = Vector3.Distance(candidate3, bounds.center);
                float d4 = Vector3.Distance(candidate4, bounds.center);

                pivotWorldPos = candidate1;
                float max = d1;
                if (d2 > max) { pivotWorldPos = candidate2; max = d2; }
                if (d3 > max) { pivotWorldPos = candidate3; max = d3; }
                if (d4 > max) { pivotWorldPos = candidate4; max = d4; }

                pivotFound = true;
            }
        }

        if (!pivotFound)
        {
            // fallback: use current transform position (will rotate around object center)
            pivotWorldPos = transform.position;
            Debug.LogWarning("[PanTiltFixedPivot] Could not find handle or renderers to guess handle. Using object center as pivot.");
        }

        // create pivot GameObject at pivotWorldPos
        GameObject pivotGO = new GameObject($"{name}_HandlePivot");
        pivot = pivotGO.transform;
        pivot.position = pivotWorldPos;

        // align pivot rotation to the pan's rotation so local axes match
        pivot.rotation = transform.rotation;

        // parent the pan to pivot but keep world position/rotation
        transform.SetParent(pivot, true);

        // if pan has a Rigidbody, make it kinematic so transform manipulation won't fight physics
        panRb = GetComponent<Rigidbody>();
        if (panRb != null)
        {
            originalConstraints = panRb.constraints;
            if (!panRb.isKinematic)
            {
                panRb.isKinematic = true;
                changedKinematic = true;
            }
            // freeze position so physics does not move it externally (rotation through pivot will still visually rotate)
            panRb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }
    }

    void OnDestroy()
    {
        // restore rigidbody if we changed it
        if (panRb != null)
        {
            if (changedKinematic) panRb.isKinematic = false;
            panRb.constraints = originalConstraints;
        }
    }

    void Update()
    {
        if (pivot == null) return;

        if (autoTilt)
        {
            timeCounter += Time.deltaTime;
            float t = Mathf.Sin(timeCounter * tiltSpeed);
            float angle = t * maxTiltAngle;
            ApplyLocalRotationToPivot(angle);
        }
    }

    /// <summary>
    /// externally callable method to set a specific tilt angle (in degrees)
    /// </summary>
    /// <param name="angleDegrees"></param>
    public void SetTiltAngle(float angleDegrees)
    {
        ApplyLocalRotationToPivot(Mathf.Clamp(angleDegrees, -maxTiltAngle, maxTiltAngle));
    }

    void ApplyLocalRotationToPivot(float angle)
    {
        Vector3 euler = Vector3.zero;
        switch (rotateAxis)
        {
            case Axis.X: euler = new Vector3(angle, 0f, 0f); break;
            case Axis.Y: euler = new Vector3(0f, angle, 0f); break;
            case Axis.Z: euler = new Vector3(0f, 0f, angle); break;
        }

        // apply rotation to pivot's local rotation; this won't change pivot position, so pan won't "lift"
        pivot.localRotation = Quaternion.Euler(euler);
    }
}
