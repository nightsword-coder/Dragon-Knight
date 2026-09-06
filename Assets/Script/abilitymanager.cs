using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    [Header("能力配置")]
    public int maxAbilities; // 最多可同时拥有的能力数量

    private List<int> acquiredAbilities = new List<int>(); // 已获得的能力ID
    private int tempSelectedAbility = -1; // 临时选中的能力ID（未确认）
    private int confirmedAbility = -1; // 最终确认的能力ID
    private bool abilityConfirmedThisLevel = false; // 本关卡是否已确认能力
    private List<int> displayedAbilitiesThisRound = new List<int>(); // 本轮已显示的能力

    public delegate void OnAbilitySelected(int abilityId);
    public static event OnAbilitySelected AbilitySelected;

    public delegate void OnAbilityConfirmed(int abilityId);
    public static event OnAbilityConfirmed AbilityConfirmed;

    private void Awake()
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
    // 临时选择能力（由能力按钮点击触发）
    public void SelectAbility(int abilityId)
    {
        tempSelectedAbility = abilityId;
        Debug.Log($"临时选择能力: {GetAbilityName(abilityId)}");
        AbilitySelected?.Invoke(abilityId);
    }

    // 确认选择（由Level Up按钮点击触发）
    public void ConfirmSelectedAbility()
    {
        {
            if (IsAbilityAcquired(7))
            {
                maxAbilities = 7;
                Debug.Log("能力上限+");
            }
            else
            {
                maxAbilities = 5;
            }
        }
        if (tempSelectedAbility == -1)
            {
                Debug.Log("未选择任何能力，直接进入下一关");
                AbilityConfirmed?.Invoke(-1);
                return;
            }

        if (!acquiredAbilities.Contains(tempSelectedAbility))
        {
            if (acquiredAbilities.Count >= maxAbilities)
            {
                Debug.Log("已达到最大能力数量！");
                return;
            }

            acquiredAbilities.Add(tempSelectedAbility);
            Debug.Log($"获得新能力: {GetAbilityName(tempSelectedAbility)}");
        }

        confirmedAbility = tempSelectedAbility;
        abilityConfirmedThisLevel = true;
        Debug.Log($"确认选择能力: {GetAbilityName(confirmedAbility)}");
        AbilityConfirmed?.Invoke(confirmedAbility);
    }

    // 使用当前确认的能力
    public void UseCurrentAbility()
    {
        if (confirmedAbility == -1 || !abilityConfirmedThisLevel)
        {
            Debug.Log("未确认任何能力或本关卡未确认能力");
            return;
        }

        Debug.Log($"使用能力: {GetAbilityName(confirmedAbility)}");
    }

    public bool CanLevelUp()
    {
        return tempSelectedAbility != -1;
    }

    public bool IsAbilityAcquired(int abilityId)
    {
        return acquiredAbilities.Contains(abilityId);
    }

    // 记录本轮已显示的能力
    public void RecordDisplayedAbilities(List<int> abilities)
    {
        displayedAbilitiesThisRound = new List<int>(abilities);
    }

    // 获取本轮已显示的能力
    public List<int> GetDisplayedAbilitiesThisRound()
    {
        return displayedAbilitiesThisRound;
    }

    // 清空本轮已显示的能力（场景切换时调用）
    public void ClearRoundDisplayedAbilities()
    {
        displayedAbilitiesThisRound.Clear();
        Debug.Log("清空本轮已显示的能力");
    }

    // 准备进入下一关
    public void LevelUp()
    {
        Debug.Log($"LevelUp调用 - 临时选择能力: {tempSelectedAbility}");
        ConfirmSelectedAbility();
        tempSelectedAbility = -1;
        abilityConfirmedThisLevel = false;
        Debug.Log("准备加载场景...");
        ScoreManager.Instance.ResetGameState();
        int currentSceneIndex = ScoreManager.returnIndex();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
    }

    // 获取所有已获得的能力
    public List<int> GetAcquiredAbilities()
    {
        return acquiredAbilities;
    }

    // 获取能力名称
    public string GetAbilityName(int abilityId)
    {
        string[] abilityNames = { "嗜血", "护盾", "火屏", "爆炸", "紫火", "血量","速度","加一" };
        return abilityId < abilityNames.Length ? abilityNames[abilityId] : $"能力 {abilityId}";
    }
    public void ClearAllAbilities()
    {
        acquiredAbilities.Clear();
        Debug.Log("已清空所有能力");
    }
}