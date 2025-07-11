using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Settings")]
    [SerializeField] private Text scoreText; // 使用SerializeField保持私有但可在Inspector编辑
    [SerializeField] private Text targetText;
    [SerializeField] private Text LevelText;

    [Header("Win Condition")]
    [Tooltip("Target score to achieve victory")]
    public int targetScore;
    public double Level = 1;
    public string victorySceneName = "VictoryScene";
    public float sceneLoadDelay = 2f;

    private int currentScore = 0;
    private bool isVictoryAchieved = false;
    public static int currentSceneIndex;

    private void Awake()
    {

        InitializeSingleton();
    }

    void Start()
    {
        UpdateScoreDisplay();
        UpdateTargetScoreDisplay();
        UpdateLevelDisplay();
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重新查找UI引用（如果场景变化）
        scoreText = FindScoreTextInScene();
        targetText = FindtargetTextInScene();
        LevelText = FindLevelInScene();
        UpdateScoreDisplay();
        UpdateTargetScoreDisplay();
        UpdateLevelDisplay();
    }

    private Text FindScoreTextInScene()
    {
        var textObj = GameObject.Find("ScoreText")?.GetComponent<Text>();
        return textObj;
    }
    private Text FindtargetTextInScene()
    {
        var textObj = GameObject.Find("targetscore")?.GetComponent<Text>();
        return textObj;
    }

    private Text FindLevelInScene()
    {
        var textObj = GameObject.Find("LevelText")?.GetComponent<Text>();
        return textObj;
    }
    public void AddScore(int points)
    {
        if (isVictoryAchieved) return;

        currentScore = Mathf.Clamp(currentScore + points, 0, int.MaxValue);
        UpdateScoreDisplay();
        if (currentScore % 400 == 0)
        {
            PlayerHealth.Instance.RestoreHeart();
        }
        
        if (currentScore >= targetScore)
            {
                if (Level == 10)
                {
                    SceneManager.LoadScene(7);
                    return;
                }
                TriggerVictory();
            }
    }

    public static int returnIndex()
    {
        return currentSceneIndex;
    }
    private void UpdateTargetScoreDisplay()
    {
        if (targetText != null)
        {
            targetText.text = $"Target:{targetScore}";
        }
    }
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }
    private void UpdateLevelDisplay()
    {
        if (LevelText != null)
        {
            LevelText.text = $"Level:{Level}";
        }
    }

    private void TriggerVictory()
    {
        isVictoryAchieved = true;
        Debug.Log($"Victory Achieved! Score: {currentScore}");

        // 暂停游戏逻辑
        //Time.timeScale = 0f;

        // 延迟加载场景（使用协程避免Time.scale影响）
        StartCoroutine(LoadVictorySceneWithDelay());
    }

    private IEnumerator LoadVictorySceneWithDelay()
    {
        // 使用realtimeSinceStartup避免受Time.timeScale影响
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < startTime + sceneLoadDelay)
        {
            yield return null;
        }

        //Time.timeScale = 1f;
        LoadSceneSafely(victorySceneName);
    }

    private void LoadSceneSafely(string sceneName)
    {
        if (SceneExists(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene not found: {sceneName}");
            DebugAvailableScenes();
        }
    }

    private bool SceneExists(string sceneName)
    {
        return Application.CanStreamedLevelBeLoaded(sceneName);
    }

    private void DebugAvailableScenes()
    {
        Debug.Log("Available Scenes:");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            Debug.Log(SceneUtility.GetScenePathByBuildIndex(i));
        }
    }

    public void ResetGameState()
    {
        currentScore = 0;
        targetScore += 100;
        Level = Level + 0.5;
        isVictoryAchieved = false;
        Time.timeScale = 1f;
    }
    public void TryAgainGameState()
    {
        currentScore = 0;
        targetScore = 800;
        Level = 1;
        isVictoryAchieved = false;
        Time.timeScale = 1f;
        AbilityManager.Instance.ClearAllAbilities();
    }
    
}