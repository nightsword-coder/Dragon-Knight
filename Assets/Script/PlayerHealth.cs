using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public GameObject[] hearts; // 假设数组索引0-5对应6颗心（从左到右）
    private int currentHeartIndex;

    [Header("游戏失败设置")]
    public string gameOverSceneName = "游戏失败场景";
    public float gameOverDelay = 1.5f;

    [Header("能力判定")]
    public int requiredAbilityId = 5; // 能力ID=5（血量）
    public static PlayerHealth Instance;
    void Awake()
    {
        // 单例初始化
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // 确保只有一个实例存在
    }
    void Start()
    {
        // 初始化当前心索引（假设前3颗心为基础生命，后3颗由能力解锁）
        currentHeartIndex = hearts.Length - 1; // 初始指向最后一颗心（索引5）

        // 检查是否有能力5，若无则销毁后3颗心（索引3、4、5）
        if (!AbilityManager.Instance.IsAbilityAcquired(requiredAbilityId))
        {
            DestroyHeartsFromIndex(3); // 销毁索引3及之后的心（共3颗）
            currentHeartIndex = 2; // 更新当前心索引为基础生命的最后一颗（索引2）
        }

        if (hearts == null || hearts.Length == 0)
        {
            Debug.LogError("心形数组未设置或为空！");
        }
    }

    // 销毁从指定索引开始的所有心
    private void DestroyHeartsFromIndex(int startIndex)
    {
        for (int i = startIndex; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                Destroy(hearts[i]);
            }
        }
    }

    public void LoseHeart()
    {
        for (int i = 1; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
            {
                currentHeartIndex = i - 1;
                break;
            }
        }
        if (currentHeartIndex >= 0)
        {
            if (hearts[currentHeartIndex] != null)
            {
                Destroy(hearts[currentHeartIndex]);
            }
            currentHeartIndex--;
        }

        if (currentHeartIndex < 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("游戏结束！切换到失败场景");
        Time.timeScale = 0f;
        StartCoroutine(LoadGameOverSceneAfterDelay());
    }

    private IEnumerator LoadGameOverSceneAfterDelay()
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + gameOverDelay)
        {
            yield return null;
        }

        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogError($"无法加载场景: {gameOverSceneName}");
        }
    }
    public void RestoreHeart()
    {
        if (AbilityManager.Instance.IsAbilityAcquired(0))
        {
            Debug.Log("触发吸血");
            int mylife = 3;
            if (AbilityManager.Instance.IsAbilityAcquired(5))
            {
                mylife = 6;
            }
            for (int i = 1; i < mylife; i++)
            {
                if (hearts[i] == null)
                {
                    float cot = i;

                    Vector3 position = new Vector3((float)(-6.7 + cot / 2), (float)4.6, 0);
                    GameObject heartexample = Instantiate(hearts[0], position, Quaternion.identity);
                    hearts[i] = heartexample;
                    return;
                }

            }
        }
    }
}