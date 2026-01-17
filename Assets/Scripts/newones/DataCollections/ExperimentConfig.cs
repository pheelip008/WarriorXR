using UnityEngine;

public class ExperimentConfig : MonoBehaviour
{
    [Header("Experiment Info")]
    public string participantID = "P01";
    public string condition = "Adaptive"; // Static or Adaptive
    public int trialNumber = 1;

    public static ExperimentConfig Instance;

    void Awake()
    {
        Instance = this;
    }
}
