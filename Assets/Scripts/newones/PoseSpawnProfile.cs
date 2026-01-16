using UnityEngine;

[CreateAssetMenu(menuName = "Pose/Pose Spawn Profile")]
public class PoseSpawnProfile : ScriptableObject
{
    public string poseName;

    [Header("Salt")]
    public Vector3 saltSpawnOffset;
    public float saltSpeed = 2f;

    [Header("Pepper")]
    public Vector3 pepperSpawnOffset;
    public float pepperSpeed = 2f;
}
