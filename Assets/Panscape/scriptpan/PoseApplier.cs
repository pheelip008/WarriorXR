using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class PoseApplier : MonoBehaviour {
    public List<Transform> ghostBones;  // set in inspector
    public string lastLoadedFile;

    public void LoadAndApply(string filename) {
        string path = Path.Combine(Application.persistentDataPath, "RecordedPoses", filename);
        if (!File.Exists(path)) {
            Debug.LogError("[PoseApplier] File not found: " + path);
            return;
        }
        var json = File.ReadAllText(path);
        var p = JsonUtility.FromJson<PoseData>(json);
        ApplyPoseToGhost(p);
        lastLoadedFile = filename;
    }

    public void ApplyPoseToGhost(PoseData p) {
        var map = new Dictionary<string, Quaternion>();
        foreach (var br in p.bones) map[br.name] = new Quaternion(br.q[0], br.q[1], br.q[2], br.q[3]);

        foreach (var t in ghostBones) {
            if (t == null) continue;
            if (map.TryGetValue(t.name, out var q)) t.localRotation = q;
        }
    }
}
