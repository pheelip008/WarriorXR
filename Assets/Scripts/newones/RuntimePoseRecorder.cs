using System;
using System.Collections.Generic;
using UnityEngine;

/// Press R to start/stop recording. Samples LOCAL rotations every sampleInterval seconds.
public class RuntimePoseRecorder : MonoBehaviour
{
    [Header("References")]
    public MocopiPoseRecorder mocopiRecorder; // assign your existing mocopi recorder

    [Header("Recording")]
    public float sampleInterval = 0.1f; // seconds between samples (10 Hz)
    public KeyCode recordKey = KeyCode.R;
    public bool autoStartRecording = false;

    bool isRecording;
    float sampleTimer;
    List<Dictionary<string, Quaternion>> samples = new List<Dictionary<string, Quaternion>>();

    void Start()
    {
        if (mocopiRecorder == null) Debug.LogError("[RuntimePoseRecorder] mocopiRecorder not set!");
        if (autoStartRecording) StartRecording();
    }

    void Update()
    {
        if (Input.GetKeyDown(recordKey))
        {
            if (!isRecording) StartRecording();
            else StopRecordingAndSave();
        }

        if (!isRecording) return;

        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            SampleCurrentPose();
            sampleTimer = 0f;
        }
    }

    void StartRecording()
    {
        if (mocopiRecorder == null) return;
        isRecording = true;
        samples.Clear();
        sampleTimer = 0f;
        SampleCurrentPose(); // immediate first sample
        Debug.Log("[RuntimePoseRecorder] Recording started.");
    }

    void StopRecordingAndSave()
    {
        if (!isRecording || mocopiRecorder == null) return;
        isRecording = false;

        if (samples.Count == 0)
        {
            Debug.LogWarning("[RuntimePoseRecorder] No samples recorded.");
            return;
        }

        RecordedPose pose = AverageSamplesToPose();
        string poseName = $"Pose_{DateTime.Now:yyyyMMdd_HHmmss}";
        pose.poseName = poseName;
        mocopiRecorder.SavePoseData(pose);

        Debug.Log($"[RuntimePoseRecorder] Recording stopped. Saved pose: {poseName} at {mocopiRecorder.GetSavePath()}");
    }

    void SampleCurrentPose()
    {
        var sample = new Dictionary<string, Quaternion>();
        var boneMap = mocopiRecorder.GetBoneMap();

        foreach (var kv in boneMap)
        {
            string boneName = kv.Key;
            Transform t = kv.Value;
            if (t == null) continue;
            sample[boneName] = t.localRotation;
        }

        samples.Add(sample);
    }

    RecordedPose AverageSamplesToPose()
    {
        var boneMap = mocopiRecorder.GetBoneMap();
        RecordedPose result = new RecordedPose();
        result.poseName = "TEMP";

        foreach (var kv in boneMap)
        {
            string boneName = kv.Key;
            float sx=0f, sy=0f, sz=0f, sw=0f;
            int count=0;
            foreach (var s in samples)
            {
                if (!s.ContainsKey(boneName)) continue;
                Quaternion q = s[boneName];
                sx += q.x; sy += q.y; sz += q.z; sw += q.w;
                count++;
            }
            if (count == 0) continue;
            Quaternion avg = new Quaternion(sx/count, sy/count, sz/count, sw/count);
            avg = NormalizeQuaternion(avg);
            BoneData bd = new BoneData();
            bd.boneName = boneName;
            bd.SetRotation(avg);
            result.bones.Add(bd);
        }

        return result;
    }

    Quaternion NormalizeQuaternion(Quaternion q)
    {
        float m = Mathf.Sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);
        if (m > 1e-6f) return new Quaternion(q.x/m, q.y/m, q.z/m, q.w/m);
        return Quaternion.identity;
    }
}
