using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("碰撞设置")]
    public string playerTag = "Player";
    public int maxHits = 3;

    private EnemySpawner spawner;
    private float speed;
    private float minX, maxX, minY, maxY;
    private int hitCount = 0;
    private bool hasHitPlayer = false; // 防止同一碰撞多次调用

    public void Initialize(EnemySpawner spawnerRef, float enemySpeed, 
                         float destroyMinX, float destroyMaxX, 
                         float destroyMinY, float destroyMaxY)
    {
        spawner = spawnerRef;
        speed = enemySpeed;
        minX = destroyMinX;
        maxX = destroyMaxX;
        minY = destroyMinY;
        maxY = destroyMaxY;
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // 边界检查
        Vector2 pos = transform.position;
        if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
        {
            DestroyEnemy();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag(playerTag))
    {
        if (!hasHitPlayer)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.LoseHeart();
            }
            hasHitPlayer = true;
        }
        DestroyEnemy();
        return;
    }

    hitCount++;
    if (hitCount >= maxHits)
    {
        // 敌人被完全击败，增加分数
        ScoreManager.Instance?.AddScore(100);
        DestroyEnemy();
    }
}

// 同样修改OnTriggerEnter2D中的相应部分

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!hasHitPlayer)
            {
                // 获取玩家的PlayerHealth组件
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.LoseHeart();
                }
                hasHitPlayer = true;
            }
            
            DestroyEnemy();
            return;
        }
    }

    void DestroyEnemy()
    {
        if (spawner != null)
        {
            spawner.OnEnemyDestroyed();
        }
        Destroy(gameObject);
    }
}