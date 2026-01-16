using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoneRot {
    public string name;
    public float[] q; // x,y,z,w
    public BoneRot() { q = new float[4]; }
}

[Serializable]
public class PoseData {
    public string name;
    public string timestamp;
    public float[] rootPos; // optional; we record root world pos at capture time
    public List<BoneRot> bones = new List<BoneRot>();
}
