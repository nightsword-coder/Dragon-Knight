using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    public Button levelUpButton;

    private void Start()
    {
        levelUpButton.onClick.AddListener(OnLevelUpClicked);
        
        // 监听能力选择事件，更新按钮状态
        AbilityManager.AbilitySelected += OnAbilitySelected;
        UpdateButtonState();
    }

    private void OnDestroy()
    {
        // 取消订阅事件防止内存泄漏
        AbilityManager.AbilitySelected -= OnAbilitySelected;
    }

    private void OnAbilitySelected(int abilityId)
    {
        UpdateButtonState();
    }

    private void OnLevelUpClicked()
    {
        AbilityManager.Instance.LevelUp();
    }

    private void UpdateButtonState()
    {
        levelUpButton.interactable = AbilityManager.Instance.CanLevelUp();
    }
}