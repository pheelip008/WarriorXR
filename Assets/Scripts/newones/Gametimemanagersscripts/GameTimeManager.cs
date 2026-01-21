// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement; 
// using System.Collections;
// using TMPro; 

// public class GameTimeManager : MonoBehaviour
// {
//     public static GameTimeManager Instance;

//     // --- STATIC VARIABLE (Survives Scene Loads) ---
//     public static float GlobalScore = 0f; 

//     [Header("UI References")]
//     public TextMeshProUGUI timerText;    
//     public GameObject resultPanel;       
//     public TextMeshProUGUI resultText;   
//     public TextMeshProUGUI scoreText;    

//     [Header("Difficulty Detection")]
//     [Tooltip("Drag the object that has the rotation script here.")]
//     public PlaneBendController_Part2 rotationScript; // REPLACE with your actual script name if different
    
//     [Header("Ideal Times (Seconds)")]
//     public float easyIdealTime = 30f;   // For speed ~8
//     public float mediumIdealTime = 20f; // For speed ~3
//     public float hardIdealTime = 10f;   // For speed ~0.2

//     [Header("Game Settings")]
//     public string[] availableScenes; 

//     // Internal State
//     private float currentTime = 0f;
//     private bool isTimerRunning = false;
//     private bool gameFinished = false;
//     private float currentIdealTime = 0f;

//     void Awake()
//     {
//         Instance = this;
//         if (resultPanel != null) resultPanel.SetActive(false);
//     }

//     void Start()
//     {
//         // SHOW PREVIOUS SCORE IMMEDIATELY
//         UpdateScoreUI(0); // 0 means "haven't earned anything this level yet"
        
//         // Detect difficulty as soon as scene starts
//         DetermineDifficulty();
//     }

//     /// <summary>
//     /// Looks at the rotation speed and sets the Ideal Time.
//     /// </summary>
//     void DetermineDifficulty()
//     {
//         if (rotationScript == null)
//         {
//             Debug.LogError("Please assign the Rotation Script to the GameTimeManager!");
//             currentIdealTime = mediumIdealTime; // Default fallback
//             return;
//         }

//         // READ SPEED (Assuming your script has a variable like 'rotationSpeed')
//         // You might need to change '.speed' to whatever your variable is named.
//         float speed = rotationScript.rotationSpeed; 

//         // LOGIC FROM YOUR PROMPT
//         if (speed <= 0.5f) // Around 0.2 -> HARD
//         {
//             currentIdealTime = hardIdealTime;
//             Debug.Log($"[Manager] Speed is {speed} (Hard). Ideal Time: {currentIdealTime}s");
//         }
//         else if (speed <= 5.0f) // Around 3 -> MEDIUM
//         {
//             currentIdealTime = mediumIdealTime;
//             Debug.Log($"[Manager] Speed is {speed} (Medium). Ideal Time: {currentIdealTime}s");
//         }
//         else // Around 8 -> EASY
//         {
//             currentIdealTime = easyIdealTime;
//             Debug.Log($"[Manager] Speed is {speed} (Easy). Ideal Time: {currentIdealTime}s");
//         }
//     }

//     public void StartGameTimer()
//     {
//         if (gameFinished) return;
//         currentTime = 0f;
//         isTimerRunning = true;
//     }

//     void Update()
//     {
//         if (isTimerRunning && !gameFinished)
//         {
//             currentTime += Time.deltaTime;
//             if (timerText != null) timerText.text = $"{currentTime:F1}s";
//         }
//     }

//     public void FinishGame()
//     {
//         if (gameFinished) return; 
//         gameFinished = true;
//         isTimerRunning = false;

//         StartCoroutine(ShowResultsAndLoadNext());
//     }

//     private IEnumerator ShowResultsAndLoadNext()
//     {
//         // 1. Calculate Score for THIS level
//         float levelScore = 0f;
//         if (currentTime > 0.1f)
//             levelScore = currentIdealTime / currentTime; 

//         // 2. Add to Global Score
//         GlobalScore += levelScore;

//         // 3. Update UI
//         UpdateScoreUI(levelScore);

//         Debug.Log($"Level Done. Level Score: {levelScore:F2}, New Total: {GlobalScore:F2}");

//         // 4. Wait 5 Seconds
//         yield return new WaitForSeconds(5.0f);

//         // 5. Load Next Scene
//         LoadRandomScene();
//     }

//     private void UpdateScoreUI(float justEarned)
//     {
//         if (scoreText != null) 
//         {
//             // Shows: "Total: 15.5 (+2.3)"
//             scoreText.text = $"Total Score: {GlobalScore:F2}";
            
//             if (gameFinished)
//             {
//                 // If game just finished, show what we just added
//                 scoreText.text += $"\n(+{justEarned:F2} from this level)";
//             }
//         }

//         if (gameFinished && resultText != null)
//         {
//             resultText.text = $"Time: {currentTime:F2}s / Ideal: {currentIdealTime}s";
//         }
//     }

//     private void LoadRandomScene()
//     {
//         if (availableScenes.Length == 0) return;
//         int randomIndex = Random.Range(0, availableScenes.Length);
//         SceneManager.LoadScene(availableScenes[randomIndex]);
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;
using TMPro; 

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;

    // --- STATIC VARIABLE (Survives Scene Loads) ---
    public static float GlobalScore = 0f; 

    [Header("UI References")]
    public TextMeshProUGUI timerText;    
    public GameObject resultPanel;       
    public TextMeshProUGUI resultText;   
    public TextMeshProUGUI scoreText;    

    [Header("External Systems")]
    [Tooltip("Drag the object that has the rotation script here.")]
    public PlaneBendController_Part2 rotationScript; 
    
    // Note: We no longer need manual Easy/Med/Hard float variables here.
    // They are handled by DifficultyManager.

    [Header("Game Settings")]
    public string[] availableScenes; 

    // Internal State
    private float currentTime = 0f;
    private bool isTimerRunning = false;
    private bool gameFinished = false;
    private float currentIdealTime = 0f;

    void Awake()
    {
        Instance = this;
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void Start()
    {
        // 1. Ensure Difficulty Manager Exists (Safety check)
        if (DifficultyManager.Instance == null)
        {
            // If starting from Scene 2/3 without menu, create a temp one
            GameObject go = new GameObject("DifficultyManager_Auto");
            go.AddComponent<DifficultyManager>();
        }

        // 2. Update Previous Score UI
        UpdateScoreUI(0); 
        
        // 3. APPLY DIFFICULTY SETTINGS (The Magic Step)
        ApplyDifficultyToLevel();
    }

    void ApplyDifficultyToLevel()
    {
        // A. Get Data from Manager
        float speed = DifficultyManager.Instance.GetCurrentRotationSpeed();
        float multiplier = DifficultyManager.Instance.GetCurrentThresholdMultiplier();
        string sceneName = SceneManager.GetActiveScene().name;
        
        // B. Calculate Ideal Time
        currentIdealTime = DifficultyManager.Instance.GetIdealTime(sceneName);
        Debug.Log($"[GameManager] Level {DifficultyManager.Instance.currentLevel} Init. Ideal Time: {currentIdealTime}s");

        // C. Apply Rotation Speed
        if (rotationScript != null)
        {
            rotationScript.rotationSpeed = speed; 
            Debug.Log($"[GameManager] Applied Rotation Speed: {speed}");
        }

        // D. Apply Squat Threshold Multiplier
        // We wait a tiny bit to ensure Instance is set, or direct call if available
        if (HeadBendZoneController_Part2.Instance != null)
        {
            HeadBendZoneController_Part2.Instance.ApplyDifficultyMultiplier(multiplier);
        }
    }

    public void StartGameTimer()
    {
        if (gameFinished) return;
        currentTime = 0f;
        isTimerRunning = true;
    }

    // Add this variable at the top of GameTimeManager class
    private float lastUiUpdate = 0f; 

    void Update()
    {
        if (isTimerRunning && !gameFinished)
        {
            currentTime += Time.deltaTime;

            // OPTIMIZATION: Only update UI 10 times per second (every 0.1s)
            // instead of every single frame.
            if (currentTime - lastUiUpdate > 0.1f)
            {
                if (timerText != null) 
                    timerText.text = $"{currentTime:F1}s";
                
                lastUiUpdate = currentTime;
            }
        }
    }

    public void FinishGame()
    {
        if (gameFinished) return; 
        gameFinished = true;
        isTimerRunning = false;

        StartCoroutine(ShowResultsAndLoadNext());
    }

    private IEnumerator ShowResultsAndLoadNext()
    {
        // 1. Calculate Score for THIS level
        float levelScore = 0f;
        if (currentTime > 0.1f)
            levelScore = currentIdealTime / currentTime; 

        // 2. Add to Global Score
        GlobalScore += levelScore;

        // 3. Update UI
        UpdateScoreUI(levelScore);

        Debug.Log($"Level Done. Level Score: {levelScore:F2}, New Total: {GlobalScore:F2}");

        // --- 4. TELL DIFFICULTY MANAGER TO ADJUST FOR NEXT LEVEL ---
        DifficultyManager.Instance.AnalyzePerformance(levelScore);
        // -----------------------------------------------------------

        // 5. Wait 5 Seconds
        yield return new WaitForSeconds(5.0f);

        // 6. Load Next Scene
        LoadRandomScene();
    }

    private void UpdateScoreUI(float justEarned)
    {
        if (scoreText != null) 
        {
            scoreText.text = $"Total Score: {GlobalScore:F2}";
            
            if (gameFinished)
            {
                scoreText.text += $"\n(+{justEarned:F2} this level)";
            }
        }

        if (gameFinished && resultText != null)
        {
            resultText.text = $"Time: {currentTime:F2}s / Ideal: {currentIdealTime:F1}s";
        }
    }

    private void LoadRandomScene()
    {
        if (availableScenes.Length == 0) return;
        int randomIndex = Random.Range(0, availableScenes.Length);
        SceneManager.LoadScene(availableScenes[randomIndex]);
    }
}