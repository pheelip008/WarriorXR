// using UnityEngine;

// public class DifficultyManager : MonoBehaviour
// {
//     public static DifficultyManager Instance;

//     [Header("Current State")]
//     public int currentLevel = 1; // Starts at 1
//     public int maxLevel = 5;

//     [Header("Difficulty Parameters")]
//     // 0 = Hardest (0.2), 1 = Medium (3), 2 = Easy (8) ... Let's map levels to values
//     public float[] rotationSpeeds = { 8.0f, 6.0f, 4.0f, 3.0f, 0.2f }; // Level 1 to 5
    
//     // Threshold Multipliers relative to Baseline
//     // Level 1 = 1.0x (Recorded), Level 5 = 1.2x (20% deeper/wider)
//     public float[] thresholdMultipliers = { 1.0f, 1.05f, 1.1f, 1.15f, 1.2f };

//     [Header("Base Times (The time it takes to beat scene on Easiest)")]
//     public float scene1BaseTime = 30f;
//     public float scene2BaseTime = 45f;
//     public float scene3BaseTime = 25f;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject); // Keeps this script alive across scenes
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     /// <summary>
//     /// Calculates the Ideal Time for the current difficulty and scene.
//     /// </summary>
//     public float GetIdealTime(string sceneName)
//     {
//         float baseTime = 30f; // Default

//         if (sceneName.Contains("1")) baseTime = scene1BaseTime;
//         else if (sceneName.Contains("2")) baseTime = scene2BaseTime;
//         else if (sceneName.Contains("3")) baseTime = scene3BaseTime;

//         // Apply Difficulty Penalties
//         // Harder Rotation (Level 5) adds ~30% time?
//         float rotationPenalty = 1.0f + (currentLevel * 0.05f); 
        
//         // Deeper Squat (Level 5) adds ~20% time?
//         float squatPenalty = 1.0f + (currentLevel * 0.04f);

//         return baseTime * rotationPenalty * squatPenalty;
//     }

//     /// <summary>
//     /// Updates difficulty based on player performance.
//     /// </summary>
//     /// <param name="scoreRatio">IdealTime / PlayerTime</param>
//     public void AdjustDifficulty(float scoreRatio)
//     {
//         Debug.Log($"[Difficulty] Analyzing Performance. Score Ratio: {scoreRatio:F2}");

//         if (scoreRatio > 1.2f)
//         {
//             // Player Crushed it -> Increase Difficulty significantly
//             currentLevel++;
//             Debug.Log("Performance Excellent! Increasing Difficulty.");
//         }
//         else if (scoreRatio > 0.9f)
//         {
//             // Player did okay -> Increase difficulty slowly (Progressive Overload)
//             // Maybe we keep same level but next time we act stricter?
//             // For this research, let's bump it up to keep pushing them.
//             currentLevel++;
//             Debug.Log("Performance Good. Progressive Overload applied.");
//         }
//         else
//         {
//             // Player struggled (Score < 0.9) -> Keep same level or drop
//             // For research, usually we maintain to let them master it.
//             Debug.Log("Player struggled. Maintaining Level.");
//         }

//         // Clamp level
//         if (currentLevel > maxLevel) currentLevel = maxLevel;
//         if (currentLevel < 1) currentLevel = 1;
//     }

//     // Getters for other scripts to use
//     public float GetCurrentRotationSpeed()
//     {
//         return rotationSpeeds[currentLevel - 1];
//     }

//     public float GetCurrentThresholdMultiplier()
//     {
//         return thresholdMultipliers[currentLevel - 1];
//     }
// }

// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class DifficultyManager : MonoBehaviour
// {
//     public static DifficultyManager Instance;

//     [Header("Current Status")]
//     public int currentLevel = 1; // Starts at Level 1 (Easiest)
//     public int maxLevel = 5;

//     [Header("Level 1-5 Settings")]
//     // Rotation Speeds: Level 1 (Easy/Fast=8) -> Level 5 (Hard/Slow=0.2)
//     public float[] rotationSpeeds = { 8.0f, 6.0f, 4.0f, 3.0f, 0.2f }; 
    
//     // Threshold Multipliers: Level 1 (1.0x) -> Level 5 (1.2x deeper squat)
//     public float[] thresholdMultipliers = { 1.0f, 1.05f, 1.1f, 1.15f, 1.2f };

//     // Base Times: How long each scene takes on "Easy" mode
//     // You can adjust these based on your own testing
//     public float scene1BaseTime = 30f;
//     public float scene2BaseTime = 45f;
//     public float scene3BaseTime = 25f;

//     void Awake()
//     {
//         // Singleton Pattern: Ensure only one brain exists and it survives scene loads
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     /// <summary>
//     /// Returns the Rotation Speed for the current level.
//     /// </summary>
//     public float GetCurrentRotationSpeed()
//     {
//         // Safety check to prevent array index errors
//         int index = Mathf.Clamp(currentLevel - 1, 0, rotationSpeeds.Length - 1);
//         return rotationSpeeds[index];
//     }

//     /// <summary>
//     /// Returns the Squat Multiplier for the current level.
//     /// </summary>
//     public float GetCurrentThresholdMultiplier()
//     {
//         int index = Mathf.Clamp(currentLevel - 1, 0, thresholdMultipliers.Length - 1);
//         return thresholdMultipliers[index];
//     }

//     /// <summary>
//     /// Calculates Ideal Time dynamically based on Scene and Difficulty.
//     /// </summary>
//     public float GetIdealTime(string sceneName)
//     {
//         float baseTime = 30f; // Default

//         // Simple check to find which scene is active
//         // Make sure your scene names in Build Settings match these keywords
//         if (sceneName.Contains("1")) baseTime = scene1BaseTime;
//         else if (sceneName.Contains("2")) baseTime = scene2BaseTime;
//         else if (sceneName.Contains("3")) baseTime = scene3BaseTime;

//         // Add penalties: Harder levels take longer to complete
//         // Example: Level 5 adds ~30% more time allowance
//         float difficultyPenalty = 1.0f + (currentLevel * 0.06f); 

//         return baseTime * difficultyPenalty;
//     }

//     /// <summary>
//     /// The Core Algorithm: Decides if level goes UP, DOWN, or STAYS.
//     /// </summary>
//     public void AnalyzePerformance(float scoreRatio)
//     {
//         Debug.Log($"[DifficultyManager] Analyzing Score Ratio: {scoreRatio:F2}");

//         if (scoreRatio > 1.2f)
//         {
//             // Player was too fast (Game too easy) -> Increase Difficulty
//             currentLevel++;
//             Debug.Log("Result: Excellent! Level Up.");
//         }
//         else if (scoreRatio > 0.9f)
//         {
//             // Player is in the zone -> Progressive Overload (Increase slowly)
//             currentLevel++; 
//             Debug.Log("Result: Good Flow. Level Up (Progressive Overload).");
//         }
//         else if (scoreRatio < 0.6f)
//         {
//             // Player struggled significantly -> Decrease Difficulty
//             currentLevel--;
//             Debug.Log("Result: Struggled. Level Down.");
//         }
//         else
//         {
//             // Between 0.6 and 0.9 -> Maintain Level
//             Debug.Log("Result: Balanced. Maintain Level.");
//         }

//         // Clamp values
//         currentLevel = Mathf.Clamp(currentLevel, 1, maxLevel);
//     }
// }

using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    public enum DifficultyMode
    {
        Adaptive,   // Adjusts based on player skill (The "Novel" Algorithm)
        Linear      // Always gets harder (The "Control" Algorithm)
    }

    [Header("Experiment Settings")]
    [Tooltip("Choose 'Adaptive' for AI-adjustment, or 'Linear' for standard increasing difficulty.")]
    public DifficultyMode progressionMode = DifficultyMode.Adaptive;

    [Header("Current Status")]
    public int currentLevel = 1; // Starts at Level 1
    public int maxLevel = 5;

    [Header("Level 1-5 Settings")]
    // Rotation Speeds: Level 1 (Easy/Fast=8) -> Level 5 (Hard/Slow=0.2)
    public float[] rotationSpeeds = { 8.0f, 6.0f, 4.0f, 3.0f, 0.2f }; 
    
    // Threshold Multipliers: Level 1 (1.0x) -> Level 5 (1.2x deeper squat)
    public float[] thresholdMultipliers = { 1.0f, 1.05f, 1.1f, 1.15f, 1.2f };

    [Header("Base Times (For Adaptive Calc)")]
    public float scene1BaseTime = 30f;
    public float scene2BaseTime = 45f;
    public float scene3BaseTime = 25f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetCurrentRotationSpeed()
    {
        int index = Mathf.Clamp(currentLevel - 1, 0, rotationSpeeds.Length - 1);
        return rotationSpeeds[index];
    }

    public float GetCurrentThresholdMultiplier()
    {
        int index = Mathf.Clamp(currentLevel - 1, 0, thresholdMultipliers.Length - 1);
        return thresholdMultipliers[index];
    }

    public float GetIdealTime(string sceneName)
    {
        // Even in Linear mode, we need an "Ideal Time" to show the user a score/grade
        float baseTime = 30f; 

        if (sceneName.Contains("1")) baseTime = scene1BaseTime;
        else if (sceneName.Contains("2")) baseTime = scene2BaseTime;
        else if (sceneName.Contains("3")) baseTime = scene3BaseTime;

        // Penalty increases with level
        float difficultyPenalty = 1.0f + (currentLevel * 0.06f); 

        return baseTime * difficultyPenalty;
    }

    /// <summary>
    /// Decides the Next Level based on the selected Mode.
    /// </summary>
    public void AnalyzePerformance(float scoreRatio)
    {
        Debug.Log($"[DifficultyManager] Mode: {progressionMode} | Score Ratio: {scoreRatio:F2}");

        if (progressionMode == DifficultyMode.Linear)
        {
            // --- LINEAR ALGORITHM ---
            // Simply increase level no matter what.
            // This forces "Progressive Overload".
            currentLevel++;
            Debug.Log("Result (Linear): Forced Level Up.");
        }
        else
        {
            // --- ADAPTIVE ALGORITHM ---
            // Your novel research logic
            if (scoreRatio > 1.2f)
            {
                currentLevel++;
                Debug.Log("Result (Adaptive): Excellent! Level Up.");
            }
            else if (scoreRatio > 0.9f)
            {
                currentLevel++; 
                Debug.Log("Result (Adaptive): Good Flow. Level Up.");
            }
            else if (scoreRatio < 0.6f)
            {
                currentLevel--;
                Debug.Log("Result (Adaptive): Struggled. Level Down.");
            }
            else
            {
                Debug.Log("Result (Adaptive): Balanced. Maintain Level.");
            }
        }

        // Safety Clamp (Ensure we stay between 1 and 5)
        currentLevel = Mathf.Clamp(currentLevel, 1, maxLevel);
        Debug.Log($"[DifficultyManager] New Level Set To: {currentLevel}");
    }
}