using UnityEngine;
using System.Reflection;

[RequireComponent(typeof(CharacterController))]
public class TestGroundProbe : MonoBehaviour
{
    public LayerMask groundLayer = ~0;
    public float sphereCastRadius = 0.3f;
    public float sphereCastDistance = 1.2f;

    // try to detect your slider/pause API by name (non-invasive)
    object sliderComponent = null;
    MethodInfo isLockedMethod = null;
    MethodInfo setPoseLockMethod = null;

    void Start()
    {
        // attempt to find any component that looks like your slope/pose script
        var comps = GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            var t = c.GetType();
            if (t.Name.ToLower().Contains("slopeslider") || t.Name.ToLower().Contains("poseslider") || t.Name.ToLower().Contains("slope"))
            {
                sliderComponent = c;
                isLockedMethod = t.GetMethod("IsSlidingLocked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                setPoseLockMethod = t.GetMethod("SetPoseLock", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Debug.LogFormat("[TestProbe] Found slider component: {0}, IsLockedMethod:{1}, SetPoseLockMethod:{2}", t.Name, isLockedMethod != null, setPoseLockMethod != null);
                break;
            }
        }
    }

    void Update()
    {
        CharacterController cc = GetComponent<CharacterController>();
        Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
        Vector3 bottomWorld = transform.position + bottomLocal;
        Vector3 origin = bottomWorld + Vector3.up * 0.12f;

        RaycastHit hit;
        bool found = Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, groundLayer, QueryTriggerInteraction.Ignore);
        if (!found)
        {
            Debug.LogFormat("[TestProbe] Not grounded. origin={0} dist={1} radius={2}", origin.ToString("F3"), sphereCastDistance, sphereCastRadius);
        }
        else
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            Debug.LogFormat("[TestProbe] Hit: {0} at {1}. slopeAngle={2:0.0} deg. Normal={3}", 
                hit.collider ? hit.collider.name : "null", hit.point.ToString("F3"), slopeAngle, hit.normal.ToString("F3"));
        }

        // report slider/pose status if detected
        if (sliderComponent != null)
        {
            if (isLockedMethod != null)
            {
                bool locked = (bool)isLockedMethod.Invoke(sliderComponent, null);
                Debug.LogFormat("[TestProbe] Slider locked? {0}", locked);
            }
            else
            {
                Debug.Log("[TestProbe] Slider component found but cannot call IsSlidingLocked()");
            }

            if (setPoseLockMethod != null)
            {
                // just report availability; do NOT call automatically
                Debug.Log("[TestProbe] SetPoseLock(bool) available on slider component.");
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        CharacterController cc = GetComponent<CharacterController>();
        if (cc == null) return;
        Vector3 bottomLocal = Vector3.down * (cc.height * 0.5f - cc.skinWidth) + cc.center;
        Vector3 bottomWorld = transform.position + bottomLocal;
        Vector3 origin = bottomWorld + Vector3.up * 0.12f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, sphereCastRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * sphereCastDistance);
    }
}
