using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("子弹设置")]
    public GameObject bulletPrefab;
    public float fireRate = 1.5f;
    
    [Header("发射参数")]
    [Tooltip("子弹初始速度")]
    public float bulletSpeed = 4f;
    
    [Tooltip("每个发射点的固定发射角度（0=右，90=上，180=左，270=下）")]
    public float[] bulletAngles = new float[1] { 180f };
    
    [Tooltip("是否为每个子弹添加随机角度偏移")]
    public bool addRandomOffset = false;
    
    [Tooltip("随机角度偏移范围（仅在addRandomOffset为true时生效）")]
    [Range(0f, 45f)]
    public float randomOffsetRange = 10f;

    [Header("发射位置")]
    [Tooltip("每个发射点的局部偏移位置")]
    public Vector2[] localOffsets = new Vector2[1]; // 多个局部偏移

    private float timer = 1.5f;

    void Start()
    {
        // 确保角度数组和偏移数组长度一致
        if (localOffsets.Length != bulletAngles.Length)
        {
            Debug.LogWarning("警告：localOffsets和bulletAngles数组长度不一致！将使用默认值。");
            
            // 调整角度数组长度以匹配偏移数组
            System.Array.Resize(ref bulletAngles, localOffsets.Length);
            
            // 填充默认角度（向左）
            for (int i = 0; i < bulletAngles.Length; i++)
            {
                if (bulletAngles[i] == 0)
                    bulletAngles[i] = 180f;
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            timer = 0f;
            ShootFromAllOffsets();
        }
    }

    void ShootFromAllOffsets()
    {
        if (bulletPrefab == null || localOffsets.Length == 0)
            return;

        // 遍历每个发射点
        for (int i = 0; i < localOffsets.Length; i++)
        {
            Vector3 spawnPosition = transform.TransformPoint(localOffsets[i]);
            
            // 获取当前发射点的固定角度
            float baseAngle = bulletAngles[i % bulletAngles.Length]; // 防止数组越界
            
            // 计算最终角度（基础角度 + 随机偏移）
            float finalAngle = baseAngle;
            if (addRandomOffset)
            {
                finalAngle += Random.Range(-randomOffsetRange, randomOffsetRange);
            }
            
            // 计算方向向量
            Vector2 direction = Quaternion.Euler(0, 0, finalAngle) * Vector2.right;
            
            // 生成子弹
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            
            if (bulletRb != null)
            {
                bulletRb.velocity = direction * bulletSpeed;
            }

            // 旋转子弹使其朝向运动方向
            bullet.transform.rotation = Quaternion.Euler(0, 0, finalAngle);
        }
    }
}