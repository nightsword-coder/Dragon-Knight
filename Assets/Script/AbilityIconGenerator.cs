using UnityEngine;
using System.Collections.Generic;

public class AbilityIconGenerator : MonoBehaviour
{
    [Header("配置")]
    public Transform iconParent;           // 图标父级容器
    public GameObject abilityIconPrefab;   // 图标预制体
    public List<Sprite> abilitySprites;    // 能力贴图列表
    public List<GameObject> abilityModels; // 能力模型列表（可选）
    
    [Header("布局设置")]
    public Vector3 iconSpacing = new Vector3(2, 0, 0); // 图标间距（水平/垂直/深度）
    public bool isHorizontal = true;                   // 水平排列（true）或垂直排列（false）
    public Vector3 startPosition = Vector3.zero;        // 起始位置

    private Dictionary<int, GameObject> activeIcons = new Dictionary<int, GameObject>();
    private int iconCount;

    private void Start()
    {
        GenerateAbilityIcons();
    }

    public void GenerateAbilityIcons()
    {
        ClearAbilityIcons();
        List<int> abilities = AbilityManager.Instance.GetAcquiredAbilities();
        iconCount = abilities.Count;

        if (iconCount == 0) return;

        for (int i = 0; i < abilities.Count; i++)
        {
            int abilityId = abilities[i];
            CreateAbilityIcon(abilityId, i);
        }
    }

    private void CreateAbilityIcon(int abilityId, int index)
    {
        if (abilityIconPrefab == null)
        {
            Debug.LogError("错误：未设置能力图标预制体！");
            return;
        }

        // 实例化图标并设置位置
        GameObject icon = Instantiate(abilityIconPrefab, iconParent);
        icon.name = $"Ability_{abilityId}";
        SetIconPosition(icon, index);

        // 设置渲染内容
        SetupRenderer(icon, abilityId);
        activeIcons[abilityId] = icon;

        // 添加点击事件
        AddClickEvent(icon, abilityId);
    }

    private void SetIconPosition(GameObject icon, int index)
    {
        Vector3 position = startPosition;
        if (isHorizontal)
        {
            position.x += index * iconSpacing.x; // 水平方向递增
        }
        else
        {
            position.y += index * iconSpacing.y; // 垂直方向递增
        }
        icon.transform.position = position;
    }

    private void SetupRenderer(GameObject icon, int abilityId)
    {
        // 优先加载模型
        if (abilityModels != null && abilityId < abilityModels.Count && abilityModels[abilityId] != null)
        {
            Instantiate(abilityModels[abilityId], icon.transform);
            return;
        }

        // 加载Sprite
        SpriteRenderer spriteRenderer = icon.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && abilityId < abilitySprites.Count)
        {
            spriteRenderer.sprite = abilitySprites[abilityId];
        }
    }

    private void AddClickEvent(GameObject icon, int abilityId)
    {
        Collider collider = icon.GetComponent<Collider>();
        if (collider == null)
        {
            collider = icon.AddComponent<BoxCollider>();
        }
        icon.AddComponent<AbilityIconClickHandler>().Initialize(abilityId);
    }

    private void ClearAbilityIcons()
    {
        foreach (var icon in activeIcons.Values)
        {
            Destroy(icon);
        }
        activeIcons.Clear();
        iconCount = 0;
    }
}

// 点击事件组件（不变）
public class AbilityIconClickHandler : MonoBehaviour
{
    private int abilityId;

    public void Initialize(int id)
    {
        abilityId = id;
    }

    private void OnMouseDown()
    {
        Debug.Log($"点击能力图标：{AbilityManager.Instance.GetAbilityName(abilityId)}");
        AbilityManager.Instance.SelectAbility(abilityId);
    }
}