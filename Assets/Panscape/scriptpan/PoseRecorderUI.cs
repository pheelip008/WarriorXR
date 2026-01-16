using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PoseRecorderUI : MonoBehaviour {
    public Transform avatarRoot;                  // root transform (e.g., Hips)
    public List<Transform> bonesToRecord;         // set in Inspector
    public Button recordButton;                   // optional UI Button
    public TMP_Text statusText;                       // optional UI text for status
    public KeyCode recordKey = KeyCode.R;
    public string saveFolder = "RecordedPoses";
    [HideInInspector] public string lastSavedFileName;

    void Start() {
        if (recordButton != null) recordButton.onClick.AddListener(OnRecordClicked);
        if (statusText != null) statusText.text = "Ready. Press R or Record.";
    }

    void Update() {
       // if (Input.GetKeyDown(recordKey)) OnRecordClicked();
    }

    public void OnRecordClicked() {
        try {
            var pose = CapturePose("basePose_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
            string path = SavePoseToFile(pose);
            lastSavedFileName = Path.GetFileName(path);
            if (statusText != null) statusText.text = "Saved: " + lastSavedFileName;
            Debug.Log("[PoseRecorderUI] Saved pose: " + path);
        } catch (Exception e) {
            Debug.LogError("[PoseRecorderUI] Save failed: " + e.Message);
            if (statusText != null) statusText.text = "Save failed: " + e.Message;
        }
    }

    PoseData CapturePose(string name) {
        var p = new PoseData();
        p.name = name;
        p.timestamp = DateTime.UtcNow.ToString("o");
        if (avatarRoot != null) p.rootPos = new float[] { avatarRoot.position.x, avatarRoot.position.y, avatarRoot.position.z };
        else p.rootPos = new float[] { 0f, 0f, 0f };

        foreach (var t in bonesToRecord) {
            if (t == null) continue;
            var br = new BoneRot();
            br.name = t.name;
            var q = t.localRotation;
            br.q[0] = q.x; br.q[1] = q.y; br.q[2] = q.z; br.q[3] = q.w;
            p.bones.Add(br);
        }
        return p;
    }

    string SavePoseToFile(PoseData p) {
        string folder = Path.Combine(Application.persistentDataPath, saveFolder);
        Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, p.name + ".json");
        string json = JsonUtility.ToJson(p, true);
        File.WriteAllText(file, json);
        return file;
    }
}
