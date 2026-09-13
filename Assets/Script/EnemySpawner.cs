using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FormationType
{
    Random,   // 随机位置散开（暂不使用，一律按阵型刷）
    Column,   // 竖列齐刷
    Fan,      // 3人扇形 "<"
    Circle,   // 4人圆形
    Serpent   // 蛇形：上下摆动前进
}

[System.Serializable]
public class FormationConfig
{
    public string name = "阵型";
    public FormationType type = FormationType.Column;

    // 【关键属性 1：生成数量】本波阵型打算刷几只敌人。
    // 扇形固定 3、圆形固定 4；实际数量还会被 maxEnemies - currentEnemies 卡住。
    [Min(1)] public int spawnCount = 5;

    [Min(0.1f)] public float intervalAfter = 2f;

    // 【关键属性 2：彼此间距】竖列 / 扇形 / 蛇形里相邻敌人的纵向间距。
    [Tooltip("竖列、扇形、蛇形时的间距")]
    public float columnSpacing = 2f;

    [Tooltip("圆形阵型半径")]
    public float circleRadius = 1.5f;

    [Tooltip("蛇形上下摆动幅度")]
    public float snakeAmplitude = 1.5f;

    [Tooltip("蛇形上下摆动频率")]
    public float snakeFrequency = 2.5f;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    public List<GameObject> enemyPrefab;     // 越靠后越强
    public GameObject boss;
    public float spawnInterval;
    public int maxEnemies;
    public float enemySpeed;

    [Header("阵型列表")]

    public List<FormationConfig> formations = new List<FormationConfig>();

    [Header("敌人子弹设置")]
    public GameObject enemyBulletPrefab;

    private Vector2 spawnAreaCenter = new Vector2(7.5f,0);
    private Vector2 spawnAreaSize = new Vector2(1, 8);

    //销毁边界
    private float minX = -8f;
    private float maxX = 10f;
    private float minY = -7f;
    private float maxY = 3.2f;

    private int currentEnemies = 0;
    private const float SpawnZ = -1f;

    void Start()
    {
        EnsureDefaultFormations();
        PrewarmPools();
        StartCoroutine(SpawnEnemies());
    }

    void PrewarmPools()
    {
        if (enemyPrefab != null)
        {
            for (int i = 0; i < enemyPrefab.Count; i++)
                ObjectPool.Prewarm(enemyPrefab[i], 8);
        }

        ObjectPool.Prewarm(enemyBulletPrefab, 20);
    }

    void EnsureDefaultFormations()
    {
        if (formations == null)
            formations = new List<FormationConfig>();

        if (formations.Count == 0)
        {
            formations.AddRange(new[]
            {
                new FormationConfig { name = "竖列突击", type = FormationType.Column, spawnCount = 4, intervalAfter = 1.8f, columnSpacing = 1.5f },
                new FormationConfig { name = "竖列突击", type = FormationType.Column, spawnCount = 5, intervalAfter = 2.4f, columnSpacing = 1.5f },
                new FormationConfig { name = "竖列突击", type = FormationType.Column, spawnCount = 6, intervalAfter = 1.6f, columnSpacing = 1.5f },
                new FormationConfig { name = "竖列突击", type = FormationType.Column, spawnCount = 7, intervalAfter = 2.6f, columnSpacing = 1.5f },
            });
        }

        EnsureFormationType(FormationType.Fan, new FormationConfig
        {
            name = "扇形突击", type = FormationType.Fan, spawnCount = 3, intervalAfter = 2f, columnSpacing = 1.5f
        });
        EnsureFormationType(FormationType.Circle, new FormationConfig
        {
            name = "圆形围攻", type = FormationType.Circle, spawnCount = 4, intervalAfter = 2.2f, circleRadius = 1.5f
        });
        EnsureFormationType(FormationType.Serpent, new FormationConfig
        {
            name = "蛇形游走", type = FormationType.Serpent, spawnCount = 5, intervalAfter = 2.4f,
            columnSpacing = 1.2f, snakeAmplitude = 1.5f, snakeFrequency = 2.5f
        });
    }

    void EnsureFormationType(FormationType type, FormationConfig fallback)
    {
        for (int i = 0; i < formations.Count; i++)
        {
            if (formations[i].type == type)
                return;
        }
        formations.Add(fallback);
    }

    IEnumerator SpawnEnemies()
    {
        int waveIndex = 0;

        while (ScoreManager.Instance.Level < 10)
        {
            RefreshLevelParams();

            FormationConfig wave = formations[waveIndex % formations.Count];
            int desired = GetFormationCount(wave);
            int available = Mathf.Max(0, maxEnemies - currentEnemies);
            int count = desired;

            if (wave.type == FormationType.Fan || wave.type == FormationType.Circle)
            {
                if (available < desired)
                    count = 0;
            }
            else
            {
                count = Mathf.Min(desired, available);
            }

            if (count > 0)
            {
                SpawnFormation(wave, count);
                waveIndex++;
            }

            float wait = wave.intervalAfter * Mathf.Max(0.35f, 1f - (float)ScoreManager.Instance.Level / 20f);
            yield return new WaitForSeconds(wait);
        }
    }

    int GetFormationCount(FormationConfig wave)
    {
        if (wave.type == FormationType.Fan)
            return 3;
        if (wave.type == FormationType.Circle)
            return 4;
        return wave.spawnCount;
    }

    void RefreshLevelParams()
    {
        double level = ScoreManager.Instance.Level;
        enemySpeed = (float)(-3 - 0.2 * level);
        spawnInterval = (float)(1 - level / 20);
        maxEnemies = (int)(5 + level / 2);
    }

    void SpawnFormation(FormationConfig wave, int count)
    {
        GameObject prefab = PickEnemyPrefab();

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetFormationPosition(wave, i, count);
            SpawnOne(prefab, pos, wave, i);
        }
    }

    Vector3 GetFormationPosition(FormationConfig wave, int index, int count)
    {
        switch (wave.type)
        {
            case FormationType.Fan:
                return GetFanPosition(index, wave.columnSpacing);
            case FormationType.Circle:
                return GetCirclePosition(index, wave.circleRadius);
            case FormationType.Serpent:
                return GetSerpentPosition(index, count, wave.columnSpacing);
            default:
                return GetColumnPosition(index, count, wave.columnSpacing);
        }
    }

    Vector3 GetRandomPosition()
    {
        return new Vector3(
            spawnAreaCenter.x + Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            spawnAreaCenter.y + Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f),
            SpawnZ
        );
    }

    Vector3 GetColumnPosition(int index, int count, float spacing)
    {
        if (count > 1)
        {
            float maxHeight = Mathf.Max(0.5f, spawnAreaSize.y);
            float needed = (count - 1) * spacing;
            if (needed > maxHeight)
                spacing = maxHeight / (count - 1);
        }

        float x = spawnAreaCenter.x + spawnAreaSize.x * 0.5f;
        float totalHeight = (count - 1) * spacing;
        float y = spawnAreaCenter.y + totalHeight * 0.5f - index * spacing;
        return new Vector3(x, y, SpawnZ);
    }

    Vector3 GetFanPosition(int index, float spread)
    {
        float backX = spawnAreaCenter.x + spawnAreaSize.x * 0.5f;
        float tipX = backX - spread;
        float midY = spawnAreaCenter.y;

        if (index == 0)
            return new Vector3(backX, midY + spread, SpawnZ);
        if (index == 1)
            return new Vector3(tipX, midY, SpawnZ);
        return new Vector3(backX, midY - spread, SpawnZ);
    }

    Vector3 GetCirclePosition(int index, float radius)
    {
        float angle = index * 90f * Mathf.Deg2Rad;
        return new Vector3(
            spawnAreaCenter.x + Mathf.Cos(angle) * radius,
            spawnAreaCenter.y + Mathf.Sin(angle) * radius,
            SpawnZ
        );
    }

    Vector3 GetSerpentPosition(int index, int count, float spacing)
    {
        Vector3 pos = GetColumnPosition(index, count, spacing);
        pos.x += Mathf.Sin(index * 0.8f) * 0.4f;
        return pos;
    }

    /// <summary>
    /// Level 越高越强：小部分从前段弱怪抽，大部分从已解锁后段强怪抽。
    /// </summary>
    GameObject PickEnemyPrefab()
    {
        int n = enemyPrefab != null ? enemyPrefab.Count : 0;
        if (n <= 0)
        {
            Debug.LogError("enemyPrefab 列表为空");
            return null;
        }
        if (n == 1)
            return enemyPrefab[0];

        float t = Mathf.Clamp01((float)((ScoreManager.Instance.Level - 1) / 9.0));
        int unlocked = Mathf.Clamp(Mathf.CeilToInt(Mathf.Lerp(2, n, t)), 1, n);

        int index;
        if (Random.value < 0.25f)
        {
            int weakEnd = Mathf.Max(1, unlocked / 3);
            index = Random.Range(0, weakEnd);
        }
        else
        {
            int strongStart = unlocked / 3;
            index = Random.Range(strongStart, unlocked);
        }

        return enemyPrefab[index];
    }

    void SpawnOne(GameObject prefab, Vector3 spawnPos, FormationConfig wave, int index)
    {
        if (prefab == null)
            return;

        GameObject enemy = ObjectPool.Get(prefab, spawnPos, Quaternion.identity, false);
        enemy.transform.localScale = new Vector3(
            -Mathf.Abs(enemy.transform.localScale.x),
            enemy.transform.localScale.y,
            enemy.transform.localScale.z
        );

        EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
        if (enemyCtrl != null)
        {
            enemyCtrl.Initialize(this, enemySpeed, minX, maxX, minY, maxY);
            if (wave != null && wave.type == FormationType.Serpent)
                enemyCtrl.EnableSnakeMove(index * 0.9f, wave.snakeAmplitude, wave.snakeFrequency);
        }

        EnemyShooting enemyShooting = enemy.GetComponent<EnemyShooting>();
        if (enemyShooting != null && enemyShooting.bulletPrefab == null)
            enemyShooting.bulletPrefab = enemyBulletPrefab;

        currentEnemies++;
    }

    public void OnEnemyDestroyed()
    {
        currentEnemies--;
    }
}
