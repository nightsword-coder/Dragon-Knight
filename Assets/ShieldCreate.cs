using UnityEngine;
using System.Collections.Generic;

public class ShieldCreate : MonoBehaviour
{
    [Header("生成配置")]
    public GameObject[] prefabs;    // 预设数组
    public Transform[] spawnPoints; // 生成位置数组
    public bool spawnOnStart = true;

    [Header("生成参数")]
    public float spawnDelay = 0f;
    public bool randomizeOrder = false;
    public bool randomizeRotation = false;

    // 存储生成的实例
    private List<GameObject> spawnedInstances = new List<GameObject>();
    public static ShieldCreate Instance;
     void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        if (spawnOnStart&&AbilityManager.Instance.IsAbilityAcquired(1))
            SpawnObjects();
    }

    public void SpawnObjects()
    {
        // 清空之前的实例（如果需要保留旧实例，注释掉此行）
        ClearSpawnedInstances();

        int spawnCount = Mathf.Min(prefabs.Length, spawnPoints.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            int index = i;
            if (randomizeOrder)
                index = Random.Range(0, spawnCount);

            GameObject prefab = prefabs[index];
            Transform spawnPoint = spawnPoints[i];
            
            Quaternion rotation = spawnPoint.rotation;
            if (randomizeRotation)
                rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            // 实例化并保存引用
            GameObject instance = Instantiate(prefab, spawnPoint.position, rotation);
            instance.transform.parent = transform;
            spawnedInstances.Add(instance);

            Debug.Log($"在位置 {i} 生成对象: {prefab.name}");
        }
    }

    // 销毁所有生成的实例
    public void ClearSpawnedInstances()
    {
        foreach (GameObject instance in spawnedInstances)
        {
            if (instance != null)
                Destroy(instance);
        }
        spawnedInstances.Clear();
    }

    // 销毁指定索引的实例
    public void DestroyInstance(int index)
    {
        if (index >= 0 && index < spawnedInstances.Count)
        {
            if (spawnedInstances[index] != null)
                Destroy(spawnedInstances[index]);
                
            spawnedInstances.RemoveAt(index);
        }
    }

    // 获取护盾数量（假设护盾是prefabs[0]类型）
    public int GetShieldCount()
    {
        int count = 0;
        foreach (GameObject instance in spawnedInstances)
        {
            if (instance != null && instance.name.Contains(prefabs[0].name))
                count++;
        }
        return count;
    }
}