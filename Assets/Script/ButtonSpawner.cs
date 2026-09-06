using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonSpawner : MonoBehaviour
{
    [Header("UI 设置")]
    public Canvas targetCanvas;
    public GameObject[] buttonPresets;
    public Transform[] spawnPositions;
    public int buttonsToSpawn = 3;
    public int maxAbilities;
    public GameObject maxAbilitiesReachedPanel;
    public Button levelUpButton; // 新增：引用LevelUp按钮

    private List<int> currentDisplayedAbilities = new List<int>(); // 本轮显示的能力

    private void Start()
    {
        maxAbilities = AbilityManager.Instance.maxAbilities;
        if (!targetCanvas || buttonPresets.Length < 6 || spawnPositions.Length < buttonsToSpawn)
        {
            Debug.LogError("请配置画布、6个按钮预设和足够的生成位置！");
            return;
        }

        // 确保LevelUp按钮始终可点击
        if (levelUpButton)
            levelUpButton.interactable = true;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        GenerateButtons();
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        GenerateButtons();
    }

    private void GenerateButtons()
    {
        ClearGeneratedButtons();

        // 获取已获得的能力列表
        List<int> acquiredAbilities = AbilityManager.Instance.GetAcquiredAbilities();
        
        // 检查是否达到最大能力数
        if (acquiredAbilities.Count >= maxAbilities)
        {
            Debug.Log($"已达到最大能力数限制: {maxAbilities}");
            ShowMaxAbilitiesReachedPanel();
            return;
        }

        // 获取可用能力列表（排除已获得和本轮已显示的）
        List<int> availableAbilities = GetAvailableAbilities(acquiredAbilities);

        // 计算实际可生成的按钮数量
        int countToSpawn = Mathf.Min(buttonsToSpawn, availableAbilities.Count, spawnPositions.Length);

        if (countToSpawn == 0)
        {
            Debug.Log("没有可用的能力或位置来生成按钮");
            return;
        }

        // 不放回抽样，确保不重复
        List<int> selectedIndices = SelectRandomIndicesWithoutReplacement(availableAbilities, countToSpawn);

        // 清空本轮显示列表
        currentDisplayedAbilities.Clear();

        // 生成按钮
        for (int i = 0; i < countToSpawn; i++)
        {
            int abilityId = availableAbilities[selectedIndices[i]];
            currentDisplayedAbilities.Add(abilityId);  // 记录本轮显示的能力
            SpawnButton(abilityId, spawnPositions[i]);
        }

        // 将本轮显示的能力告知能力管理器
        AbilityManager.Instance.RecordDisplayedAbilities(currentDisplayedAbilities);
    }

    private List<int> GetAvailableAbilities(List<int> acquiredAbilities)
    {
        List<int> available = new List<int>();
        List<int> displayedThisRound = AbilityManager.Instance.GetDisplayedAbilitiesThisRound();
        
        for (int i = 0; i < buttonPresets.Length; i++)
        {
            // 排除已获得的能力 和 本轮已显示的能力
            if (!acquiredAbilities.Contains(i) && !displayedThisRound.Contains(i))
            {
                available.Add(i);
            }
        }
        return available;
    }

    private List<int> SelectRandomIndicesWithoutReplacement(List<int> pool, int count)
    {
        List<int> result = new List<int>();
        
        if (count >= pool.Count)
        {
            for (int i = 0; i < pool.Count; i++)
                result.Add(i);
            return result;
        }
        
        List<int> indices = new List<int>();
        for (int i = 0; i < pool.Count; i++)
            indices.Add(i);
        
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(i, indices.Count);
            (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
            result.Add(indices[i]);
        }
        
        return result;
    }

    private void SpawnButton(int abilityId, Transform position)
    {
        if (abilityId < 0 || abilityId >= buttonPresets.Length)
        {
            Debug.LogError($"能力ID越界: {abilityId}");
            return;
        }

        GameObject button = Instantiate(buttonPresets[abilityId], targetCanvas.transform);
        button.name = $"AbilityButton_{abilityId}";
        
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchoredPosition = position.GetComponent<RectTransform>().anchoredPosition;
        
        Button btnComponent = button.GetComponent<Button>();
        if (btnComponent)
        {
            btnComponent.onClick.AddListener(() => OnButtonClicked(abilityId));
        }
    }

    private void OnButtonClicked(int abilityId)
    {
        Debug.Log($"选择能力: {abilityId}");
        AbilityManager.Instance.SelectAbility(abilityId);
    }

    private void ClearGeneratedButtons()
    {
        foreach (Transform child in targetCanvas.transform)
        {
            if (child.name.StartsWith("AbilityButton_"))
            {
                Destroy(child.gameObject);
            }
        }
        
        // 隐藏最大能力数提示面板
        HideMaxAbilitiesReachedPanel();
    }

    private void ShowMaxAbilitiesReachedPanel()
    {
        if (maxAbilitiesReachedPanel)
        {
            maxAbilitiesReachedPanel.SetActive(true);
        }
    }

    private void HideMaxAbilitiesReachedPanel()
    {
        if (maxAbilitiesReachedPanel)
        {
            maxAbilitiesReachedPanel.SetActive(false);
        }
    }
}