using UnityEngine;
using System.Collections;

public class FireSpawner : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject dragonfirePrefab;  // 普通火球预制体
    public GameObject purplefire;        // 紫火预制体（不挂载碰撞脚本）
    public GameObject dragonbreath;      // 龙息预制体

    public float purpleFireCooldown = 1f;
    public float dragonBreathCooldown = 0.2f;

    private float purpleFireTimer = 0f;
    private float dragonBreathTimer = 0.2f;
    public Transform player;            // 玩家位置
    
    [Header("生成偏移")]
    public float spawnOffsetX = 1f;     // X轴生成偏移
    public float spawnOffsetY = 0f;     // Y轴生成偏移
    
    [Header("移动设置")]
    public float fireSpeed = 10f;       // 火球移动速度
    public float destroyX = 15f;        // 销毁的X坐标阈值

    [Header("按键设置")]
    public KeyCode spawnKey = KeyCode.Space;
    public KeyCode j = KeyCode.J;
    private float firerate = 0.2f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= firerate)
        {
            timer = 0f;
            SpawnFire(dragonfirePrefab);
        }
        
        purpleFireTimer = Mathf.Max(0f, purpleFireTimer - Time.deltaTime);
        dragonBreathTimer += Time.deltaTime; 
        
        // 检查能力表中是否有ID=4的能力（紫火）
        bool hasPurpleFireAbility = AbilityManager.Instance.IsAbilityAcquired(4);
        // 检查能力表中是否有ID=2的能力（龙息）
        bool hasDragonBreathAbility = AbilityManager.Instance.IsAbilityAcquired(2);

        if (player != null && hasPurpleFireAbility && purpleFireTimer <= 0f)
        {
            SpawnFire(purplefire);
            purpleFireTimer = purpleFireCooldown;
        }
        else if (hasDragonBreathAbility && dragonBreathTimer >=dragonBreathCooldown)
        {
            dragonBreathTimer = 0f;
            SpawnFire(dragonbreath);
            
        }
    }

    void SpawnFire(GameObject prefab) 
    {
        
            // 计算生成位置
           
            Vector3 spawnPos = player.transform.position + new Vector3(spawnOffsetX, spawnOffsetY, 0);
            GameObject fire = Instantiate(prefab, spawnPos, Quaternion.identity);


        if (prefab == dragonbreath)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, 45);
            Quaternion rotation1 = Quaternion.Euler(0, 0, -45);
            GameObject fire1 = Instantiate(prefab, spawnPos, rotation);
            GameObject fire2 = Instantiate(prefab, spawnPos, rotation1);
            fire1.AddComponent<FireCollisionHandler>();
            fire2.AddComponent<FireCollisionHandler>();
            float angle = 45f; // 旋转角度
            float angle1 = -45f;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            Vector2 direction1 = new Vector2(Mathf.Cos(angle1 * Mathf.Deg2Rad), Mathf.Sin(angle1 * Mathf.Deg2Rad)).normalized;
            fire1.GetComponent<Rigidbody2D>().velocity = direction * fireSpeed;
            fire2.GetComponent<Rigidbody2D>().velocity = direction1 * fireSpeed;
            StartCoroutine(DestroyAfterDistance(fire1));
            StartCoroutine(DestroyAfterDistance(fire2));
            }
        
        // 关键修改：只给特定预制体添加碰撞处理组件
            if (prefab != purplefire) // 紫火不添加碰撞脚本
            {
                fire.AddComponent<FireCollisionHandler>();
            }
        
        // 设置移动速度
        Rigidbody2D rb = fire.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(fireSpeed, 0);
        
        // 启动自动销毁协程
        StartCoroutine(DestroyAfterDistance(fire));
    }

    IEnumerator DestroyAfterDistance(GameObject fire) 
    {
        while (fire != null && fire.transform.position.x < destroyX) 
        {
            yield return null;
        }
        if (fire != null) 
        {
            Destroy(fire);
        }
    }
}