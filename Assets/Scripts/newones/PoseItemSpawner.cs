using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseItemSpawner : MonoBehaviour
{
    [Header("Profiles")]
    public List<PoseSpawnProfile> profiles;

    [Header("Prefabs")]
    public GameObject saltPrefab;
    public GameObject pepperPrefab;
    public PoseHandTargetResolver handTargetResolver;


    [Header("Hand Targets")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Spawn Timing")]
    public float spawnInterval = 2f;

    PoseSpawnProfile currentProfile;
    Coroutine spawnRoutine;

    // Called by PoseManagerUI
    public void SetActivePose(string poseName)
    {
        currentProfile = profiles.Find(p => p.poseName == poseName);

        if (currentProfile == null)
        {
            Debug.LogWarning($"[PoseItemSpawner] No profile for pose '{poseName}'");
            StopSpawning();
            return;
        }
        

        StartSpawning();
        Debug.Log($"[PoseItemSpawner] SetActivePose called with: {poseName}");

    }

    void StartSpawning()
    {
        StopSpawning();
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnSalt();
            SpawnPepper();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnSalt()
{
    if (currentProfile == null || saltPrefab == null) return;

    Vector3 target =
        handTargetResolver.GetHandTarget(currentProfile.poseName, true);

    GameObject obj = Instantiate(
        saltPrefab,
        transform.position,
        Quaternion.identity
    );

    CollectibleItem item = obj.GetComponent<CollectibleItem>();
    item.itemType = CollectibleType.Salt;
    item.moveSpeed = currentProfile.saltSpeed;
    item.InitializeWorldTarget(target);
}



    void SpawnPepper()
{
    if (currentProfile == null || pepperPrefab == null) return;

    Vector3 target =
        handTargetResolver.GetHandTarget(currentProfile.poseName, false);

    GameObject obj = Instantiate(
        pepperPrefab,
        transform.position,
        Quaternion.identity
    );

    CollectibleItem item = obj.GetComponent<CollectibleItem>();
    item.itemType = CollectibleType.Pepper;
    item.moveSpeed = currentProfile.pepperSpeed;
    item.InitializeWorldTarget(target);
}

}
