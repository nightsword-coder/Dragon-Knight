using UnityEngine;
using UnityEngine.SceneManagement;
public class NianMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 2f;       // 移动速度
    public float minY = -3f;           // Y轴最小位置
    public float maxY = 3f;            // Y轴最大位置
    public float randomInterval = 2f;  // 随机目标间隔(秒)
    
    [Header("碰撞参数")]
    public string projectileTag = "PlayerProjectiles"; // 玩家子弹标签
    private float targetY;             // 目标Y坐标
    private float nextRandomTime;      // 下次随机时间
    public int BossHealth = 20000;
    public GameObject BossFire;
    void Start()
    {
        nextRandomTime = Time.time + randomInterval;
        SetRandomTargetY();
    }

    void Update()
    {
        // 移动到目标位置
        float currentY = transform.position.y;
        float newY = Mathf.MoveTowards(currentY, targetY, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        // 到达目标或时间到，随机选择新目标
        if (IsAtTargetPosition() || Time.time >= nextRandomTime)
        {
            SetRandomTargetY();
            nextRandomTime = Time.time + randomInterval;
        }
        
    }

    /// <summary>
    /// 设置随机目标Y坐标
    /// </summary>
    private void SetRandomTargetY()
    {
        targetY = Random.Range(minY, maxY);
    }

    /// <summary>
    /// 检查是否到达目标位置
    /// </summary>
    private bool IsAtTargetPosition()
    {
        return Mathf.Abs(transform.position.y - targetY) < 0.1f;
    }
     public void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag(projectileTag))
        {
            if (collision.name == "dragonfire(Clone)")
            {
                BossHealth = BossHealth - 100;
                Debug.Log("Boss血量" + BossHealth);
            }
            else if(collision.name == "紫火炮弹图片(Clone)"){
                BossHealth = BossHealth - 300;
                Debug.Log("Boss血量" + BossHealth);
            }
            else if(collision.name == "巨焰龙息炮弹(Clone)"){
                BossHealth = BossHealth - 300;
                Debug.Log("Boss血量" + BossHealth);
            }
            if (BossHealth <= 0)
            {
                Destroy(gameObject);
                SceneManager.LoadScene(7);
            }
            Debug.Log(collision.name);
            Destroy(collision.gameObject);   
        }
    }
}