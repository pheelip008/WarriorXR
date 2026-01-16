using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PoseManager : MonoBehaviour
{
    [Header("References")]
    public MocopiPoseRecorder poseRecorder;
    public TextMeshProUGUI statusText;
    public GameObject gameManager;      // NOTE: now a GameObject so we can SendMessage safely
    public PlayerHealth playerHealth;   // optional

    [Header("Pose Settings")]
    public float holdTime = 5f;
    public string[] poseNames = { "PoseA", "PoseB", "PoseC" };

    [Header("Controls")]
    public KeyCode recordKey = KeyCode.Space;
    public KeyCode restartKey = KeyCode.R;
    public KeyCode deleteAllKey = KeyCode.Delete;

    enum Mode { Recording, Playing }
    Mode currentMode;

    int currentPoseIndex = 0;
    int currentWave = 0;
    List<string> currentWavePoses = new List<string>();
    float timer = 0f;
    bool waitingForNext = false;

    void Start()
    {
        if (poseRecorder == null) Debug.LogWarning("PoseManager: poseRecorder not assigned.");
        if (playerHealth == null) Debug.LogWarning("PoseManager: playerHealth not assigned (invulnerability won't work).");

        if (poseRecorder != null && poseRecorder.GetRecordedPoseCount() > 0) StartPlayMode();
        else StartRecordingMode();
    }

    void Update()
    {
        if (currentMode == Mode.Recording) UpdateRecordingMode();
        else UpdatePlayMode();

        if (Input.GetKeyDown(restartKey)) StartRecordingMode();

        if (Input.GetKeyDown(deleteAllKey))
        {
            if (poseRecorder != null) poseRecorder.DeleteAllSavedPoses();
            StartRecordingMode();
        }
    }

    // ---------- Recording ----------
    void StartRecordingMode()
    {
        currentMode = Mode.Recording;
        currentPoseIndex = 0;
        timer = 0f;
        waitingForNext = false;

        if (statusText != null)
        {
            string first = poseNames.Length > 0 ? poseNames[0] : "POSE";
            statusText.text = $"RECORDING\n\n<b>{first}</b>\n\nPress {recordKey}";
            statusText.color = Color.cyan;
        }

        Debug.Log("PoseManager: Recording mode started.");
    }

    void UpdateRecordingMode()
    {
        if (Input.GetKeyDown(recordKey))
        {
            string poseName = poseNames[Mathf.Clamp(currentPoseIndex, 0, Mathf.Max(0, poseNames.Length - 1))];
            if (poseRecorder != null) poseRecorder.RecordAndSavePose(poseName);
            else Debug.LogWarning("PoseManager: poseRecorder is null — cannot record.");

            if (statusText != null)
            {
                statusText.text = $"{poseName} Recorded!";
                statusText.color = Color.green;
            }

            currentPoseIndex++;
            if (currentPoseIndex >= poseNames.Length) StartCoroutine(FinishRecording());
            else StartCoroutine(ShowNextPoseToRecord());
        }
    }

    IEnumerator ShowNextPoseToRecord()
    {
        yield return new WaitForSeconds(1.2f);
        if (statusText != null)
        {
            string next = poseNames[Mathf.Clamp(currentPoseIndex, 0, poseNames.Length - 1)];
            statusText.text = $"RECORDING\n\n<b>{next}</b>\n\nPress {recordKey}";
            statusText.color = Color.cyan;
        }
    }

    IEnumerator FinishRecording()
    {
        if (statusText != null)
        {
            statusText.text = "All Poses Saved!\n\nStarting game...";
            statusText.color = Color.yellow;
        }

        yield return new WaitForSeconds(2f);
        StartPlayMode();
    }

    // ---------- Play ----------
    void StartPlayMode()
    {
        currentMode = Mode.Playing;
        currentWave = 1;
        currentPoseIndex = 0;
        timer = 0f;
        waitingForNext = false;

        GeneratePosesForWave(currentWave);

        if (statusText != null)
        {
            statusText.text = $"WAVE {currentWave}\n\nGet Ready...";
            statusText.color = Color.red;
        }

        // Call gameManager.OnWaveCompleted(wave) if exists (safe)
        if (gameManager != null)
        {
            // use SendMessage so it won't error if method missing
            gameManager.SendMessage("OnWaveCompleted", currentWave, SendMessageOptions.DontRequireReceiver);
        }

        StartCoroutine(StartWaveCoroutine(currentWave));
    }

    IEnumerator StartWaveCoroutine(int waveNumber)
    {
        if (statusText != null)
        {
            statusText.text = $"WAVE {waveNumber}\n\nEnemies incoming!";
            statusText.color = Color.red;
        }

        yield return new WaitForSeconds(1.2f);
        ShowCurrentPosePrompt();
    }

    void GeneratePosesForWave(int wave)
    {
        currentWavePoses.Clear();
        for (int i = 0; i < 3; i++)
        {
            string p = poseNames[Random.Range(0, poseNames.Length)];
            currentWavePoses.Add(p);
        }

        Debug.Log($"PoseManager: Wave {wave} poses: {string.Join(", ", currentWavePoses)}");
    }

    void ShowCurrentPosePrompt()
    {
        if (statusText != null)
        {
            string firstPose = currentWavePoses.Count > 0 ? currentWavePoses[currentPoseIndex] : "POSE";
            statusText.text = $"WAVE {currentWave}\n\n<b>{firstPose}</b>\n\nPose {currentPoseIndex + 1}/3";
            statusText.color = Color.white;
        }
    }

    void UpdatePlayMode()
    {
        if (waitingForNext) return;
        if (currentWavePoses.Count == 0) return;

        string targetPoseName = currentWavePoses[currentPoseIndex];
        bool poseMatched = poseRecorder != null && poseRecorder.IsCurrentPoseMatching(targetPoseName);

        GameObject playerObj = GameObject.FindGameObjectWithTag("MainPlayer");
        SlopePlayer_CharacterController slopeCtrl = null;
        if (playerObj != null) slopeCtrl = playerObj.GetComponent<SlopePlayer_CharacterController>();

        if (poseMatched)
        {
            if (slopeCtrl != null) slopeCtrl.enabled = false;
            if (playerHealth != null) playerHealth.SetInvulnerable(true);

            timer += Time.deltaTime;
            int secondsLeft = Mathf.CeilToInt(holdTime - timer);

            if (statusText != null)
            {
                statusText.text = $"WAVE {currentWave}\n<b>{targetPoseName}</b>\n\nSAFE\n<size=80><b>{secondsLeft}</b></size>s\n\n{currentPoseIndex + 1}/3";
                statusText.color = Color.green;
            }

            if (timer >= holdTime) StartCoroutine(PoseCompleted());
        }
        else
        {
            if (slopeCtrl != null) slopeCtrl.enabled = true;
            if (playerHealth != null) playerHealth.SetInvulnerable(false);
            timer = 0f;

            if (statusText != null)
            {
                statusText.text = $"WAVE {currentWave}\n\n<b>{targetPoseName}</b>\n\n{currentPoseIndex + 1}/3";
                statusText.color = Color.white;
            }
        }
    }

    IEnumerator PoseCompleted()
    {
        waitingForNext = true;
        string completedPose = currentWavePoses[currentPoseIndex];

        // Notify gameManager if it has OnPoseCompleted()
        if (gameManager != null)
        {
            gameManager.SendMessage("OnPoseCompleted", SendMessageOptions.DontRequireReceiver);
        }

        if (statusText != null)
        {
            statusText.text = $"Perfect!\n{completedPose}\n\nBOOM!";
            statusText.color = Color.green;
        }

        yield return new WaitForSeconds(1.2f);

        currentPoseIndex++;

        if (currentPoseIndex >= currentWavePoses.Count)
        {
            if (statusText != null)
            {
                statusText.text = $"WAVE {currentWave}\nCOMPLETE!";
                statusText.color = Color.yellow;
            }

            yield return new WaitForSeconds(1.2f);

            currentWave++;
            if (currentWave > 3)
            {
                if (statusText != null)
                {
                    statusText.text = "ALL COMPLETE!\n\nYoga Master!\n\n<size=40>Press R to restart</size>";
                    statusText.color = Color.yellow;
                }
                yield break;
            }

            currentPoseIndex = 0;
            GeneratePosesForWave(currentWave);
            waitingForNext = false;
            timer = 0f;
            StartCoroutine(StartWaveCoroutine(currentWave));
        }
        else
        {
            waitingForNext = false;
            timer = 0f;
            ShowCurrentPosePrompt();
        }
    }
}
