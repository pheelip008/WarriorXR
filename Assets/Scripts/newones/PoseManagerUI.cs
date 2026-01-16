// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// public class PoseManagerUI : MonoBehaviour
// {
//     [Header("References")]
//     public MocopiPoseRecorder poseRecorder;
//     public GameObject gameManager;
//     public PlayerHealth playerHealth;

//     [Header("UI")]
//     public TextMeshProUGUI statusText;
//     public TextMeshProUGUI matchPercentText;
//     public TextMeshProUGUI poseNameText;

//     [Header("Game Settings")]
//     public string[] poseNames = { "PoseA", "PoseB", "PoseC" };
//     public float holdTime = 5f;

//     [Header("Controls")]
//     public KeyCode restartKey = KeyCode.R;
//     public KeyCode deleteAllKey = KeyCode.Delete;

//     enum Mode { Recording, Playing }
//     Mode currentMode;

//     int currentWave = 1;
//     int currentPoseIndex = 0;
//     List<string> posesThisWave = new List<string>();

//     float timer = 0f;
//     bool waitingForNext = false;

//     void Start()
//     {
//         if (poseRecorder != null && poseRecorder.GetRecordedPoseCount() > 0)
//             StartPlayMode();
//         else
//             StartRecordingMode();

//         UpdateUIAll();
//     }

//     void Update()
//     {
//         if (currentMode == Mode.Playing) UpdatePlayMode();

//         if (Input.GetKeyDown(restartKey)) StartRecordingMode();

//         if (Input.GetKeyDown(deleteAllKey))
//         {
//             if (poseRecorder != null) poseRecorder.DeleteAllSavedPoses();
//             StartRecordingMode();
//             UpdateUIAll();
//         }
//     }

//     // ------------------------------- RECORDING MODE -------------------------------
//     void StartRecordingMode()
//     {
//         currentMode = Mode.Recording;
//         currentWave = 1;
//         currentPoseIndex = 0;
//         posesThisWave.Clear();
//         timer = 0f;
//         waitingForNext = false;

//         if (statusText != null)
//             statusText.text = "RECORDING MODE\nPress R to record poses";

//         Debug.Log("PoseManagerUI: Recording mode started.");
//     }

//     // ------------------------------- PLAY MODE -------------------------------
//     void StartPlayMode()
//     {
//         currentMode = Mode.Playing;
//         currentWave = 1;
//         currentPoseIndex = 0;
//         timer = 0f;
//         waitingForNext = false;

//         GenerateWave();
//         ShowCurrentPosePrompt();

//         if (gameManager != null)
//             gameManager.SendMessage("OnWaveCompleted", currentWave, SendMessageOptions.DontRequireReceiver);

//         Debug.Log("PoseManagerUI: Play mode started.");
//     }

//     void GenerateWave()
//     {
//         posesThisWave.Clear();
//         for (int i = 0; i < 3; i++)
//         {
//             posesThisWave.Add(poseNames[Random.Range(0, poseNames.Length)]);
//         }
//     }

//     void ShowCurrentPosePrompt()
//     {
//         string pose = posesThisWave[currentPoseIndex];

//         if (poseNameText != null) poseNameText.text = pose;
//         if (matchPercentText != null) matchPercentText.text = "Match: 0%";
//         if (statusText != null) statusText.text = $"WAVE {currentWave}\nDo Pose:\n<b>{pose}</b>";
//     }

//     void UpdatePlayMode()
//     {
//         if (waitingForNext) return;
//         if (posesThisWave.Count == 0) return;

//         string target = posesThisWave[currentPoseIndex];
//         float similarity = poseRecorder.CompareToPose(target, false);
//         float percent = Mathf.Round(similarity * 100f);

//         if (matchPercentText != null) matchPercentText.text = $"Match: {percent}%";

//         bool matched = similarity >= poseRecorder.similarityThreshold;

//         GameObject player = SafeFindMainPlayer();
//         SlopePlayer_CharacterController slope = null;
//         if (player != null) slope = player.GetComponent<SlopePlayer_CharacterController>();

//         if (matched)
//         {
//             // Disable sliding
//             if (slope != null) slope.enabled = false;

//             if (playerHealth != null) playerHealth.SetInvulnerable(true);

//             timer += Time.deltaTime;
//             int secondsLeft = Mathf.CeilToInt(holdTime - timer);

//             if (statusText != null)
//                 statusText.text = $"SAFE!\nHold {secondsLeft}s";

//             if (timer >= holdTime)
//                 StartCoroutine(OnPoseFinished());
//         }
//         else
//         {
//             // Re-enable sliding
//             if (slope != null) slope.enabled = true;

//             if (playerHealth != null) playerHealth.SetInvulnerable(false);

//             timer = 0f;

//             if (statusText != null)
//                 statusText.text = $"Match the pose:\n<b>{target}</b>\nMatch: {percent}%";
//         }
//     }

//     IEnumerator OnPoseFinished()
//     {
//         waitingForNext = true;

//         string completed = posesThisWave[currentPoseIndex];

//         if (gameManager != null)
//             gameManager.SendMessage("OnPoseCompleted", SendMessageOptions.DontRequireReceiver);

//         if (statusText != null)
//             statusText.text = $"Perfect!\n{completed}";

//         yield return new WaitForSeconds(1.2f);

//         currentPoseIndex++;

//         if (currentPoseIndex >= posesThisWave.Count)
//         {
//             currentWave++;

//             if (currentWave > 3)
//             {
//                 if (statusText != null)
//                     statusText.text = "ALL WAVES COMPLETE!\nPress R to restart";
//                 yield break;
//             }

//             currentPoseIndex = 0;
//             GenerateWave();
//             waitingForNext = false;
//             timer = 0f;
//             ShowCurrentPosePrompt();
//         }
//         else
//         {
//             waitingForNext = false;
//             timer = 0f;
//             ShowCurrentPosePrompt();
//         }
//     }

//     // ------------------------------- HELPERS -------------------------------
//     void UpdateUIAll()
//     {
//         if (poseRecorder != null && statusText != null)
//             statusText.text = $"Saved poses: {poseRecorder.GetRecordedPoseCount()}\nPress Play to begin.";
//     }

//     GameObject SafeFindMainPlayer()
//     {
//         try { return GameObject.FindGameObjectWithTag("MainPlayer"); }
//         catch { return null; }
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PoseManagerUI : MonoBehaviour
{
    [Header("References")]
    public MocopiPoseRecorder poseRecorder;
    public GameObject gameManager;       // optional, use SendMessage on it
    public PlayerHealth playerHealth;    // optional (invulnerability)

    [Header("UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI matchPercentText;
    public TextMeshProUGUI poseNameText;
    public PoseItemSpawner poseItemSpawner;

    [Header("Game Settings")]
    public bool useSavedPoses = true;
    public string[] fallbackPoseNames = { "Warrior1", "Warrior2", "Warrior3" };
    public float holdTime = 5f;

    [Header("Controls")]
    public KeyCode restartKey = KeyCode.R;
    public KeyCode deleteAllKey = KeyCode.Delete;

    enum Mode { Recording, Playing }
    Mode currentMode;

    int currentWave = 1;
    int currentPoseIndex = 0;
    List<string> posesThisWave = new List<string>();

    // pool of available pose names (from saved or fallback)
    List<string> posePool = new List<string>();

    float timer = 0f;
    bool waitingForNext = false;

    void Start()
    {
        BuildPosePool();

        if (poseRecorder != null && poseRecorder.GetRecordedPoseCount() > 0)
            StartPlayMode();
        else
            StartRecordingMode();

        UpdateUIAll();
    }

    void Update()
    {
        if (currentMode == Mode.Playing) UpdatePlayMode();

        if (Input.GetKeyDown(restartKey)) StartRecordingMode();

        if (Input.GetKeyDown(deleteAllKey))
        {
            if (poseRecorder != null) poseRecorder.DeleteAllSavedPoses();
            StartRecordingMode();
            BuildPosePool();
            UpdateUIAll();
        }
    }

    #region Mode control

    void StartRecordingMode()
    {
        currentMode = Mode.Recording;
        currentWave = 1;
        currentPoseIndex = 0;
        posesThisWave.Clear();
        waitingForNext = false;
        timer = 0f;

        if (statusText != null) statusText.text = "RECORDING MODE\nPress R to record poses";
        Debug.Log("PoseManagerUI: Recording mode started.");
    }

    void StartPlayMode()
    {
        if (posePool == null || posePool.Count == 0)
        {
            BuildPosePool();
            if (posePool.Count == 0)
            {
                Debug.LogWarning("PoseManagerUI: No poses available to play.");
                StartRecordingMode();
                return;
            }
        }

        currentMode = Mode.Playing;
        currentWave = 1;
        currentPoseIndex = 0;
        waitingForNext = false;
        timer = 0f;

        GenerateWave();
        ShowCurrentPosePrompt();

        if (gameManager != null) gameManager.SendMessage("OnWaveCompleted", currentWave, SendMessageOptions.DontRequireReceiver);

        Debug.Log("PoseManagerUI: Play mode started.");
    }

    void GenerateWave()
    {
        posesThisWave.Clear();
        // pick up to 3 unique poses from posePool
        var copy = new List<string>(posePool);
        for (int i = 0; i < 3 && copy.Count > 0; i++)
        {
            int idx = Random.Range(0, copy.Count);
            posesThisWave.Add(copy[idx]);
            copy.RemoveAt(idx);
        }
    }

    void ShowCurrentPosePrompt()
    {
        if (posesThisWave.Count == 0) return;
        string pose = posesThisWave[currentPoseIndex];
        if (poseNameText != null) poseNameText.text = pose;
        if (matchPercentText != null) matchPercentText.text = "Match: 0%";
        if (statusText != null) statusText.text = $"WAVE {currentWave}\nDo Pose:\n<b>{pose}</b>";
        if (poseItemSpawner != null);
        
        poseItemSpawner.SetActivePose(pose);
        Debug.Log($"[PoseManagerUI] Active pose: {pose}");

    }

    #endregion

    #region Play logic

    void UpdatePlayMode()
    {
        if (waitingForNext) return;
        if (posesThisWave.Count == 0) return;

        string target = posesThisWave[currentPoseIndex];

        if (poseRecorder == null)
        {
            Debug.LogWarning("PoseManagerUI: poseRecorder is null.");
            return;
        }

        // If target missing (deleted externally), rebuild pool and skip gracefully.
        if (!poseRecorder.IsPoseRecorded(target))
        {
            Debug.LogWarning($"PoseManagerUI: Target pose '{target}' missing. Rebuilding pool and skipping.");
            BuildPosePool();
            StartCoroutine(SkipToNextPose());
            return;
        }

        float similarity = poseRecorder.CompareToPose(target, false);
        float percent = Mathf.Round(similarity * 100f);

        if (matchPercentText != null) matchPercentText.text = $"Match: {percent}%";

        bool matched = similarity >= poseRecorder.similarityThreshold;

        GameObject player = SafeFindMainPlayer();
        SlopePlayer_CharacterController slope = null;
        if (player != null) slope = player.GetComponent<SlopePlayer_CharacterController>();

        if (matched)
        {
            // disable sliding
            if (slope != null) slope.enabled = false;

            if (playerHealth != null) playerHealth.SetInvulnerable(true);

            timer += Time.deltaTime;
            int secondsLeft = Mathf.CeilToInt(holdTime - timer);
            if (statusText != null) statusText.text = $"SAFE!\nHold {secondsLeft}s";

            if (timer >= holdTime) StartCoroutine(OnPoseFinished());
        }
        else
        {
            // enable sliding
            if (slope != null) slope.enabled = true;

            if (playerHealth != null) playerHealth.SetInvulnerable(false);

            timer = 0f;
            if (statusText != null) statusText.text = $"Match the pose:\n<b>{target}</b>\nMatch: {percent}%";
        }
    }

    IEnumerator SkipToNextPose()
    {
        yield return new WaitForSeconds(0.5f);

        currentPoseIndex++;
        if (currentPoseIndex >= posesThisWave.Count)
        {
            currentWave++;
            if (currentWave > 3)
            {
                if (statusText != null) statusText.text = "ALL WAVES COMPLETE!\nPress R to restart";
                yield break;
            }
            currentPoseIndex = 0;
            GenerateWave();
        }
        ShowCurrentPosePrompt();
    }

    IEnumerator OnPoseFinished()
    {
        waitingForNext = true;
        string completed = posesThisWave[currentPoseIndex];

        if (gameManager != null) gameManager.SendMessage("OnPoseCompleted", SendMessageOptions.DontRequireReceiver);

        if (statusText != null) statusText.text = $"Perfect!\n{completed}";

        yield return new WaitForSeconds(1.2f);

        currentPoseIndex++;
        if (currentPoseIndex >= posesThisWave.Count)
        {
            //currentWave++;
            //if (currentWave > 3)
            //{
            //    if (statusText != null) statusText.text = "ALL WAVES COMPLETE!\nPress R to restart";
            //    yield break;
            //
            //}
           
            currentWave = 1;
            currentPoseIndex = 0;
            GenerateWave();
            waitingForNext = false;
            timer = 0f;
            ShowCurrentPosePrompt();
            yield break;

            //currentPoseIndex = 0;
            //GenerateWave();
            //waitingForNext = false;
            //timer = 0f;
            //ShowCurrentPosePrompt();
        }
        else
        {
            waitingForNext = false;
            timer = 0f;
            ShowCurrentPosePrompt();
        }
    }

    #endregion

    #region Pool building & helpers

    void BuildPosePool()
    {
        posePool.Clear();

        if (useSavedPoses && poseRecorder != null)
        {
            var saved = poseRecorder.GetAllSavedPoseNames();
            if (saved != null && saved.Count > 0)
            {
                // filter to ensure they actually exist in recorder
                foreach (var n in saved)
                    if (poseRecorder.IsPoseRecorded(n))
                        posePool.Add(n);

                if (posePool.Count > 0)
                {
                    Debug.Log($"PoseManagerUI: using {posePool.Count} saved poses.");
                    return;
                }
            }
        }

        // fallback to inspector list (only include those that exist if recorder has saved ones)
        if (fallbackPoseNames != null && fallbackPoseNames.Length > 0)
        {
            foreach (var n in fallbackPoseNames)
            {
                if (poseRecorder != null && poseRecorder.GetRecordedPoseCount() > 0)
                {
                    if (poseRecorder.IsPoseRecorded(n)) posePool.Add(n);
                }
                else
                {
                    posePool.Add(n);
                }
            }
        }

        Debug.Log($"PoseManagerUI: built pose pool with {posePool.Count} entries.");
    }

    void UpdateUIAll()
    {
        if (poseRecorder != null && statusText != null)
        {
            int count = poseRecorder.GetRecordedPoseCount();
            statusText.text = $"Saved poses: {count}\nPress Play to begin.";
        }
        if (matchPercentText != null) matchPercentText.text = "";
        if (poseNameText != null) poseNameText.text = "";
    }

    GameObject SafeFindMainPlayer()
    {
        try { return GameObject.FindGameObjectWithTag("MainPlayer"); }
        catch { return null; }
    }

    #endregion

    #region Inspector helpers & deletion API

    /// <summary>
    /// Print saved pose names to console (context menu friendly).
    /// </summary>
    [ContextMenu("LogSavedPoses")]
    public void LogSavedPoses()
    {
        if (poseRecorder == null) { Debug.Log("PoseManagerUI: poseRecorder null."); return; }
        var names = poseRecorder.GetAllSavedPoseNames();
        Debug.Log($"PoseManagerUI: Saved poses ({names.Count}): {string.Join(", ", names)}");
    }

    /// <summary>
    /// Delete the currently-targeted saved pose (if it exists in the record pool).
    /// Callable from other UI buttons or via Inspector context menu.
    /// </summary>
    public bool DeleteCurrentSavedPose()
    {
        if (posesThisWave == null || posesThisWave.Count == 0)
        {
            Debug.LogWarning("PoseManagerUI: No current pose to delete.");
            return false;
        }

        string target = posesThisWave[currentPoseIndex];
        if (!poseRecorder.IsPoseRecorded(target))
        {
            Debug.LogWarning($"PoseManagerUI: Cannot delete '{target}' — not found.");
            return false;
        }

        bool ok = poseRecorder.DeletePose(target);
        if (ok)
        {
            Debug.Log($"PoseManagerUI: Deleted saved pose '{target}'. Rebuilding pool.");
            BuildPosePool();
            // move safely to next pose
            StartCoroutine(SkipToNextPose());
        }
        else
        {
            Debug.LogError($"PoseManagerUI: Failed to delete pose '{target}'.");
        }
        return ok;
    }

    #endregion
}
