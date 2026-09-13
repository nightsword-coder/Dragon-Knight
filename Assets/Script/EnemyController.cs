using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("碰撞设置")]
    public string playerTag = "Player";
    public int maxHealth = 300;
    public int scoreValue = 100;
    public float speedMultiplier = 1f;

    private EnemySpawner spawner;
    public float speed;
    private float minX, maxX, minY, maxY;
    private int currentHealth;
    private bool hasHitPlayer = false; // 防止同一碰撞多次调用

    private bool snakeMove;
    private float snakeAmplitude = 1.5f;
    private float snakeFrequency = 2.5f;
    private float snakePhase;
    private float snakeTime;
    private float centerY;

    private bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Initialize(EnemySpawner spawnerRef, float enemySpeed, 
                         float destroyMinX, float destroyMaxX, 
                         float destroyMinY, float destroyMaxY)
    {
        spawner = spawnerRef;
        speed = enemySpeed * speedMultiplier;
        minX = destroyMinX;
        maxX = destroyMaxX;
        minY = destroyMinY;
        maxY = destroyMaxY;
        centerY = transform.position.y;
        currentHealth = maxHealth;
        hasHitPlayer = false;
        isDead = false;
        snakeMove = false;
        snakeTime = 0f;
    }

    public void EnableSnakeMove(float phase, float amplitude, float frequency)
    {
        snakeMove = true;
        snakePhase = phase;
        snakeAmplitude = amplitude;
        snakeFrequency = frequency;
        snakeTime = 0f;
        centerY = transform.position.y;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.x += speed * Time.deltaTime;

        if (snakeMove)
        {
            snakeTime += Time.deltaTime;
            pos.y = centerY + Mathf.Sin(snakeTime * snakeFrequency + snakePhase) * snakeAmplitude;
            pos.y = Mathf.Clamp(pos.y, minY + 0.3f, maxY - 0.3f);
        }

        transform.position = pos;

        if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
            DestroyEnemy();
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

    public void TakeDamage(int amount)
    {
        if (isDead || currentHealth <= 0)
            return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            ScoreManager.Instance?.AddScore(scoreValue);
            DestroyEnemy();
        }
    }

    void DestroyEnemy()
    {
        if (isDead)
            return;

        isDead = true;
        if (spawner != null)
            spawner.OnEnemyDestroyed();

        ObjectPool.Release(gameObject);
    }
}