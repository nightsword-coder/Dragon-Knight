using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using Unity.Mathematics;
using Unity.VisualScripting;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject enemyPrefab;     // 拖入敌人预制体
    public GameObject boss;
    public float spawnInterval;
    public int maxEnemies;
    public float enemySpeed;

    [Header("敌人子弹设置")]
    public GameObject enemyBulletPrefab; // 拖入敌人子弹预制体

    [Header("生成区域")]
    public Vector2 spawnAreaCenter = Vector2.zero;
    public Vector2 spawnAreaSize = new Vector2(10, 5);

    [Header("销毁边界")]
    public float minX = -15f;
    public float maxX = 15f;
    public float minY = -10f;
    public float maxY = 10f;

    private int currentEnemies = 0;

    void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (ScoreManager.Instance.Level < 10)
        {
            if (currentEnemies < maxEnemies)
            {
                Vector3 spawnPos = new Vector3(
                    spawnAreaCenter.x + UnityEngine.Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                    spawnAreaCenter.y + UnityEngine.Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                    -0.98f
                );

                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.transform.localScale = new Vector3(
                    -Mathf.Abs(enemy.transform.localScale.x),
                    enemy.transform.localScale.y,
                    enemy.transform.localScale.z
                );

                // 初始化敌人移动逻辑
                EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
                enemySpeed = (float)(-3 - 0.2 * ScoreManager.Instance.Level);
                spawnInterval = (float)(1 - ScoreManager.Instance.Level / 20);
                maxEnemies = (int)(5 + ScoreManager.Instance.Level / 2);
                enemyCtrl.Initialize(this, enemySpeed, minX, maxX, minY, maxY);

                // 添加/初始化敌人射击逻辑（修改：不再设置player引用）
                EnemyShooting enemyShooting = enemy.GetComponent<EnemyShooting>();
                if (enemyShooting == null)
                    enemyShooting = enemy.AddComponent<EnemyShooting>();

                enemyShooting.bulletPrefab = enemyBulletPrefab;

                currentEnemies++;
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void OnEnemyDestroyed()
    {
        currentEnemies--;
    }
}