using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public CollectibleType itemType;
    public float moveSpeed = 2f;

    Transform targetHand;
    Vector3 worldTarget;
    bool useWorldTarget = false;
    bool collected = false;

    // OLD MODE (not used right now, but kept)
    public void Initialize(Transform handTarget)
    {
        targetHand = handTarget;
        useWorldTarget = false;
    }

    // NEW MODE (POSE-BASED TARGET)
    public void InitializeWorldTarget(Vector3 target)
    {
        worldTarget = target;
        useWorldTarget = true;
    }

    void Update()
    {
        if (collected) return;

        Vector3 targetPos;

        if (useWorldTarget)
            targetPos = worldTarget;
        else if (targetHand != null)
            targetPos = targetHand.position;
        else
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    public void Collect()
    {
        if (collected) return;
        collected = true;

        SaltPepperCounter.Instance?.OnCollected(itemType);

        Destroy(gameObject);
    }
}
