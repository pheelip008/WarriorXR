// using UnityEngine;


// public class CharacterControllerSlopeSlider_Pose : MonoBehaviour
// {
//     // [Header("Ground / Cast")]
//     public LayerMask groundLayer = ~0;
//     public float sphereCastRadius = 0.3f;
//     public float sphereCastDistance = 1.2f;

//     // [Header("Slope sliding")]
//     public float minSlideAngle = 15f;
//     public float maxSlideSpeed = 6f;
//     public float slideGravityMultiplier = 1f;
//     public float slideSmoothTime = 0.12f;

//     // [Header("Platform handling")]
//     public bool applyPlatformVelocity = true;
//     public float maxPlatformVelocity = 20f;
//     public float platformVelocitySmoothing = 10f;
//     public float localHitJumpTolerance = 0.2f;

//     // [Header("Pose (stop sliding) settings")]
//     // [Tooltip("Assign the head (Camera) transform from your XR rig.")]
//     public Transform headTransform;
//     // [Tooltip("Assign left hand/controller transform.")]
//     public Transform leftHandTransform;
//     // [Tooltip("Assign right hand/controller transform.")]
//     public Transform rightHandTransform;
//     //  [Tooltip("How long player must hold the pose to lock sliding (seconds).")]
//     public float poseHoldTime = 0.55f;
//     // [Tooltip("How much higher (meters) a hand must be above head to count as raised.")]
//     public float poseHeightAboveHead = 0.15f;
//     // [Tooltip("Maximum allowed distance between hands when posing (meters).")]
//     public float poseMaxHandSeparation = 0.45f;
//     // [Tooltip("Minimum downward velocity (m/s) of hands to consider stationary for pose (small value).")]
//     public float poseMaxHandVelocity = 0.3f;

//     // [Header("Debug")]
//     public bool enableVerboseLogs = false;
//     public bool drawGizmos = true;

//     // internals
//     CharacterController cc;
//     Vector3 slideVelocity = Vector3.zero;
//     Vector3 velocitySmoothRef = Vector3.zero;

//     // platform tracking (local coords)
//     Collider lastPlatform = null;
//     Vector3 lastLocalHitPoint = Vector3.zero;
//     Vector3 platformVelocity = Vector3.zero;
//     Vector3 lastPlatformVelocity = Vector3.zero;
//     Vector3 newPlatformVel = Vector3.zero;
//     bool hadValidHitLastFrame = false;

//     RaycastHit lastHit;
//     bool lastHitValid = false;

//     // pose state
//     bool poseLockRequested = false;   // external override via API
//     bool isPoseActive = false;        // true while holding pose (after hold time)
//     float poseTimer = 0f;
//     Vector3 lastLeftHandPos = Vector3.zero;
//     Vector3 lastRightHandPos = Vector3.zero;

//     void Awake()
//     {
//         cc = GetComponent<CharacterController>();
//     }

//     void Update()
//     {
//         float dt = Time.deltaTime;
//         if (dt <= 0f) return;

//         // ---- ground detection ----
//         Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
//         Vector3 bottomWorld = transform.position + bottomLocal;
//         Vector3 origin = bottomWorld + Vector3.up * 0.12f;

//         lastHitValid = false;
//         RaycastHit hit;
//         bool hitFound = Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, groundLayer, QueryTriggerInteraction.Ignore);

//         // overlap fallback for starting-inside or very close contacts
//         // ---- fallback: OverlapSphere to detect ground when spherecast fails ----
//     if (!hitFound)
//     {
//         Collider[] overlaps = Physics.OverlapSphere(
//             bottomWorld + Vector3.up * 0.01f,
//             Mathf.Max(0.01f, sphereCastRadius * 0.5f),
//             groundLayer,
//             QueryTriggerInteraction.Ignore
//         );

//         if (overlaps.Length > 0)
//         {
//         // Pick closest collider
//         Collider best = overlaps[0];
//         float bestDist = Vector3.Distance(bottomWorld, best.ClosestPoint(bottomWorld));

//         for (int i = 1; i < overlaps.Length; ++i)
//         {
//             float d = Vector3.Distance(bottomWorld, overlaps[i].ClosestPoint(bottomWorld));
//             if (d < bestDist)
//             {
//                 best = overlaps[i];
//                 bestDist = d;
//             }
//         }

//         // Try to get a proper hit using a tiny downward ray
//         Vector3 cp = best.ClosestPoint(bottomWorld + Vector3.up * 0.05f);
//         RaycastHit approxHit;

//         if (Physics.Raycast(cp + Vector3.up * 0.05f, Vector3.down,
//                             out approxHit, 0.12f, groundLayer,
//                             QueryTriggerInteraction.Ignore))
//         {
//             hit = approxHit; // <-- Safe! We assign the whole RaycastHit struct
//         }
//         else
//         {
//             // Construct a synthetic hit struct
//             hit = new RaycastHit
//             {
//                 point = cp,
//                 normal = (bottomWorld - cp).normalized,
//                 distance = Vector3.Distance(origin, cp),
//                 // collider cannot be assigned here (readonly)
//                 // but we can store the collider separately
//             };
//         }

//         lastHit = hit;
//         lastHitValid = true;
//         hitFound = true;
//         }
//     }


//         lastHitValid = hitFound;

//         // ---- compute platform velocity robustly (local-space delta) ----
//         newPlatformVel = Vector3.zero;
//         if (lastHitValid && applyPlatformVelocity)
//         {
//             Collider currentPlatform = hit.collider;
//             if (currentPlatform != null)
//             {
//                 Vector3 localHit = currentPlatform.transform.InverseTransformPoint(hit.point);

//                 if (lastPlatform == currentPlatform && hadValidHitLastFrame)
//                 {
//                     float localJump = Vector3.Distance(localHit, lastLocalHitPoint);
//                     if (localJump > localHitJumpTolerance)
//                     {
//                         if (enableVerboseLogs) Debug.LogFormat("[PoseSlider] Local hit jumped {0:F3} > {1:F3} — ignore platform vel this frame", localJump, localHitJumpTolerance);
//                         newPlatformVel = Vector3.zero;
//                     }
//                     else
//                     {
//                         Vector3 prevWorld = currentPlatform.transform.TransformPoint(lastLocalHitPoint);
//                         Vector3 currWorld = currentPlatform.transform.TransformPoint(localHit);
//                         newPlatformVel = (currWorld - prevWorld) / dt;
//                     }
//                 }
//                 else
//                 {
//                     newPlatformVel = Vector3.zero;
//                 }

//                 lastPlatform = currentPlatform;
//                 lastLocalHitPoint = localHit;
//             }
//         }
//         else
//         {
//             lastPlatform = null;
//             lastLocalHitPoint = Vector3.zero;
//             newPlatformVel = Vector3.zero;
//         }

//         // clamp & smooth platform velocity
//         if (newPlatformVel.magnitude > maxPlatformVelocity) newPlatformVel = newPlatformVel.normalized * maxPlatformVelocity;
//         platformVelocity = Vector3.Lerp(platformVelocity, newPlatformVel, Mathf.Clamp01(platformVelocitySmoothing * dt));
//         if (platformVelocity.magnitude < 0.001f) platformVelocity = Vector3.zero;

//         if (enableVerboseLogs)
//         {
//             if (!lastHitValid) Debug.Log("[PoseSlider] Not grounded");
//             else Debug.LogFormat("[PoseSlider] Grounded on {0}. platformVel={1}", hit.collider ? hit.collider.name : "null", platformVelocity);
//         }

//         // ---- pose detection (based on hand transforms relative to head) ----
//         bool poseDetected = false;
//         if (headTransform != null && leftHandTransform != null && rightHandTransform != null)
//         {
//             // simple "both hands raised above head and not far apart" check
//             float headY = headTransform.position.y;
//             float leftY = leftHandTransform.position.y;
//             float rightY = rightHandTransform.position.y;

//             float avgHandHeightAboveHead = ((leftY - headY) + (rightY - headY)) * 0.5f;
//             float handsSeparation = Vector3.Distance(leftHandTransform.position, rightHandTransform.position);

//             // check hand speeds (optional: ensure hands are relatively stationary)
//             Vector3 leftVel = (leftHandTransform.position - lastLeftHandPos) / Mathf.Max(0.0001f, dt);
//             Vector3 rightVel = (rightHandTransform.position - lastRightHandPos) / Mathf.Max(0.0001f, dt);

//             bool handsStationary = leftVel.magnitude < poseMaxHandVelocity && rightVel.magnitude < poseMaxHandVelocity;

//             if (avgHandHeightAboveHead > poseHeightAboveHead && handsSeparation <= poseMaxHandSeparation && handsStationary)
//             {
//                 poseDetected = true;
//             }

//             lastLeftHandPos = leftHandTransform.position;
//             lastRightHandPos = rightHandTransform.position;
//         }
//         else
//         {
//             // Not enough info to auto-detect pose — pose detection disabled; rely on API call
//             poseDetected = false;
//         }

//         // handle hold-timer & override
//         if (poseDetected && !poseLockRequested)
//         {
//             poseTimer += dt;
//             if (poseTimer >= poseHoldTime)
//             {
//                 if (!isPoseActive)
//                 {
//                     isPoseActive = true;
//                     if (enableVerboseLogs) Debug.Log("[PoseSlider] Pose engaged - sliding locked");
//                 }
//             }
//         }
//         else
//         {
//             poseTimer = 0f;
//             if (isPoseActive && !poseLockRequested)
//             {
//                 isPoseActive = false;
//                 if (enableVerboseLogs) Debug.Log("[PoseSlider] Pose released - sliding unlocked");
//             }
//         }

//         // If external override requested, respect that (poseLockRequested forces lock)
//         bool slidingLocked = poseLockRequested || isPoseActive;

//         // ---- apply platform velocity first so controller rides the moving ground ----
//         if (!slidingLocked && platformVelocity.sqrMagnitude > 0.000001f)
//         {
//             cc.Move(platformVelocity * dt);
//         }
//         else if (slidingLocked && platformVelocity.sqrMagnitude > 0.000001f)
//         {
//             // If sliding is locked, we still want player to follow platform (so they remain on the moving surface)
//             cc.Move(platformVelocity * dt);
//         }

//         // ---- sliding logic (only when not locked by pose) ----
//         if (!lastHitValid)
//         {
//             // in air -> smooth out slide velocity
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
//             hadValidHitLastFrame = lastHitValid;
//             return;
//         }

//         float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

//         if (!slidingLocked && slopeAngle > minSlideAngle + 0.01f)
//         {
//             Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
//             float angleRad = slopeAngle * Mathf.Deg2Rad;
//             float targetSpeed = Mathf.Clamp(Mathf.Sin(angleRad) * Physics.gravity.magnitude * slideGravityMultiplier, 0f, maxSlideSpeed);
//             Vector3 targetVel = slideDir * targetSpeed;
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, targetVel, ref velocitySmoothRef, slideSmoothTime);
//             cc.Move(slideVelocity * dt);
//             if (enableVerboseLogs) Debug.LogFormat("[PoseSlider] Sliding. dir={0}, targetSpeed={1:0.00}", slideDir, targetSpeed);
//         }
//         else
//         {
//             // either locked by pose OR slope too small: damp sliding to zero, but still follow platform
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
//             cc.Move(slideVelocity * dt);
//         }

//         lastHit = hit;
//         hadValidHitLastFrame = lastHitValid;
//     }

//     // ----------------- Public API -----------------
//     /// <summary>Externally request lock/unlock of sliding (true = lock/stop sliding, false = release).</summary>
//     public void SetPoseLock(bool lockSliding)
//     {
//         poseLockRequested = lockSliding;
//         if (lockSliding)
//         {
//             isPoseActive = false;
//             poseTimer = 0f;
//             if (enableVerboseLogs) Debug.Log("[PoseSlider] External pose lock applied");
//         }
//         else
//         {
//             if (enableVerboseLogs) Debug.Log("[PoseSlider] External pose lock released");
//         }
//     }

//     /// <summary>Return whether sliding is currently blocked (pose or api lock)</summary>
//     public bool IsSlidingLocked()
//     {
//         return poseLockRequested || isPoseActive;
//     }

//     // ----------------- Gizmos -----------------
//     void OnDrawGizmos()
//     {
//         if (!drawGizmos || cc == null) return;
//         Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
//         Vector3 bottomWorld = transform.position + bottomLocal;
//         Vector3 origin = bottomWorld + Vector3.up * 0.12f;

//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(origin, sphereCastRadius);
//         Gizmos.DrawLine(origin, origin + Vector3.down * sphereCastDistance);

//         if (Application.isPlaying && lastHitValid)
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawSphere(lastHit.point, 0.02f);
//             Gizmos.color = Color.red;
//             Gizmos.DrawLine(lastHit.point, lastHit.point + lastHit.normal * 0.5f);
//             Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, lastHit.normal).normalized;
//             Gizmos.color = Color.cyan;
//             Gizmos.DrawLine(lastHit.point + lastHit.normal * 0.02f, lastHit.point + lastHit.normal * 0.02f + slideDir * 0.6f);

//             if (platformVelocity.sqrMagnitude > 0.00001f)
//             {
//                 Gizmos.color = Color.magenta;
//                 Gizmos.DrawLine(lastHit.point + Vector3.up * 0.05f, lastHit.point + Vector3.up * 0.05f + platformVelocity.normalized * 0.5f);
//             }
//         }

//         // draw pose visualization
//         if (headTransform != null && leftHandTransform != null && rightHandTransform != null)
//         {
//             Gizmos.color = isPoseActive ? Color.green : Color.yellow;
//             Gizmos.DrawSphere(headTransform.position, 0.03f);
//             Gizmos.DrawSphere(leftHandTransform.position, 0.02f);
//             Gizmos.DrawSphere(rightHandTransform.position, 0.02f);
//             Gizmos.DrawLine(leftHandTransform.position, rightHandTransform.position);
//         }
//     }
// }
// using UnityEngine;

// [RequireComponent(typeof(CharacterController))]
// public class CharacterControllerSlopeSlider_Pose : MonoBehaviour
// {
//     [Header("Ground / Cast")]
//     public LayerMask groundLayer = ~0;
//     public float sphereCastRadius = 0.3f;
//     public float sphereCastDistance = 1.2f;

//     [Header("Slope sliding")]
//     public float minSlideAngle = 15f;
//     public float maxSlideSpeed = 6f;
//     public float slideGravityMultiplier = 1f;
//     public float slideSmoothTime = 0.12f;

//     [Header("Platform handling")]
//     public bool applyPlatformVelocity = true;
//     public float maxPlatformVelocity = 20f;
//     public float platformVelocitySmoothing = 10f;
//     public float localHitJumpTolerance = 0.2f; // if local hit jumps more than this, ignore platform velocity

//     [Header("Pose (stop sliding) settings")]
//     [Tooltip("Assign the head (Camera) transform from your XR rig.")]
//     public Transform headTransform;
//     [Tooltip("Assign left hand/controller transform.")]
//     public Transform leftHandTransform;
//     [Tooltip("Assign right hand/controller transform.")]
//     public Transform rightHandTransform;
//     [Tooltip("How long player must hold the pose to lock sliding (seconds).")]
//     public float poseHoldTime = 0.55f;
//     [Tooltip("How much higher (meters) a hand must be above head to count as raised.")]
//     public float poseHeightAboveHead = 0.15f;
//     [Tooltip("Maximum allowed distance between hands when posing (meters).")]
//     public float poseMaxHandSeparation = 0.45f;
//     [Tooltip("Maximum allowed hand velocity while holding pose (m/s).")]
//     public float poseMaxHandVelocity = 0.3f;

//     [Header("Debug")]
//     public bool enableVerboseLogs = false;
//     public bool drawGizmos = true;

//     // internals
//     CharacterController cc;
//     Vector3 slideVelocity = Vector3.zero;
//     Vector3 velocitySmoothRef = Vector3.zero;

//     // platform tracking
//     Collider lastPlatform = null;
//     Vector3 lastLocalHitPoint = Vector3.zero;     // hit point in platform local space
//     Vector3 platformVelocity = Vector3.zero;
//     Vector3 newPlatformVel = Vector3.zero;
//     bool hadValidHitLastFrame = false;
//     Collider fallbackCollider = null; // used when hit.collider is null (overlap fallback)

//     RaycastHit lastHit;
//     bool lastHitValid = false;

//     // pose state
//     bool poseLockRequested = false;   // external override via API
//     bool isPoseActive = false;        // true while holding pose (after hold time)
//     float poseTimer = 0f;
//     Vector3 lastLeftHandPos = Vector3.zero;
//     Vector3 lastRightHandPos = Vector3.zero;

//     void Awake()
//     {
//         cc = GetComponent<CharacterController>();
//     }

//     void Update()
//     {
//         float dt = Time.deltaTime;
//         if (dt <= 0f) return;

//         // ---- ground detection ----
//         Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
//         Vector3 bottomWorld = transform.position + bottomLocal;
//         Vector3 origin = bottomWorld + Vector3.up * 0.12f;

//         lastHitValid = false;
//         RaycastHit hit;
//         bool hitFound = Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, groundLayer, QueryTriggerInteraction.Ignore);

//         // reset fallback collider each frame (will be assigned if fallback happens)
//         fallbackCollider = null;

//         // overlap fallback for when spherecast misses (starting inside or very near)
//         if (!hitFound)
//         {
//             Collider[] overlaps = Physics.OverlapSphere(bottomWorld + Vector3.up * 0.01f, Mathf.Max(0.01f, sphereCastRadius * 0.5f), groundLayer, QueryTriggerInteraction.Ignore);
//             if (overlaps.Length > 0)
//             {
//                 // choose closest overlap
//                 Collider best = overlaps[0];
//                 float bestDist = Vector3.Distance(bottomWorld, best.ClosestPoint(bottomWorld));
//                 for (int i = 1; i < overlaps.Length; ++i)
//                 {
//                     float d = Vector3.Distance(bottomWorld, overlaps[i].ClosestPoint(bottomWorld));
//                     if (d < bestDist) { best = overlaps[i]; bestDist = d; }
//                 }

//                 // attempt a small raycast from just above the closest point to get a reliable RaycastHit (with collider)
//                 Vector3 cp = best.ClosestPoint(bottomWorld + Vector3.up * 0.05f);
//                 RaycastHit approxHit;
//                 if (Physics.Raycast(cp + Vector3.up * 0.05f, Vector3.down, out approxHit, 0.12f, groundLayer, QueryTriggerInteraction.Ignore))
//                 {
//                     hit = approxHit;
//                     fallbackCollider = null;
//                 }
//                 else
//                 {
//                     // synthesize hit (can't set hit.collider), but remember the actual collider separately
//                     Vector3 synthesizedPoint = cp;
//                     Vector3 synthesizedNormal = (bottomWorld - cp).normalized;
//                     if (synthesizedNormal.sqrMagnitude < 0.0001f) synthesizedNormal = Vector3.up;
//                     hit = new RaycastHit
//                     {
//                         point = synthesizedPoint,
//                         normal = synthesizedNormal,
//                         distance = Vector3.Distance(origin, synthesizedPoint)
//                     };
//                     fallbackCollider = best;
//                 }

//                 lastHit = hit;
//                 lastHitValid = true;
//                 hitFound = true;
//             }
//         }
//         else
//         {
//             lastHit = hit;
//             lastHitValid = true;
//         }

//         // ---- compute platform velocity robustly (platform-local difference) ----
//         newPlatformVel = Vector3.zero;
//         if (lastHitValid && applyPlatformVelocity)
//         {
//             // determine current platform collider robustly (use fallback if hit.collider is null)
//             Collider currentPlatform = lastHit.collider != null ? lastHit.collider : fallbackCollider;

//             if (currentPlatform != null)
//             {
//                 // compute hit point in platform local space (works even if hit was synthesized)
//                 Vector3 localHit = currentPlatform.transform.InverseTransformPoint(lastHit.point);

//                 if (lastPlatform == currentPlatform && hadValidHitLastFrame)
//                 {
//                     // if the local point jumped too much, ignore platform velocity this frame
//                     float localJump = Vector3.Distance(localHit, lastLocalHitPoint);
//                     if (localJump > localHitJumpTolerance)
//                     {
//                         if (enableVerboseLogs) Debug.LogFormat("[PoseSlider] Local hit jumped {0:F3} > {1:F3} — ignoring platform velocity this frame", localJump, localHitJumpTolerance);
//                         newPlatformVel = Vector3.zero;
//                     }
//                     else
//                     {
//                         Vector3 prevWorld = currentPlatform.transform.TransformPoint(lastLocalHitPoint);
//                         Vector3 currWorld = currentPlatform.transform.TransformPoint(localHit);
//                         newPlatformVel = (currWorld - prevWorld) / Mathf.Max(0.000001f, dt);
//                     }
//                 }
//                 else
//                 {
//                     newPlatformVel = Vector3.zero;
//                 }

//                 // store
//                 lastPlatform = currentPlatform;
//                 lastLocalHitPoint = localHit;
//             }
//             else
//             {
//                 // no platform found (shouldn't happen here) -> clear
//                 newPlatformVel = Vector3.zero;
//                 lastPlatform = null;
//                 lastLocalHitPoint = Vector3.zero;
//             }
//         }
//         else
//         {
//             // not grounded or platform velocity disabled
//             newPlatformVel = Vector3.zero;
//             lastPlatform = null;
//             lastLocalHitPoint = Vector3.zero;
//         }

//         // clamp & smooth platform velocity
//         if (newPlatformVel.magnitude > maxPlatformVelocity)
//         {
//             if (enableVerboseLogs) Debug.LogWarningFormat("[PoseSlider] Clamping platformVel {0:F2} to max {1}", newPlatformVel.magnitude, maxPlatformVelocity);
//             newPlatformVel = newPlatformVel.normalized * maxPlatformVelocity;
//         }
//         platformVelocity = Vector3.Lerp(platformVelocity, newPlatformVel, Mathf.Clamp01(platformVelocitySmoothing * dt));
//         if (platformVelocity.magnitude < 0.001f) platformVelocity = Vector3.zero;

//         if (enableVerboseLogs)
//         {
//             Collider debugPlatform = lastHit.collider != null ? lastHit.collider : fallbackCollider;
//             Debug.LogFormat("[PoseSlider] {0} on {1}. platformVel={2}",
//                 lastHitValid ? "Grounded" : "Not grounded",
//                 debugPlatform ? debugPlatform.name : "null",
//                 platformVelocity.ToString("F2"));
//         }

//         // ---- pose detection (hands over head & stationary) ----
//         bool poseDetected = false;
//         if (headTransform != null && leftHandTransform != null && rightHandTransform != null)
//         {
//             float headY = headTransform.position.y;
//             float leftY = leftHandTransform.position.y;
//             float rightY = rightHandTransform.position.y;
//             float avgHandHeightAboveHead = ((leftY - headY) + (rightY - headY)) * 0.5f;
//             float handsSeparation = Vector3.Distance(leftHandTransform.position, rightHandTransform.position);

//             Vector3 leftVel = (leftHandTransform.position - lastLeftHandPos) / Mathf.Max(0.0001f, dt);
//             Vector3 rightVel = (rightHandTransform.position - lastRightHandPos) / Mathf.Max(0.0001f, dt);
//             bool handsStationary = leftVel.magnitude < poseMaxHandVelocity && rightVel.magnitude < poseMaxHandVelocity;

//             if (avgHandHeightAboveHead > poseHeightAboveHead && handsSeparation <= poseMaxHandSeparation && handsStationary)
//             {
//                 poseDetected = true;
//             }

//             lastLeftHandPos = leftHandTransform.position;
//             lastRightHandPos = rightHandTransform.position;
//         }

//         // handle hold-timer & override
//         if (poseDetected && !poseLockRequested)
//         {
//             poseTimer += dt;
//             if (poseTimer >= poseHoldTime)
//             {
//                 if (!isPoseActive)
//                 {
//                     isPoseActive = true;
//                     if (enableVerboseLogs) Debug.Log("[PoseSlider] Pose engaged - sliding locked");
//                 }
//             }
//         }
//         else
//         {
//             poseTimer = 0f;
//             if (isPoseActive && !poseLockRequested)
//             {
//                 isPoseActive = false;
//                 if (enableVerboseLogs) Debug.Log("[PoseSlider] Pose released - sliding unlocked");
//             }
//         }

//         bool slidingLocked = poseLockRequested || isPoseActive;

//         // ---- apply platform velocity so controller follows moving ground ----
//         if (platformVelocity.sqrMagnitude > 0.000001f)
//         {
//             cc.Move(platformVelocity * dt);
//         }

//         // ---- sliding logic (only when not locked) ----
//         if (!lastHitValid)
//         {
//             // in air: damp slide velocity
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
//             hadValidHitLastFrame = lastHitValid;
//             return;
//         }

//         // use a safe normal
//         Vector3 hitNormal = lastHit.normal;
//         if (hitNormal.sqrMagnitude < 0.000001f) hitNormal = Vector3.up;

//         float slopeAngle = Vector3.Angle(hitNormal, Vector3.up);

//         // determine current platform for logs / possible use
//         Collider currentPlatformCollider = lastHit.collider != null ? lastHit.collider : fallbackCollider;

//         if (enableVerboseLogs)
//         {
//             Debug.LogFormat("[PoseSlider] Grounded on {0}. slopeAngle={1:0.00} deg, locked={2}", currentPlatformCollider ? currentPlatformCollider.name : "null", slopeAngle, slidingLocked);
//         }

//         if (!slidingLocked && slopeAngle > minSlideAngle + 0.01f)
//         {
//             Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hitNormal).normalized;
//             if (slideDir.sqrMagnitude < 0.000001f) slideDir = Vector3.zero;

//             float angleRad = slopeAngle * Mathf.Deg2Rad;
//             float targetSpeed = Mathf.Clamp(Mathf.Sin(angleRad) * Physics.gravity.magnitude * slideGravityMultiplier, 0f, maxSlideSpeed);
//             Vector3 targetVel = slideDir * targetSpeed;
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, targetVel, ref velocitySmoothRef, slideSmoothTime);

//             cc.Move(slideVelocity * dt);

//             if (enableVerboseLogs) Debug.LogFormat("[PoseSlider] Sliding. dir={0}, targetSpeed={1:0.00}", slideDir.ToString("F2"), targetSpeed);
//         }
//         else
//         {
//             // locked or no slope: damp sliding to zero, but still ensure we follow platform (done above)
//             slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
//             cc.Move(slideVelocity * dt);
//         }

//         lastHit = lastHit; // keep lastHit for gizmos
//         hadValidHitLastFrame = lastHitValid;
//     }

//     // ----------------- Public API -----------------
//     /// <summary>Externally request lock/unlock of sliding (true = lock/stop sliding, false = release).</summary>
//     public void SetPoseLock(bool lockSliding)
//     {
//         poseLockRequested = lockSliding;
//         if (lockSliding)
//         {
//             isPoseActive = false;
//             poseTimer = 0f;
//             if (enableVerboseLogs) Debug.Log("[PoseSlider] External pose lock applied");
//         }
//         else
//         {
//             if (enableVerboseLogs) Debug.Log("[PoseSlider] External pose lock released");
//         }
//     }

//     /// <summary>Return whether sliding is currently blocked (pose or api lock)</summary>
//     public bool IsSlidingLocked()
//     {
//         return poseLockRequested || isPoseActive;
//     }

//     void OnDrawGizmos()
//     {
//         if (!drawGizmos || cc == null) return;

//         Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
//         Vector3 bottomWorld = transform.position + bottomLocal;
//         Vector3 origin = bottomWorld + Vector3.up * 0.12f;

//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(origin, sphereCastRadius);
//         Gizmos.DrawLine(origin, origin + Vector3.down * sphereCastDistance);

//         if (Application.isPlaying && lastHitValid)
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawSphere(lastHit.point, 0.02f);
//             Gizmos.color = Color.red;
//             Gizmos.DrawLine(lastHit.point, lastHit.point + lastHit.normal * 0.5f);
//             Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, lastHit.normal).normalized;
//             Gizmos.color = Color.cyan;
//             Gizmos.DrawLine(lastHit.point + lastHit.normal * 0.02f, lastHit.point + lastHit.normal * 0.02f + slideDir * 0.6f);

//             if (platformVelocity.sqrMagnitude > 0.00001f)
//             {
//                 Gizmos.color = Color.magenta;
//                 Gizmos.DrawLine(lastHit.point + Vector3.up * 0.05f, lastHit.point + Vector3.up * 0.05f + platformVelocity.normalized * 0.5f);
//             }
//         }

//         // draw pose visualization
//         if (headTransform != null && leftHandTransform != null && rightHandTransform != null)
//         {
//             Gizmos.color = isPoseActive ? Color.green : Color.yellow;
//             Gizmos.DrawSphere(headTransform.position, 0.03f);
//             Gizmos.DrawSphere(leftHandTransform.position, 0.02f);
//             Gizmos.DrawSphere(rightHandTransform.position, 0.02f);
//             Gizmos.DrawLine(leftHandTransform.position, rightHandTransform.position);
//         }
//     }
// }
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerSlopeSlider_Pose : MonoBehaviour
{
    [Header("Ground / Cast")]
    public LayerMask groundLayer = ~0;
    public float sphereCastRadius = 0.3f;
    public float sphereCastDistance = 1.2f;

    [Header("Slope sliding")]
    public float minSlideAngle = 15f;
    public float maxSlideSpeed = 6f;
    public float slideGravityMultiplier = 1f;
    public float slideSmoothTime = 0.12f;

    [Header("Platform handling")]
    public bool applyPlatformVelocity = true;
    public float maxPlatformVelocity = 20f;
    public float platformVelocitySmoothing = 10f;
    public float localHitJumpTolerance = 0.2f; // if local hit jumps more than this, ignore platform velocity

    [Header("Pose (stop sliding) settings")]
    [Tooltip("Assign the head (Camera) transform from your XR rig.")]
    public Transform headTransform;
    [Tooltip("Assign left hand/controller transform.")]
    public Transform leftHandTransform;
    [Tooltip("Assign right hand/controller transform.")]
    public Transform rightHandTransform;
    [Tooltip("How long player must hold the pose to lock sliding (seconds).")]
    public float poseHoldTime = 0.55f;
    [Tooltip("How much higher (meters) a hand must be above head to count as raised.")]
    public float poseHeightAboveHead = 0.15f;
    [Tooltip("Maximum allowed distance between hands when posing (meters).")]
    public float poseMaxHandSeparation = 0.45f;
    [Tooltip("Maximum allowed hand velocity while holding pose (m/s).")]
    public float poseMaxHandVelocity = 0.3f;

    [Header("Debug")]
    public bool enableVerboseLogs = false;
    public bool drawGizmos = true;

    // internals
    CharacterController cc;
    Vector3 slideVelocity = Vector3.zero;
    Vector3 velocitySmoothRef = Vector3.zero;

    // platform tracking
    Collider lastPlatform = null;
    Vector3 lastLocalHitPoint = Vector3.zero;     // hit point in platform local space
    Vector3 platformVelocity = Vector3.zero;
    Vector3 newPlatformVel = Vector3.zero;
    bool hadValidHitLastFrame = false;
    Collider fallbackCollider = null; // used when hit.collider is null (overlap fallback)

    RaycastHit lastHit;
    bool lastHitValid = false;

    // pose state
    bool poseLockRequested = false;   // external override via API
    bool isPoseActive = false;        // true while holding pose (after hold time)
    float poseTimer = 0f;
    Vector3 lastLeftHandPos = Vector3.zero;
    Vector3 lastRightHandPos = Vector3.zero;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // ---- ground detection ----
        Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
        Vector3 bottomWorld = transform.position + bottomLocal;
        Vector3 origin = bottomWorld + Vector3.up * 0.12f;

        lastHitValid = false;
        RaycastHit hit;
        bool hitFound = Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, groundLayer, QueryTriggerInteraction.Ignore);

        // reset fallback collider each frame (will be assigned if fallback happens)
        fallbackCollider = null;

        // overlap fallback for when spherecast misses (starting inside or very near)
        if (!hitFound)
        {
            Collider[] overlaps = Physics.OverlapSphere(bottomWorld + Vector3.up * 0.01f, Mathf.Max(0.01f, sphereCastRadius * 0.5f), groundLayer, QueryTriggerInteraction.Ignore);
            if (overlaps.Length > 0)
            {
                // choose closest overlap
                Collider best = overlaps[0];
                float bestDist = Vector3.Distance(bottomWorld, best.ClosestPoint(bottomWorld));
                for (int i = 1; i < overlaps.Length; ++i)
                {
                    float d = Vector3.Distance(bottomWorld, overlaps[i].ClosestPoint(bottomWorld));
                    if (d < bestDist) { best = overlaps[i]; bestDist = d; }
                }

                // attempt a small raycast from just above the closest point to get a reliable RaycastHit (with collider)
                Vector3 cp = best.ClosestPoint(bottomWorld + Vector3.up * 0.05f);
                RaycastHit approxHit;
                if (Physics.Raycast(cp + Vector3.up * 0.05f, Vector3.down, out approxHit, 0.12f, groundLayer, QueryTriggerInteraction.Ignore))
                {
                    hit = approxHit;
                    fallbackCollider = null;
                }
                else
                {
                    // synthesize hit (can't set hit.collider), but remember the actual collider separately
                    Vector3 synthesizedPoint = cp;
                    Vector3 synthesizedNormal = (bottomWorld - cp).normalized;
                    if (synthesizedNormal.sqrMagnitude < 0.0001f) synthesizedNormal = Vector3.up;
                    hit = new RaycastHit
                    {
                        point = synthesizedPoint,
                        normal = synthesizedNormal,
                        distance = Vector3.Distance(origin, synthesizedPoint)
                    };
                    fallbackCollider = best;
                }

                lastHit = hit;
                lastHitValid = true;
                hitFound = true;
            }
        }
        else
        {
            lastHit = hit;
            lastHitValid = true;
        }

        // ---- compute platform velocity robustly (platform-local difference) ----
        newPlatformVel = Vector3.zero;
        if (lastHitValid && applyPlatformVelocity)
        {
            // determine current platform collider robustly (use fallback if hit.collider is null)
            Collider currentPlatform = lastHit.collider != null ? lastHit.collider : fallbackCollider;

            if (currentPlatform != null)
            {
                // compute hit point in platform local space (works even if hit was synthesized)
                Vector3 localHit = currentPlatform.transform.InverseTransformPoint(lastHit.point);

                if (lastPlatform == currentPlatform && hadValidHitLastFrame)
                {
                    // if the local point jumped too much, ignore platform velocity this frame
                    float localJump = Vector3.Distance(localHit, lastLocalHitPoint);
                    if (localJump > localHitJumpTolerance)
                    {
                        if (enableVerboseLogs) Debug.LogFormat("[PoseSlider] Local hit jumped {0:F3} > {1:F3} — ignoring platform velocity this frame", localJump, localHitJumpTolerance);
                        newPlatformVel = Vector3.zero;
                    }
                    else
                    {
                        Vector3 prevWorld = currentPlatform.transform.TransformPoint(lastLocalHitPoint);
                        Vector3 currWorld = currentPlatform.transform.TransformPoint(localHit);
                        newPlatformVel = (currWorld - prevWorld) / Mathf.Max(0.000001f, dt);
                    }
                }
                else
                {
                    newPlatformVel = Vector3.zero;
                }

                // store
                lastPlatform = currentPlatform;
                lastLocalHitPoint = localHit;
            }
            else
            {
                // no platform found (shouldn't happen here) -> clear
                newPlatformVel = Vector3.zero;
                lastPlatform = null;
                lastLocalHitPoint = Vector3.zero;
            }
        }
        else
        {
            // not grounded or platform velocity disabled
            newPlatformVel = Vector3.zero;
            lastPlatform = null;
            lastLocalHitPoint = Vector3.zero;
        }

        // clamp & smooth platform velocity
        if (newPlatformVel.magnitude > maxPlatformVelocity)
        {
            if (enableVerboseLogs) Debug.LogWarningFormat("[PoseSlider] Clamping platformVel {0:F2} to max {1}", newPlatformVel.magnitude, maxPlatformVelocity);
            newPlatformVel = newPlatformVel.normalized * maxPlatformVelocity;
        }
        platformVelocity = Vector3.Lerp(platformVelocity, newPlatformVel, Mathf.Clamp01(platformVelocitySmoothing * dt));
        if (platformVelocity.magnitude < 0.001f) platformVelocity = Vector3.zero;

        if (enableVerboseLogs)
        {
            Collider debugPlatform = lastHit.collider != null ? lastHit.collider : fallbackCollider;
            Debug.LogFormat("[PoseSlider] {0} on {1}. platformVel={2}",
                lastHitValid ? "Grounded" : "Not grounded",
                debugPlatform ? debugPlatform.name : "null",
                platformVelocity.ToString("F2"));
        }

        // ---- pose detection (hands over head & stationary) ----
        bool poseDetected = false;
        if (headTransform != null && leftHandTransform != null && rightHandTransform != null)
        {
            float headY = headTransform.position.y;
            float leftY = leftHandTransform.position.y;
            float rightY = rightHandTransform.position.y;
            float avgHandHeightAboveHead = ((leftY - headY) + (rightY - headY)) * 0.5f;
            float handsSeparation = Vector3.Distance(leftHandTransform.position, rightHandTransform.position);

            Vector3 leftVel = (leftHandTransform.position - lastLeftHandPos) / Mathf.Max(0.0001f, dt);
            Vector3 rightVel = (rightHandTransform.position - lastRightHandPos) / Mathf.Max(0.0001f, dt);
            bool handsStationary = leftVel.magnitude < poseMaxHandVelocity && rightVel.magnitude < poseMaxHandVelocity;

            if (avgHandHeightAboveHead > poseHeightAboveHead && handsSeparation <= poseMaxHandSeparation && handsStationary)
            {
                poseDetected = true;
            }

            lastLeftHandPos = leftHandTransform.position;
            lastRightHandPos = rightHandTransform.position;
        }

        // handle hold-timer & override
        if (poseDetected && !poseLockRequested)
        {
            poseTimer += dt;
            if (poseTimer >= poseHoldTime)
            {
                if (!isPoseActive)
                {
                    isPoseActive = true;
                    if (enableVerboseLogs) Debug.Log("[PoseSlider] Pose engaged - sliding locked");
                }
            }
        }
        else
        {
            poseTimer = 0f;
            if (isPoseActive && !poseLockRequested)
            {
                isPoseActive = false;
                if (enableVerboseLogs) Debug.Log("[PoseSlider] Pose released - sliding unlocked");
            }
        }

        bool slidingLocked = poseLockRequested || isPoseActive;

        // ---- apply platform velocity so controller follows moving ground ----
        if (platformVelocity.sqrMagnitude > 0.000001f)
        {
            cc.Move(platformVelocity * dt);
        }

        // ---- sliding logic (only when not locked) ----
        if (!lastHitValid)
        {
            // in air: damp slide velocity
            slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
            hadValidHitLastFrame = lastHitValid;
            return;
        }

        // use a safe normal
        Vector3 hitNormal = lastHit.normal;
        if (hitNormal.sqrMagnitude < 0.000001f) hitNormal = Vector3.up;

        float slopeAngle = Vector3.Angle(hitNormal, Vector3.up);

        // determine current platform for logs / possible use
        Collider currentPlatformCollider = lastHit.collider != null ? lastHit.collider : fallbackCollider;

        if (enableVerboseLogs)
        {
            Debug.LogFormat("[PoseSlider] Grounded on {0}. slopeAngle={1:0.00} deg, locked={2}", currentPlatformCollider ? currentPlatformCollider.name : "null", slopeAngle, slidingLocked);
        }

        if (!slidingLocked && slopeAngle > minSlideAngle + 0.01f)
        {
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hitNormal).normalized;
            if (slideDir.sqrMagnitude < 0.000001f) slideDir = Vector3.zero;

            float angleRad = slopeAngle * Mathf.Deg2Rad;
            float targetSpeed = Mathf.Clamp(Mathf.Sin(angleRad) * Physics.gravity.magnitude * slideGravityMultiplier, 0f, maxSlideSpeed);
            Vector3 targetVel = slideDir * targetSpeed;
            slideVelocity = Vector3.SmoothDamp(slideVelocity, targetVel, ref velocitySmoothRef, slideSmoothTime);

            cc.Move(slideVelocity * dt);

            if (enableVerboseLogs) Debug.LogFormat("[PoseSlider] Sliding. dir={0}, targetSpeed={1:0.00}", slideDir.ToString("F2"), targetSpeed);
        }
        else
        {
            // locked or no slope: damp sliding to zero, but still ensure we follow platform (done above)
            slideVelocity = Vector3.SmoothDamp(slideVelocity, Vector3.zero, ref velocitySmoothRef, slideSmoothTime);
            cc.Move(slideVelocity * dt);
        }

        lastHit = lastHit; // keep lastHit for gizmos
        hadValidHitLastFrame = lastHitValid;
    }

    // ----------------- Public API -----------------
    /// <summary>Externally request lock/unlock of sliding (true = lock/stop sliding, false = release).</summary>
    public void SetPoseLock(bool lockSliding)
    {
        poseLockRequested = lockSliding;
        if (lockSliding)
        {
            isPoseActive = false;
            poseTimer = 0f;
            if (enableVerboseLogs) Debug.Log("[PoseSlider] External pose lock applied");
        }
        else
        {
            if (enableVerboseLogs) Debug.Log("[PoseSlider] External pose lock released");
        }
    }

    /// <summary>Return whether sliding is currently blocked (pose or api lock)</summary>
    public bool IsSlidingLocked()
    {
        return poseLockRequested || isPoseActive;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || cc == null) return;

        Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
        Vector3 bottomWorld = transform.position + bottomLocal;
        Vector3 origin = bottomWorld + Vector3.up * 0.12f;

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

        // draw pose visualization
        if (headTransform != null && leftHandTransform != null && rightHandTransform != null)
        {
            Gizmos.color = isPoseActive ? Color.green : Color.yellow;
            Gizmos.DrawSphere(headTransform.position, 0.03f);
            Gizmos.DrawSphere(leftHandTransform.position, 0.02f);
            Gizmos.DrawSphere(rightHandTransform.position, 0.02f);
            Gizmos.DrawLine(leftHandTransform.position, rightHandTransform.position);
        }
    }
}

