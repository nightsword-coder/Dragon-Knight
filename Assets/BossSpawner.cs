using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss配置")]
    public GameObject boss;          // Nian预设体
    public int spawnLevel = 10;      // 生成Boss的等级
    public int activationCode = 1;   // 激活码（用于条件判断）

    private int currentActivationCode = 1; // 当前激活码状态
    private bool bossSpawned = false;      // Boss是否已生成
    public string projectileTag = "PlayerProjectiles"; // 玩家子弹标签
    void Update()
    {
        // 检查是否满足生成条件且尚未生成Boss
        if (ScoreManager.Instance.Level == spawnLevel
            && currentActivationCode == activationCode
            && !bossSpawned)
        {
            SpawnBoss();
        }
    }

    /// <summary>
    /// 生成Boss并配置移动参数
    /// </summary>
    private void SpawnBoss()
    {
        // 更新状态防止重复生成
        bossSpawned = true;
        currentActivationCode = 2; // 更新激活码状态

        // 生成Boss
        Vector3 spawnPosition = new Vector3(6.5f, 0, -1);
        GameObject nian = Instantiate(boss, spawnPosition, Quaternion.identity);
        Debug.Log("生成年兽");

        // 配置移动参数
        NianMovement movement = nian.GetComponent<NianMovement>();
        if (movement != null)
        {
            movement.minY = -3f;      // 移动下界
            movement.maxY = 3f;       // 移动上界
            movement.moveSpeed = 2f;  // 移动速度
        }
        else
        {
            Debug.LogError("Nian对象缺少NianMovement组件！");
        }
    }

    /// <summary>
    /// 重置Boss生成状态（可在关卡重置时调用）
    /// </summary>
    public void ResetSpawner()
    {
        bossSpawned = false;
        currentActivationCode = activationCode;
    }
}