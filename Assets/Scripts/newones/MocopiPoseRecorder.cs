// using UnityEngine;
// using System.Collections.Generic;
// using System.IO;

// [System.Serializable]
// public class RecordedPose
// {
//     public string poseName;
//     public List<BoneData> bones = new List<BoneData>();
// }

// [System.Serializable]
// public class BoneData
// {
//     public string boneName;
//     // Store LOCAL rotation (relative to parent) - this is orientation-independent!
//     public float x, y, z, w;

//     public Quaternion GetRotation()
//     {
//         return new Quaternion(x, y, z, w);
//     }

//     public void SetRotation(Quaternion rot)
//     {
//         x = rot.x;
//         y = rot.y;
//         z = rot.z;
//         w = rot.w;
//     }
// }

// public class MocopiPoseRecorder : MonoBehaviour
// {
//     [Header("Reference Bones - Assign from Mocopi")]
//     public Transform root;
//     public Transform torso;
//     public Transform leftThigh;
//     public Transform rightThigh;
//     public Transform leftUpperArm;
//     public Transform rightUpperArm;
//     public Transform leftForearm;
//     public Transform rightForearm;
//     public Transform head;

//     [Header("Settings")]
//     public float similarityThreshold = 0.85f; // 85% match required
//     public bool showDebugLogs = true;

//     [Header("Save/Load")]
//     public string saveFileName = "YogaPoses.json";

//     private Dictionary<string, Transform> boneMap;
//     private Dictionary<string, RecordedPose> loadedPoses = new Dictionary<string, RecordedPose>();
//     private string savePath;

//     void Awake()
//     {
//         savePath = Path.Combine(Application.persistentDataPath, saveFileName);
//         SetupBoneMap();
//         LoadAllPoses();
//     }

//     void SetupBoneMap()
//     {
//         boneMap = new Dictionary<string, Transform>
//         {
//             { "root", root },
//             { "torso", torso },
//             { "leftThigh", leftThigh },
//             { "rightThigh", rightThigh },
//             { "leftUpperArm", leftUpperArm },
//             { "rightUpperArm", rightUpperArm },
//             { "leftForearm", leftForearm },
//             { "rightForearm", rightForearm },
//             { "head", head }
//         };
//     }

//     // ═══════════════════════════════════════════════════════
//     // RECORDING A POSE (stores LOCAL rotations)
//     // ═══════════════════════════════════════════════════════
//     public void RecordAndSavePose(string poseName)
//     {
//         RecordedPose pose = new RecordedPose();
//         pose.poseName = poseName;

//         // Store LOCAL rotations (relative to parent bone)
//         // These are orientation-independent!
//         foreach (var bone in boneMap)
//         {
//             if (bone.Value != null)
//             {
//                 BoneData boneData = new BoneData();
//                 boneData.boneName = bone.Key;
//                 boneData.SetRotation(bone.Value.localRotation); // LOCAL = relative to parent!
//                 pose.bones.Add(boneData);
//             }
//         }

//         // Save to dictionary
//         loadedPoses[poseName] = pose;

//         // Save to disk
//         SaveAllPoses();

//         Debug.Log($"✅ Recorded & Saved: {poseName} with {pose.bones.Count} bones");
//         Debug.Log($"💾 Saved to: {savePath}");
//     }

//     // ═══════════════════════════════════════════════════════
//     // SAVE TO DISK (JSON)
//     // ═══════════════════════════════════════════════════════
//     void SaveAllPoses()
//     {
//         PoseCollection collection = new PoseCollection();
//         collection.poses = new List<RecordedPose>(loadedPoses.Values);

//         string json = JsonUtility.ToJson(collection, true);
//         File.WriteAllText(savePath, json);

//         Debug.Log($"💾 All poses saved to: {savePath}");
//     }

//     // ═══════════════════════════════════════════════════════
//     // LOAD FROM DISK (JSON)
//     // ═══════════════════════════════════════════════════════
//     void LoadAllPoses()
//     {
//         if (!File.Exists(savePath))
//         {
//             Debug.LogWarning($"⚠️ No saved poses found at: {savePath}");
//             Debug.LogWarning("📹 You need to record poses first!");
//             return;
//         }

//         string json = File.ReadAllText(savePath);
//         PoseCollection collection = JsonUtility.FromJson<PoseCollection>(json);

//         loadedPoses.Clear();
//         foreach (var pose in collection.poses)
//         {
//             loadedPoses[pose.poseName] = pose;
//         }

//         Debug.Log($"✅ Loaded {loadedPoses.Count} poses from: {savePath}");
//         foreach (var poseName in loadedPoses.Keys)
//         {
//             Debug.Log($"   - {poseName}");
//         }
//     }
//     // Return a list of all saved pose names (sorted for determinism)
// public List<string> GetAllSavedPoseNames()
// {
//     if (loadedPoses == null) return new List<string>();
//     var names = new List<string>(loadedPoses.Keys);
//     names.Sort();
//     return names;
// }

//     // ═══════════════════════════════════════════════════════
//     // COMPARE CURRENT POSE TO SAVED POSE
//     // ═══════════════════════════════════════════════════════
//     public float CompareToPose(string poseName, bool logDetails = false)
//     {
//         if (!loadedPoses.ContainsKey(poseName))
//         {
//             Debug.LogError($"❌ Pose '{poseName}' not found! Have you recorded it?");
//             return 0f;
//         }

//         RecordedPose targetPose = loadedPoses[poseName];

//         float totalSimilarity = 0f;
//         int boneCount = 0;
//         Dictionary<string, float> boneSimilarities = new Dictionary<string, float>();

//         foreach (var boneData in targetPose.bones)
//         {
//             if (!boneMap.ContainsKey(boneData.boneName)) continue;
//             Transform currentBone = boneMap[boneData.boneName];
//             if (currentBone == null) continue;

//             // Compare LOCAL rotations
//             Quaternion currentRotation = currentBone.localRotation;
//             Quaternion targetRotation = boneData.GetRotation();

//             // Calculate angular difference
//             float angleDiff = Quaternion.Angle(currentRotation, targetRotation);

//             // Convert to similarity (0° = 100%, 180° = 0%)
//             float similarity = 1f - (angleDiff / 180f);

//             boneSimilarities[boneData.boneName] = similarity;
//             totalSimilarity += similarity;
//             boneCount++;
//         }

//         float averageSimilarity = boneCount > 0 ? totalSimilarity / boneCount : 0f;

//         // Debug logging
//         if (logDetails && showDebugLogs)
//         {
//             Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
//             Debug.Log($"  📊 POSE COMPARISON: {poseName}");
//             Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
//             Debug.Log($"  Overall: {averageSimilarity:P1} (need {similarityThreshold:P1})");
//             Debug.Log($"");

//             foreach (var bone in boneSimilarities)
//             {
//                 string status = bone.Value > 0.85f ? "✅" : bone.Value > 0.7f ? "⚠️" : "❌";
//                 Debug.Log($"    {status} {bone.Key}: {bone.Value:P1}");
//             }

//             string result = averageSimilarity >= similarityThreshold ? "✅ MATCHED!" : "❌ Not matched";
//             Debug.Log($"\n  {result}");
//             Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
//         }

//         return averageSimilarity;
//     }

//     // ═══════════════════════════════════════════════════════
//     // PUBLIC API
//     // ═══════════════════════════════════════════════════════
//     public bool IsPoseRecorded(string poseName)
//     {
//         return loadedPoses.ContainsKey(poseName);
//     }

//     public bool IsCurrentPoseMatching(string poseName)
//     {
//         float similarity = CompareToPose(poseName, false);
//         return similarity >= similarityThreshold;
//     }

//     public int GetRecordedPoseCount()
//     {
//         return loadedPoses.Count;
//     }
    

//     public void DeleteAllSavedPoses()
//     {
//         if (File.Exists(savePath))
//         {
//             File.Delete(savePath);
//             loadedPoses.Clear();
//             Debug.Log("🗑️ All saved poses deleted!");
//         }
//     }
//     // PUBLIC API additions for RuntimePoseRecorder

// // Save a prepared RecordedPose (called by RuntimePoseRecorder)
// public void SavePoseData(RecordedPose pose)
// {
//     if (pose == null || string.IsNullOrEmpty(pose.poseName))
//     {
//         Debug.LogError("SavePoseData: invalid pose or missing poseName");
//         return;
//     }

//     loadedPoses[pose.poseName] = pose;
//     SaveAllPoses(); // uses your existing save method
// }

// // Expose the bone map for sampling
// public Dictionary<string, Transform> GetBoneMap()
// {
//     if (boneMap == null) SetupBoneMap();
//     return boneMap;
// }

// // Expose save path for debug logs
// public string GetSavePath()
// {
//     return savePath;
// }

// }

// [System.Serializable]
// public class PoseCollection
// {
//     public List<RecordedPose> poses = new List<RecordedPose>();
// }
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class RecordedPose
{
    public string poseName;
    public List<BoneData> bones = new List<BoneData>();
}

[Serializable]
public class BoneData
{
    public string boneName;
    public float x, y, z, w;

    public Quaternion GetRotation() => new Quaternion(x, y, z, w);

    public void SetRotation(Quaternion rot)
    {
        x = rot.x; y = rot.y; z = rot.z; w = rot.w;
    }
}

[Serializable]
public class PoseCollection
{
    public List<RecordedPose> poses = new List<RecordedPose>();
}

/// <summary>
/// MocopiPoseRecorder - full implementation with helpers
/// </summary>
public class MocopiPoseRecorder : MonoBehaviour
{
    [Header("Reference Bones - Assign from Mocopi")]
    public Transform root;
    public Transform torso;
    public Transform leftThigh;
    public Transform rightThigh;
    public Transform leftUpperArm;
    public Transform rightUpperArm;
    public Transform leftForearm;
    public Transform rightForearm;
    public Transform head;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float similarityThreshold = 0.85f; // 85% default
    public bool showDebugLogs = true;

    [Header("Save/Load")]
    public string saveFileName = "YogaPoses.json";

    // internals
    Dictionary<string, Transform> boneMap;
    Dictionary<string, RecordedPose> loadedPoses = new Dictionary<string, RecordedPose>();
    string savePath;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        SetupBoneMap();
        LoadAllPoses();
    }

    void SetupBoneMap()
    {
        boneMap = new Dictionary<string, Transform>
        {
            { "root", root },
            { "torso", torso },
            { "leftThigh", leftThigh },
            { "rightThigh", rightThigh },
            { "leftUpperArm", leftUpperArm },
            { "rightUpperArm", rightUpperArm },
            { "leftForearm", leftForearm },
            { "rightForearm", rightForearm },
            { "head", head }
        };
    }
    public Quaternion GetBoneLocalRotation(string poseName, string boneName)
    {
        if (!loadedPoses.ContainsKey(poseName))
            return Quaternion.identity;

        var pose = loadedPoses[poseName];
        foreach (var bone in pose.bones)
        {
            if (bone.boneName == boneName)
                return bone.GetRotation();
        }

        return Quaternion.identity;
    }
    public Transform GetBoneTransform(string boneName)
    {
        if (boneMap != null && boneMap.ContainsKey(boneName))
            return boneMap[boneName];

        return null;
    }



    #region Recording / saving

    public void RecordAndSavePose(string poseName)
    {
        if (string.IsNullOrEmpty(poseName)) poseName = $"Pose_{DateTime.Now:yyyyMMdd_HHmmss}";

        RecordedPose pose = new RecordedPose { poseName = poseName };

        foreach (var kv in boneMap)
        {
            if (kv.Value == null) continue;
            BoneData bd = new BoneData();
            bd.boneName = kv.Key;
            bd.SetRotation(kv.Value.localRotation);
            pose.bones.Add(bd);
        }

        loadedPoses[pose.poseName] = pose;
        SaveAllPoses();

        if (showDebugLogs) Debug.Log($"MocopiPoseRecorder: Recorded & saved pose '{pose.poseName}' to {savePath}");
    }

    public void SavePoseData(RecordedPose pose)
    {
        if (pose == null || string.IsNullOrEmpty(pose.poseName))
        {
            Debug.LogError("MocopiPoseRecorder.SavePoseData: invalid pose or missing poseName");
            return;
        }

        loadedPoses[pose.poseName] = pose;
        SaveAllPoses();
        if (showDebugLogs) Debug.Log($"MocopiPoseRecorder: SavePoseData wrote '{pose.poseName}' to {savePath}");
    }

    void SaveAllPoses()
    {
        PoseCollection col = new PoseCollection();
        col.poses = new List<RecordedPose>(loadedPoses.Values);
        string json = JsonUtility.ToJson(col, true);

        try
        {
            File.WriteAllText(savePath, json);
            if (showDebugLogs) Debug.Log($"MocopiPoseRecorder: Saved {col.poses.Count} poses to: {savePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"MocopiPoseRecorder: Failed saving poses: {ex}");
        }
    }

    public void LoadAllPoses()
    {
        loadedPoses.Clear();

        if (!File.Exists(savePath))
        {
            if (showDebugLogs) Debug.LogWarning($"MocopiPoseRecorder: No saved poses at {savePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            PoseCollection col = JsonUtility.FromJson<PoseCollection>(json);
            if (col != null && col.poses != null)
            {
                foreach (var p in col.poses)
                {
                    if (string.IsNullOrEmpty(p.poseName)) continue;
                    loadedPoses[p.poseName] = p;
                }
                if (showDebugLogs) Debug.Log($"MocopiPoseRecorder: Loaded {loadedPoses.Count} poses from {savePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"MocopiPoseRecorder: Failed loading poses: {ex}");
        }
    }

    #endregion

    #region Public API

    public Dictionary<string, Transform> GetBoneMap()
    {
        if (boneMap == null) SetupBoneMap();
        return boneMap;
    }

    public string GetSavePath() => savePath;

    public List<string> GetAllSavedPoseNames()
    {
        var names = new List<string>(loadedPoses.Keys);
        names.Sort();
        return names;
    }

    public int GetRecordedPoseCount() => loadedPoses.Count;

    public bool DeletePose(string poseName)
    {
        if (string.IsNullOrEmpty(poseName)) return false;
        if (!loadedPoses.ContainsKey(poseName)) return false;
        loadedPoses.Remove(poseName);
        SaveAllPoses();
        if (showDebugLogs) Debug.Log($"MocopiPoseRecorder: Deleted pose '{poseName}' and saved file.");
        return true;
    }

    public void DeleteAllSavedPoses()
    {
        loadedPoses.Clear();
        try
        {
            if (File.Exists(savePath)) File.Delete(savePath);
            if (showDebugLogs) Debug.Log("MocopiPoseRecorder: All saved poses deleted.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"MocopiPoseRecorder: Failed deleting poses file: {ex}");
        }
    }

    [ContextMenu("Reload Saved Poses")]
    public void ReloadSavedPoses()
    {
        LoadAllPoses();
        if (showDebugLogs) Debug.Log($"MocopiPoseRecorder: Reloaded {GetRecordedPoseCount()} poses from {savePath}");
    }

    #endregion

    #region Comparison

    /// <summary>
    /// Compare current bone local rotations to a saved pose.
    /// Returns average similarity 0..1 (1.0 = perfect)
    /// Logs a warning (not error) if the named pose is missing.
    /// </summary>
    public float CompareToPose(string poseName, bool logDetails = false)
    {
        if (!loadedPoses.ContainsKey(poseName))
        {
            Debug.LogWarning($"Pose '{poseName}' not found! Have you recorded it?");
            return 0f;
        }

        RecordedPose targetPose = loadedPoses[poseName];

        float totalSimilarity = 0f;
        int boneCount = 0;
        Dictionary<string, float> boneSimilarities = new Dictionary<string, float>();

        foreach (var boneData in targetPose.bones)
        {
            if (!boneMap.ContainsKey(boneData.boneName)) continue;
            Transform currentBone = boneMap[boneData.boneName];
            if (currentBone == null) continue;

            Quaternion currentRotation = currentBone.localRotation;
            Quaternion targetRotation = boneData.GetRotation();

            float angleDiff = Quaternion.Angle(currentRotation, targetRotation);
            float similarity = 1f - (angleDiff / 180f);

            boneSimilarities[boneData.boneName] = similarity;
            totalSimilarity += similarity;
            boneCount++;
        }

        float averageSimilarity = boneCount > 0 ? (totalSimilarity / boneCount) : 0f;

        if (logDetails && showDebugLogs)
        {
            Debug.Log($"Pose Comparison: {poseName} -> {averageSimilarity:P1}");
            foreach (var kv in boneSimilarities) Debug.Log($"  {kv.Key}: {kv.Value:P2}");
        }

        return averageSimilarity;
    }

    /// <summary>
    /// Convenience: returns true if the current pose matches the named pose at or above threshold.
    /// </summary>
    public bool IsCurrentPoseMatching(string poseName)
    {
        float sim = CompareToPose(poseName, false);
        return sim >= similarityThreshold;
    }

    public bool IsPoseRecorded(string poseName)
    {
        return loadedPoses.ContainsKey(poseName);
    }

    #endregion
}
