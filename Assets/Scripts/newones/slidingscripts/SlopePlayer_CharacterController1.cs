using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SlopePlayer_CharacterController : MonoBehaviour
{
    [Header("Sliding Settings")]
    public Transform panTransform;         // set to the pan
    public float slideSpeed = 3f;          // base slide speed
    public float gravityScale = 1f;        // increases slide magnitude
    public float minSlopeAngleToSlide = 1f;// degrees

    CharacterController cc;
    Vector3 verticalVelocity = Vector3.zero;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (panTransform == null) Debug.LogError("panTransform not set on SlopePlayer_CharacterController!");
    }
    


    void Update()
    {
        if (panTransform == null) return;

        Vector3 planeNormal = panTransform.up.normalized;
        float slopeAngle = Vector3.Angle(planeNormal, Vector3.up);
        if (slopeAngle < minSlopeAngleToSlide) return;

        // Slide direction: gravity projected onto plane
        Vector3 gravity = Physics.gravity * gravityScale;
        Vector3 slideDir = Vector3.ProjectOnPlane(gravity, planeNormal).normalized;

        // magnitude scaled by slope
        float slideMagnitude = Mathf.Abs(Mathf.Sin(slopeAngle * Mathf.Deg2Rad)) * slideSpeed;
        Vector3 move = slideDir * slideMagnitude;

        // Always call CharacterController.Move in Update
        cc.Move(move * Time.deltaTime);
       
    }
}
