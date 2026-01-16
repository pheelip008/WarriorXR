using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class BoneWeight {
    public string boneName;
    public float weight = 1f;
}

public class PoseMatcher : MonoBehaviour {
    public List<Transform> bonesToUse;      // assign Transforms (same skeleton)
    public List<BoneWeight> boneWeights;    // specify bone names and weights (must match transform names)
    public float scoreThreshold = 0.75f;    // similarity threshold (0..1)
    public float holdTimeRequired = 0.9f;   // seconds above threshold to trigger
    public bool ignoreRootYaw = true;       // reduce facing-direction sensitivity
    public string loadFileName;             // name of saved json (auto load if set)
    public float slerpSmooth = 0.7f;        // smoothing factor (0..0.95) ; higher = smoother but more lag

    private PoseData targetPose;
    private float holdTimer = 0f;
    public Action OnPoseSucceeded;

    // smoothing buffer
    private Dictionary<string, Quaternion> prevLocalRot = new Dictionary<string, Quaternion>();

    void Start() {
        if (!string.IsNullOrEmpty(loadFileName)) LoadPoseFromFile(loadFileName);
    }

    public void LoadPoseFromFile(string filename) {
        string path = Path.Combine(Application.persistentDataPath, "RecordedPoses", filename);
        if (!File.Exists(path)) {
            Debug.LogError("[PoseMatcher] Pose file not found: " + path);
            return;
        }
        var json = File.ReadAllText(path);
        targetPose = JsonUtility.FromJson<PoseData>(json);
        Debug.Log("[PoseMatcher] Loaded pose: " + targetPose.name);
        prevLocalRot.Clear();
        foreach (var t in bonesToUse) if (t!=null) prevLocalRot[t.name] = t.localRotation;
    }

    // Enhanced similarity (rotation-based, with per-joint tolerance & smoothing)
    public float ComputeSimilarityEnhanced() {
        if (targetPose == null) return 0f;

        var targetMap = new Dictionary<string, Quaternion>();
        foreach (var br in targetPose.bones) targetMap[br.name] = new Quaternion(br.q[0], br.q[1], br.q[2], br.q[3]);

        // example tolerance defaults
        Dictionary<string, (float deadzone, float maxAngle, float sigma)> jointTolerance = new Dictionary<string, (float, float, float)>() {
            {"Hips",(4f,60f,18f)}, {"Spine",(6f,50f,16f)}, {"Chest",(6f,45f,15f)},
            {"Neck",(8f,60f,20f)}, {"Head",(8f,60f,22f)},
            {"LeftUpperArm",(8f,50f,18f)}, {"RightUpperArm",(8f,50f,18f)},
            {"LeftLowerArm",(10f,60f,22f)}, {"RightLowerArm",(10f,60f,22f)},
            {"LeftUpperLeg",(6f,55f,18f)}, {"RightUpperLeg",(6f,55f,18f)},
            {"LeftLowerLeg",(8f,60f,20f)}, {"RightLowerLeg",(8f,60f,20f)}
        };

        // compute root yaw neutralization (if desired)
        Quaternion rootYawInv = Quaternion.identity;
        if (ignoreRootYaw && bonesToUse.Count>0) {
            // assume first bonesToUse is the root/hips — set it that way in Inspector
            Transform root = bonesToUse[0];
            if (root != null) {
                Vector3 fwd = root.forward; fwd.y = 0f;
                if (fwd.sqrMagnitude > 0.0001f) rootYawInv = Quaternion.Inverse(Quaternion.LookRotation(fwd));
            }
        }

        float acc = 0f;
        float wsum = 0f;

        foreach (var bw in boneWeights) {
            Transform bone = bonesToUse.Find(t => t != null && t.name == bw.boneName);
            if (bone == null) continue;
            if (!targetMap.ContainsKey(bw.boneName)) continue;

            Quaternion qCur = bone.localRotation;
            if (slerpSmooth > 0f) {
                Quaternion prev = prevLocalRot.ContainsKey(bone.name) ? prevLocalRot[bone.name] : qCur;
                qCur = Quaternion.Slerp(prev, qCur, 1f - slerpSmooth);
                prevLocalRot[bone.name] = qCur;
            }

            Quaternion qTarget = targetMap[bw.boneName];

            if (ignoreRootYaw) { qCur = rootYawInv * qCur; qTarget = rootYawInv * qTarget; }

            float angle = Quaternion.Angle(qTarget, qCur);

            float deadzone = 6f, maxAngle = 60f, sigma = 18f;
            if (jointTolerance.ContainsKey(bw.boneName)) {
                var t = jointTolerance[bw.boneName];
                deadzone = t.deadzone; maxAngle = t.maxAngle; sigma = t.sigma;
            }

            float simRot;
            if (angle <= deadzone) simRot = 1f;
            else if (angle >= maxAngle) simRot = 0f;
            else simRot = Mathf.Exp(-(angle*angle)/(2f*sigma*sigma)); // gaussian falloff

            // bone similarity (rotation-only here)
            float boneSim = simRot;

            acc += boneSim * bw.weight;
            wsum += bw.weight;
        }

        if (wsum <= 0f) return 0f;
        return Mathf.Clamp01(acc / wsum);
    }

    // simple wrapper for old code compatibility
    public float ComputeSimilarity() {
        return ComputeSimilarityEnhanced();
    }

    void Update() {
        float sim = ComputeSimilarity();
        if (sim >= scoreThreshold) {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdTimeRequired) {
                OnPoseSucceeded?.Invoke();
                holdTimer = 0f;
            }
        } else {
            holdTimer = 0f;
        }
    }
}
