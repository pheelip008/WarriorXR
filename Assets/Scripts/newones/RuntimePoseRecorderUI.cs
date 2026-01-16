// // RuntimePoseRecorderUI.cs
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// [DisallowMultipleComponent]
// public class RuntimePoseRecorderUI : MonoBehaviour
// {
//     [Header("References")]
//     public MocopiPoseRecorder mocopiRecorder;    // assign your MocopiPoseRecorder
//     public TextMeshProUGUI recordingText;        // "Recording..." / "Not recording"
//     public TextMeshProUGUI savedCountText;      // "Saved poses: 3"
//     public TextMeshProUGUI sampleCountText;     // "Samples: 12" (optional)

//     [Header("Recording Settings")]
//     public KeyCode recordKey = KeyCode.R;
//     public float sampleInterval = 0.1f;         // seconds between samples
//     [Tooltip("0 = no limit")]
//     public int maxSavedPoses = 0;

//     // internal
//     bool isRecording = false;
//     float sampleTimer = 0f;
//     List<Dictionary<string, Quaternion>> samples = new List<Dictionary<string, Quaternion>>();

//     void Start()
//     {
//         if (mocopiRecorder == null) Debug.LogWarning("[RuntimePoseRecorderUI] mocopiRecorder not set!");
//         UpdateUI();
//     }

//     void Update()
//     {
//         if (Input.GetKeyDown(recordKey))
//         {
//             if (!isRecording) StartRecording();
//             else StopRecordingAndSave();
//         }

//         if (!isRecording) return;

//         sampleTimer += Time.deltaTime;
//         if (sampleTimer >= sampleInterval)
//         {
//             SampleCurrentPose();
//             sampleTimer = 0f;
//             UpdateUI(); // update sample count live
//         }
//     }

//     void StartRecording()
//     {
//         if (mocopiRecorder == null)
//         {
//             Debug.LogWarning("[RuntimePoseRecorderUI] Cannot start recording: mocopiRecorder is null.");
//             return;
//         }

//         // Max saved poses check
//         if (maxSavedPoses > 0 && mocopiRecorder.GetRecordedPoseCount() >= maxSavedPoses)
//         {
//             Debug.LogWarning($"[RuntimePoseRecorderUI] Max saved poses reached ({maxSavedPoses}). Delete some poses first.");
//             return;
//         }

//         isRecording = true;
//         samples.Clear();
//         sampleTimer = 0f;
//         SampleCurrentPose(); // immediate sample
//         Debug.Log("[RuntimePoseRecorderUI] Recording STARTED.");
//         UpdateUI();
//     }

//     void StopRecordingAndSave()
//     {
//         if (!isRecording) return;
//         isRecording = false;

//         if (samples.Count == 0)
//         {
//             Debug.LogWarning("[RuntimePoseRecorderUI] No samples recorded.");
//             UpdateUI();
//             return;
//         }

//         RecordedPose pose = AverageSamplesToPose();
//         pose.poseName = $"Pose_{DateTime.Now:yyyyMMdd_HHmmss}";

//         // Save through MocopiPoseRecorder API
//         mocopiRecorder.SavePoseData(pose);

//         Debug.Log($"[RuntimePoseRecorderUI] Recording STOPPED. Saved pose '{pose.poseName}' to {mocopiRecorder.GetSavePath()}");

//         UpdateUI();
//     }

//     void SampleCurrentPose()
//     {
//         var boneMap = mocopiRecorder.GetBoneMap();
//         var sample = new Dictionary<string, Quaternion>();
//         foreach (var kv in boneMap)
//         {
//             var t = kv.Value;
//             if (t == null) continue;
//             sample[kv.Key] = t.localRotation;
//         }
//         samples.Add(sample);
//     }

//     RecordedPose AverageSamplesToPose()
//     {
//         var boneMap = mocopiRecorder.GetBoneMap();
//         RecordedPose result = new RecordedPose();
//         result.poseName = "TEMP";

//         foreach (var kv in boneMap)
//         {
//             string boneName = kv.Key;
//             float sx = 0f, sy = 0f, sz = 0f, sw = 0f;
//             int count = 0;
//             foreach (var s in samples)
//             {
//                 if (!s.ContainsKey(boneName)) continue;
//                 var q = s[boneName];
//                 sx += q.x; sy += q.y; sz += q.z; sw += q.w;
//                 count++;
//             }
//             if (count == 0) continue;
//             var avg = new Quaternion(sx / count, sy / count, sz / count, sw / count);
//             avg = Normalize(avg);
//             BoneData bd = new BoneData();
//             bd.boneName = boneName;
//             bd.SetRotation(avg);
//             result.bones.Add(bd);
//         }

//         return result;
//     }

//     Quaternion Normalize(Quaternion q)
//     {
//         float m = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
//         if (m > 1e-6f) return new Quaternion(q.x / m, q.y / m, q.z / m, q.w / m);
//         return Quaternion.identity;
//     }

//     void UpdateUI()
//     {
//         if (recordingText != null)
//         {
//             recordingText.text = isRecording ? "<color=#ff4444><b>RECORDING</b></color>\nPress R to stop" : "<color=#88ff88>Not recording\nPress R to record</color>";
//         }

//         if (savedCountText != null && mocopiRecorder != null)
//         {
//             savedCountText.text = $"Saved poses: {mocopiRecorder.GetRecordedPoseCount()}";
//         }

//         if (sampleCountText != null)
//         {
//             sampleCountText.text = isRecording ? $"Samples: {samples.Count}" : "";
//         }
//     }
// }
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class RuntimePoseRecorderUI : MonoBehaviour
{
    [Header("References")]
    public MocopiPoseRecorder mocopiRecorder;
    public TextMeshProUGUI recordingText;
    public TextMeshProUGUI savedCountText;
    public TextMeshProUGUI sampleCountText;
    public TextMeshProUGUI instructionText;

    [Header("Controls")]
    public KeyCode recordKey = KeyCode.R;

    [Header("Sampling")]
    public float sampleInterval = 0.1f;

    // Calibration pose order (DO NOT CHANGE names)
    readonly string[] calibrationPoses =
    {
        "Warrior1",
        "Warrior2",
        "Warrior3"
    };

    int currentPoseIndex = 0;
    bool isRecording = false;
    float sampleTimer = 0f;

    List<Dictionary<string, Quaternion>> samples =
        new List<Dictionary<string, Quaternion>>();

    void Start()
    {
        if (mocopiRecorder == null)
            Debug.LogWarning("[RuntimePoseRecorderUI] MocopiPoseRecorder not set!");

        UpdateUI();
    }

    void Update()
    {
        if (currentPoseIndex >= calibrationPoses.Length)
            return; // calibration complete

        if (Input.GetKeyDown(recordKey))
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecordingAndSave();
        }

        if (!isRecording) return;

        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            SampleCurrentPose();
            sampleTimer = 0f;
            UpdateUI();
        }
    }

    void StartRecording()
    {
        isRecording = true;
        samples.Clear();
        sampleTimer = 0f;

        SampleCurrentPose(); // immediate sample

        Debug.Log($"[Calibration] Recording {calibrationPoses[currentPoseIndex]}");
        UpdateUI();
    }

    void StopRecordingAndSave()
    {
        if (samples.Count == 0)
        {
            Debug.LogWarning("[Calibration] No samples recorded.");
            isRecording = false;
            UpdateUI();
            return;
        }

        RecordedPose pose = AverageSamplesToPose();
        pose.poseName = calibrationPoses[currentPoseIndex];

        // Save / overwrite deterministically
        mocopiRecorder.SavePoseData(pose);

        Debug.Log($"[Calibration] Saved pose '{pose.poseName}'");

        isRecording = false;
        currentPoseIndex++;

        UpdateUI();

        if (currentPoseIndex >= calibrationPoses.Length)
        {
            Debug.Log("✅ Calibration COMPLETE. Restart game to play.");
        }
    }

    void SampleCurrentPose()
    {
        var boneMap = mocopiRecorder.GetBoneMap();
        var sample = new Dictionary<string, Quaternion>();

        foreach (var kv in boneMap)
        {
            if (kv.Value == null) continue;
            sample[kv.Key] = kv.Value.localRotation;
        }

        samples.Add(sample);
    }

    RecordedPose AverageSamplesToPose()
    {
        var boneMap = mocopiRecorder.GetBoneMap();
        RecordedPose result = new RecordedPose();

        foreach (var kv in boneMap)
        {
            string boneName = kv.Key;
            float sx = 0, sy = 0, sz = 0, sw = 0;
            int count = 0;

            foreach (var s in samples)
            {
                if (!s.ContainsKey(boneName)) continue;
                Quaternion q = s[boneName];
                sx += q.x; sy += q.y; sz += q.z; sw += q.w;
                count++;
            }

            if (count == 0) continue;

            Quaternion avg = Normalize(new Quaternion(
                sx / count, sy / count, sz / count, sw / count));

            BoneData bd = new BoneData();
            bd.boneName = boneName;
            bd.SetRotation(avg);
            result.bones.Add(bd);
        }

        return result;
    }

    Quaternion Normalize(Quaternion q)
    {
        float m = Mathf.Sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);
        if (m > 1e-6f)
            return new Quaternion(q.x/m, q.y/m, q.z/m, q.w/m);
        return Quaternion.identity;
    }

    void UpdateUI()
    {
        if (currentPoseIndex >= calibrationPoses.Length)
        {
            recordingText.text = "<color=#88ff88><b>CALIBRATION COMPLETE</b></color>";
            instructionText.text = "Restart the game to begin";
            sampleCountText.text = "";
            savedCountText.text = $"Saved poses: {mocopiRecorder.GetRecordedPoseCount()}";
            return;
        }

        string poseName = calibrationPoses[currentPoseIndex];

        instructionText.text =
            $"Stand in:\n<b>{poseName}</b>";

        recordingText.text = isRecording
            ? "<color=#ff4444><b>RECORDING</b></color>\nPress R to save"
            : "<color=#ffffff>Press R to record</color>";

        sampleCountText.text = isRecording
            ? $"Samples: {samples.Count}"
            : "";

        savedCountText.text =
            $"Saved poses: {mocopiRecorder.GetRecordedPoseCount()}";
    }
}
