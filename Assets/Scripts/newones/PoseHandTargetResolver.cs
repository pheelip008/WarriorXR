using UnityEngine;

public class PoseHandTargetResolver : MonoBehaviour
{
    [Header("References")]
    public MocopiPoseRecorder poseRecorder;
    public Transform playerRoot;

    [Header("Tuning")]
    public float reachDistance = 0.75f;
    public float heightOffset = 1.3f;

    public Vector3 GetHandTarget(string poseName, bool leftHand)
    {
        if (poseRecorder == null || playerRoot == null)
            return playerRoot.position;

        // Bone names from your screenshot
        string upper = leftHand ? "leftUpperArm" : "rightUpperArm";
        string fore = leftHand ? "leftForearm" : "rightForearm";

        // Get the CURRENT transforms (live mocopi skeleton)
        Transform upperT = poseRecorder.GetBoneTransform(upper);
        Transform foreT = poseRecorder.GetBoneTransform(fore);

        if (upperT == null || foreT == null)
            return playerRoot.position;

        // Direction from upper arm → forearm (THIS IS THE FIX)
        Vector3 dir = (foreT.position - upperT.position);

        // Convert to player-local space so pan movement doesn't break it
        dir = playerRoot.InverseTransformDirection(dir);

        // Flatten + comfort clamp
        dir.y = Mathf.Clamp(dir.y, -0.2f, 0.6f);

        if (dir.sqrMagnitude < 0.001f)
            dir = leftHand ? Vector3.left : Vector3.right;

        dir.Normalize();

        // Back to world space
        dir = playerRoot.TransformDirection(dir);

        return playerRoot.position
             + Vector3.up * heightOffset
             + dir * reachDistance;

        Debug.DrawRay(
        playerRoot.position + Vector3.up * heightOffset,
        dir * reachDistance,
        leftHand ? Color.blue : Color.red
    );

    }


}
