using UnityEngine;

/// <summary>
/// Minimal GameManager that safely handles OnPoseCompleted and OnWaveCompleted
/// without assuming an Enemy tag or any enemy objects exist.
/// </summary>
public class GameManager2 : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject boomEffectPrefab;
    public float boomDuration = 1f;
    public float killRadius = 5f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = null;
            try { p = GameObject.FindGameObjectWithTag("MainPlayer"); }
            catch (UnityException) { /* tag missing -- ignore */ }

            if (p != null) player = p.transform;
        }
    }

    /// <summary>
    /// Called by PoseManagerUI when a pose is completed.
    /// This method attempts to destroy objects tagged "Enemy" within killRadius,
    /// but does so safely: if the "Enemy" tag does not exist it logs and returns.
    /// </summary>
    public void OnPoseCompleted()
    {
        if (player == null)
        {
            Debug.LogWarning("GameManager2: player is null; cannot destroy nearby enemies.");
            return;
        }

        // Try to find objects with tag "Enemy". If the tag doesn't exist,
        // GameObject.FindGameObjectsWithTag throws UnityException — catch it.
        GameObject[] enemies = null;
        try
        {
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
        }
        catch (UnityException)
        {
            // Tag doesn't exist in project settings — that's fine if you have no enemies.
            Debug.Log("GameManager2: 'Enemy' tag not defined. No enemies to destroy.");
            return;
        }

        if (enemies == null || enemies.Length == 0)
        {
            Debug.Log("GameManager2: No enemies found to destroy.");
            return;
        }

        int destroyed = 0;
        foreach (GameObject e in enemies)
        {
            if (e == null) continue;
            float dist = Vector3.Distance(player.position, e.transform.position);
            if (dist <= killRadius)
            {
                // optional boom effect
                if (boomEffectPrefab != null)
                {
                    GameObject boom = Instantiate(boomEffectPrefab, e.transform.position, Quaternion.identity);
                    Destroy(boom, boomDuration);
                }

                Destroy(e);
                destroyed++;
            }
        }

        Debug.Log($"GameManager2: Destroyed {destroyed} enemies within {killRadius} units.");
    }

    /// <summary>
    /// Placeholder for wave-spawn notification. Does nothing by default but kept for compatibility.
    /// </summary>
    public void OnWaveCompleted(int waveNumber)
    {
        Debug.Log($"GameManager2: OnWaveCompleted called for wave {waveNumber} (no spawner logic in this project).");
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, killRadius);
        }
    }
}
