using UnityEngine;

public class DebugStatsOverlay : MonoBehaviour
{
    [Header("--- LIVE DATA VIEWER ---")]
    [Tooltip("Current Difficulty Level (from DifficultyManager)")]
    public int currentLevel;

    [Tooltip("How fast the maze is spinning right now")]
    public float currentRotationSpeed;

    [Tooltip("The calculated height drop required to bend")]
    public float currentThreshold;

    [Tooltip("Multiplier currently applied to difficulty")]
    public float difficultyMultiplier;

    [Header("--- STEERING DEBUG ---")]
    public float lateralDifference;
    public string steeringDirection;

    // References to the scripts we are watching
    private HeadBendZoneController_Part2 headScript;
    private DifficultyManager diffScript;

    void Update()
    {
        // 1. Find scripts if we lost them (e.g., after scene load)
        if (headScript == null) 
            headScript = HeadBendZoneController_Part2.Instance;
            
        if (diffScript == null) 
            diffScript = DifficultyManager.Instance;

        // 2. READ VALUES (Only if scripts exist)
        if (headScript != null)
        {
            currentThreshold = headScript.bendHeightThreshold;
            difficultyMultiplier = headScript.difficultyMultiplier;
            lateralDifference = headScript.currentLateralDiff;
            steeringDirection = headScript.steeringDebug;

            // Get rotation speed from the plane controller inside the head script
            if (headScript.planeController != null)
            {
                currentRotationSpeed = headScript.planeController.rotationSpeed;
            }
        }

        if (diffScript != null)
        {
            currentLevel = diffScript.currentLevel;
        }
    }
}