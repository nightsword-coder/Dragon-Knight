using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NianMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 2f;
    public float minY = -3f;
    public float maxY = 3f;
    public float randomInterval = 2f;

    [Header("碰撞参数")]
    public string projectileTag = "PlayerProjectiles";

    [Header("血条设置")]
    public int MaxBossHealth = 5000;

    private float targetY;
    private float nextRandomTime;
    public int BossHealth;
    public GameObject BossFire;

    private Image fillImage;

    void Start()
    {

       
        GameObject fillGo = GameObject.Find("Canvas/HpBar/Fill");
        if (fillGo != null)
        {
            fillImage = fillGo.GetComponent<Image>();
            if (fillImage == null)
            {
                Debug.LogError("【错误】Fill物体上没有Image组件");
            }
        }
        else
        {
            Debug.LogError("【错误】找不到 Canvas/Fill，请检查Hierarchy名字");
        }

        BossHealth = MaxBossHealth;
        nextRandomTime = Time.time + randomInterval;
        SetRandomTargetY();
        UpdateHealthBar();
    }

    void Update()
    {
        float currentY = transform.position.y;
        float newY = Mathf.MoveTowards(currentY, targetY, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (IsAtTargetPosition() || Time.time >= nextRandomTime)
        {
            SetRandomTargetY();
            nextRandomTime = Time.time + randomInterval;
        }
    }

    private void SetRandomTargetY()
    {
        targetY = Random.Range(minY, maxY);
    }

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
                BossHealth -= 100;
            }
            else if (collision.name == "紫火炮弹图片(Clone)")
            {
                BossHealth -= 300;
            }
            else if (collision.name == "巨焰龙息炮弹(Clone)")
            {
                BossHealth -= 300;
            }

            BossHealth = Mathf.Clamp(BossHealth, 0, MaxBossHealth);
            Debug.Log("Boss血量：" + BossHealth);
            UpdateHealthBar();

            if (BossHealth <= 0)
            {
                Destroy(gameObject);
                SceneManager.LoadScene(7);
            }
            Debug.Log(collision.name);
            ObjectPool.Release(collision.gameObject);
        }
    }

    void UpdateHealthBar()
    {
        if (fillImage == null) return;
        fillImage.fillAmount = (float)BossHealth / MaxBossHealth;
    }
}
