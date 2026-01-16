// // using UnityEngine;

// // [RequireComponent(typeof(CharacterController))]
// // public class CharacterControllerSlopeSlider : MonoBehaviour
// // {
// //     public LayerMask groundLayer = ~0;
// //     public float sphereCastRadius = 0.2f;
// //     public float sphereCastDistance = 0.8f;

// //     [Header("Slope")]
// //     public float minSlideAngle = 15f;
// //     public float maxSlideSpeed = 6f;
// //     public float slideGravityMultiplier = 1f;
// //     public float slideSmoothTime = 0.12f;

// //     CharacterController cc;
// //     Vector3 slideVelocity = Vector3.zero;
// //     Vector3 velocitySmoothRef = Vector3.zero;

// //     void Awake()
// //     {
// //         cc = GetComponent<CharacterController>();
// //     }

// //     void Update()
// //     {
// //         // ground check from the controller center downward
// //         Vector3 origin = transform.position + Vector3.up * 0.1f;
// //         RaycastHit hit;
// //         bool grounded = Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, groundLayer, QueryTriggerInteraction.Ignore);

// //         if (!grounded)
// //         {
// //             // not grounded -> let gravity or other systems handle movement (clear slide)
// //             slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
// //             // apply slideVelocity so it gradually stops
// //             cc.Move(slideVelocity * Time.deltaTime);
// //             return;
// //         }

// //         float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

// //         if (slopeAngle > minSlideAngle + 0.01f)
// //         {
// //             // direction along slope (downhill)
// //             Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
// //             float angleRad = slopeAngle * Mathf.Deg2Rad;
// //             float targetSpeed = Mathf.Clamp(Mathf.Sin(angleRad) * Physics.gravity.magnitude * slideGravityMultiplier, 0f, maxSlideSpeed);
// //             Vector3 targetVel = slideDir * targetSpeed;
// //             slideVelocity = Vector3.SmoothDamp(slideVelocity, targetVel, ref velocitySmoothRef, slideSmoothTime);
// //             // Move the controller
// //             cc.Move(slideVelocity * Time.deltaTime);
// //         }
// //         else
// //         {
// //             // tiny damping when slope is flat
// //             slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
// //             cc.Move(slideVelocity * Time.deltaTime);
// //         }
// //     }
// // }
// using UnityEngine;

// [RequireComponent(typeof(CharacterController))]
// public class CharacterControllerSlopeSlider_Debug : MonoBehaviour
// {
//     public LayerMask groundLayer = ~0;
//     public float sphereCastRadius = 0.2f;
//     public float sphereCastDistance = 0.9f;

//     [Header("Slope")]
//     public float minSlideAngle = 15f;
//     public float maxSlideSpeed = 6f;
//     public float slideGravityMultiplier = 1f;
//     public float slideSmoothTime = 0.12f;

//     CharacterController cc;
//     Vector3 slideVelocity = Vector3.zero;
//     Vector3 velocitySmoothRef = Vector3.zero;

//     // debug toggles
//     public bool enableDebugLogs = true;
//     public bool drawGizmos = true;

//     // store last hit info for gizmos
//     RaycastHit lastHit;
//     bool lastHitValid = false;

//     void Awake() => cc = GetComponent<CharacterController>();

//     void Update()
//     {
//         // origin at slightly above feet; adjust if your CC center is different
//         Vector3 origin = transform.position + Vector3.up * 0.1f;
//         RaycastHit hit;
//         bool grounded = Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, groundLayer, QueryTriggerInteraction.Ignore);

//         lastHit = hit;
//         lastHitValid = grounded;

//         if (!grounded)
//         {
//             if (enableDebugLogs) Debug.Log("[SlopeSlider] Not grounded");
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
//             cc.Move(slideVelocity * Time.deltaTime);
//             return;
//         }

//         float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

//         if (enableDebugLogs)
//         {
//             Debug.LogFormat("[SlopeSlider] Grounded. slopeAngle={0:0.0} deg, hitPoint={1}, normal={2}", slopeAngle, hit.point, hit.normal);
//         }

//         if (slopeAngle > minSlideAngle + 0.01f)
//         {
//             Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized; // downhill along plane
//             float angleRad = slopeAngle * Mathf.Deg2Rad;
//             float targetSpeed = Mathf.Clamp(Mathf.Sin(angleRad) * Physics.gravity.magnitude * slideGravityMultiplier, 0f, maxSlideSpeed);
//             Vector3 targetVel = slideDir * targetSpeed;
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, targetVel, ref velocitySmoothRef, slideSmoothTime);

//             if (enableDebugLogs) Debug.LogFormat("[SlopeSlider] Sliding. dir={0}, targetSpeed={1:0.00}", slideDir, targetSpeed);

//             cc.Move(slideVelocity * Time.deltaTime);
//         }
//         else
//         {
//             // damp out slide
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
//             cc.Move(slideVelocity * Time.deltaTime);
//         }
//     }

//     void OnDrawGizmos()
//     {
//         if (!drawGizmos) return;
//         if (!Application.isPlaying)
//         {
//             // draw sphereCast origin in editor
//             Gizmos.color = Color.yellow;
//             Vector3 origin = transform.position + Vector3.up * 0.1f;
//             Gizmos.DrawWireSphere(origin, sphereCastRadius);
//             Gizmos.DrawLine(origin, origin + Vector3.down * sphereCastDistance);
//             return;
//         }

//         if (lastHitValid)
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawSphere(lastHit.point, 0.02f);
//             Gizmos.color = Color.red;
//             Gizmos.DrawLine(lastHit.point, lastHit.point + lastHit.normal * 0.5f); // normal
//             // slide dir
//             Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, lastHit.normal).normalized;
//             Gizmos.color = Color.cyan;
//             Gizmos.DrawLine(lastHit.point + lastHit.normal * 0.02f, lastHit.point + lastHit.normal * 0.02f + slideDir * 0.6f);
//         }
//     }
// }
using UnityEngine;


public class CharacterControllerSlopeSlider : MonoBehaviour
{
    public LayerMask groundLayer = ~0;
    public float sphereCastRadius = 0.3f;
    public float sphereCastDistance = 1.2f;


    public float minSlideAngle = 15f;
    public float maxSlideSpeed = 6f;
    public float slideGravityMultiplier = 1f;
    public float slideSmoothTime = 0.12f;

 
    public bool applyPlatformVelocity = true;
    public float maxPlatformVelocity = 50f; // safety clamp (m/s)


    public bool enableVerboseLogs = false;
    public bool drawGizmos = true;

    CharacterController cc;
    Vector3 slideVelocity = Vector3.zero;
    Vector3 velocitySmoothRef = Vector3.zero;

    // platform tracking (corrected)
    Collider lastPlatform = null;
    Vector3 lastLocalHitPoint = Vector3.zero;     // hit point *in platform local space*
    Vector3 platformVelocity = Vector3.zero;

    RaycastHit lastHit;
    bool lastHitValid = false;

    void Awake() => cc = GetComponent<CharacterController>();

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // ground detection origin (bottom of controller)
        Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
        Vector3 origin = transform.position + bottomLocal + Vector3.up * 0.05f;

        lastHitValid = false;
        RaycastHit hit;
        bool usedSphereCast = false;

        if (Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            lastHitValid = true;
            usedSphereCast = true;
        }
        else if (Physics.Raycast(origin, Vector3.down, out hit, sphereCastDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            lastHitValid = true;
            usedSphereCast = false;
        }

        // ---------- Correct platform velocity calculation ----------
        platformVelocity = Vector3.zero;
        if (lastHitValid && applyPlatformVelocity)
        {
            Collider currentPlatform = hit.collider;
            if (currentPlatform != null)
            {
                // compute contact point in platform local space (so player movement is excluded)
                Vector3 localHitPoint = currentPlatform.transform.InverseTransformPoint(hit.point);

                if (lastPlatform == currentPlatform)
                {
                    // reconstruct previous world positions from stored local coords
                    Vector3 prevWorld = currentPlatform.transform.TransformPoint(lastLocalHitPoint);
                    Vector3 currWorld = currentPlatform.transform.TransformPoint(localHitPoint);
                    platformVelocity = (currWorld - prevWorld) / dt;

                    // safety clamp (avoid bizarre spikes)
                    if (platformVelocity.magnitude > maxPlatformVelocity)
                    {
                        if (enableVerboseLogs) Debug.LogWarningFormat("[PlatformFix] Clamped platformVel {0} to max {1}", platformVelocity.magnitude, maxPlatformVelocity);
                        platformVelocity = platformVelocity.normalized * maxPlatformVelocity;
                    }
                }
                else
                {
                    // different platform this frame - no previous point to compare, set zero
                    platformVelocity = Vector3.zero;
                }

                // store for next frame (store local)
                lastPlatform = currentPlatform;
                lastLocalHitPoint = localHitPoint;
            }
        }
        else
        {
            // no hit this frame -> clear platform tracking
            lastPlatform = null;
            lastLocalHitPoint = Vector3.zero;
            platformVelocity = Vector3.zero;
        }

        if (enableVerboseLogs)
        {
            if (!lastHitValid) Debug.Log("[PlatformFix] Not grounded");
            else Debug.LogFormat("[PlatformFix] Grounded on {0} (angle check next). platformVel={1}", hit.collider.name, platformVelocity);
        }

        // ------------- Apply platform velocity ----------------
        if (platformVelocity.sqrMagnitude > 0.000001f)
        {
            cc.Move(platformVelocity * dt);
        }

        // ---------- Sliding logic ----------
        if (!lastHitValid)
        {
            slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
            return;
        }

        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

        if (slopeAngle > minSlideAngle + 0.01f)
        {
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
            float angleRad = slopeAngle * Mathf.Deg2Rad;
            float targetSpeed = Mathf.Clamp(Mathf.Sin(angleRad) * Physics.gravity.magnitude * slideGravityMultiplier, 0f, maxSlideSpeed);
            Vector3 targetVel = slideDir * targetSpeed;
            slideVelocity = Vector3.SmoothDamp(slideVelocity, targetVel, ref velocitySmoothRef, slideSmoothTime);

            if (enableVerboseLogs) Debug.LogFormat("[PlatformFix] Sliding. dir={0}, targetSpeed={1:0.00}", slideDir, targetSpeed);

            cc.Move(slideVelocity * dt);
        }
        else
        {
            slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
            cc.Move(slideVelocity * dt);
        }

        lastHit = hit;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || cc == null) return;

        Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
        Vector3 origin = transform.position + bottomLocal + Vector3.up * 0.05f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, sphereCastRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * sphereCastDistance);

        if (Application.isPlaying && lastHitValid)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(lastHit.point, 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lastHit.point, lastHit.point + lastHit.normal * 0.5f);
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, lastHit.normal).normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(lastHit.point + lastHit.normal * 0.02f, lastHit.point + lastHit.normal * 0.02f + slideDir * 0.6f);

            if (platformVelocity.sqrMagnitude > 0.00001f)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(lastHit.point + Vector3.up * 0.05f, lastHit.point + Vector3.up * 0.05f + platformVelocity.normalized * 0.5f);
            }
        }
    }
}

