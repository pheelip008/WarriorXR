using UnityEngine;

public class HandCollector : MonoBehaviour
{
    public CollectibleType acceptsType; // Salt OR Pepper

    void OnTriggerEnter(Collider other)
    {
        CollectibleItem item = other.GetComponent<CollectibleItem>();
        if (item == null) return;

        if (item.itemType != acceptsType) return;

        item.Collect();
    }
}
