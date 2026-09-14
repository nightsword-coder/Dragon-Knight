using UnityEngine;

public enum BossPhase
{
    Phase1 = 1,
    Phase2 = 2,
    Phase3 = 3,
    Phase4 = 4,
    Dead = 5
}

[RequireComponent(typeof(NianMovement))]
[DefaultExecutionOrder(50)]
public class BossFsm : MonoBehaviour
{
    [Header("阶段血量阈值（占最大生命）")]
    [SerializeField, Range(0.05f, 0.95f)] float phase2Hp = 0.75f;
    [SerializeField, Range(0.05f, 0.95f)] float phase3Hp = 0.50f;
    [SerializeField, Range(0.05f, 0.95f)] float phase4Hp = 0.25f;

    [Header("一阶段：100°~240°，间隔 20°（8 向）")]
    [SerializeField] float phase1FromAngle = 100f;
    [SerializeField] float phase1ToAngle = 240f;
    [SerializeField] float phase1AngleStep = 20f;
    [SerializeField] float phase1FireRate = 1.5f;

    [Header("二/三阶段：140°~220°，间隔 20°（5 向）")]
    [SerializeField] float fiveWayFromAngle = 140f;
    [SerializeField] float fiveWayToAngle = 220f;
    [SerializeField] float fiveWayAngleStep = 20f;
    [SerializeField] float phase2FireRate = 1.2f;
    [SerializeField] float minionInterval = 3f;

    [Header("三阶段：变红 + 玩家受伤回血")]
    [SerializeField] float lifeStealPercent = 0.10f;
    [SerializeField] Color enragedColor = new Color(1f, 0.32f, 0.32f, 1f);
    [SerializeField] float phase3FireRate = 1f;

    [Header("四阶段：后段强敌")]
    [SerializeField] int phase4BurstCount = 8;
    [SerializeField] int phase4WaveCount = 4;
    [SerializeField] float phase4WaveInterval = 3.5f;

    public BossPhase CurrentPhase { get; private set; } = BossPhase.Phase1;

    NianMovement boss;
    EnemyShooting shooting;
    SpriteRenderer sprite;
    Color baseColor = Color.white;
    EnemySpawner spawner;
    float minionTimer;
    float waveTimer;
    bool phase4Locked;

    void Awake()
    {
        boss = GetComponent<NianMovement>();
        shooting = GetComponent<EnemyShooting>();
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            baseColor = sprite.color;
    }

    void OnEnable()
    {
        if (boss != null)
            boss.HealthChanged += OnHealthChanged;
        PlayerHealth.Damaged += OnPlayerDamaged;
    }

    void OnDisable()
    {
        if (boss != null)
            boss.HealthChanged -= OnHealthChanged;
        PlayerHealth.Damaged -= OnPlayerDamaged;
        ApplyEnraged(false);
    }

    void Start()
    {
        spawner = EnemySpawner.FindPrimary();
        ChangePhase(BossPhase.Phase1, true);
    }

    void Update()
    {
        if (CurrentPhase == BossPhase.Dead)
            return;

        if (CurrentPhase == BossPhase.Phase2)
            TickRandomMinions();
        else if (CurrentPhase == BossPhase.Phase4)
            TickLateWaves();
    }

    void OnHealthChanged()
    {
        if (CurrentPhase == BossPhase.Dead || boss == null)
            return;

        if (boss.BossHealth <= 0)
        {
            ChangePhase(BossPhase.Dead);
            return;
        }

        BossPhase next = EvaluatePhase(boss.HealthNormalized);
        if (next != CurrentPhase)
            ChangePhase(next);
    }

    BossPhase EvaluatePhase(float hp)
    {
        if (phase4Locked)
            return BossPhase.Phase4;
        if (hp <= phase4Hp)
            return BossPhase.Phase4;
        if (hp <= phase3Hp)
            return BossPhase.Phase3;
        if (hp <= phase2Hp)
            return BossPhase.Phase2;
        return BossPhase.Phase1;
    }

    void ChangePhase(BossPhase next, bool force = false)
    {
        if (!force && next == CurrentPhase)
            return;

        ExitPhase(CurrentPhase);
        CurrentPhase = next;
        EnterPhase(next);
    }

    void EnterPhase(BossPhase phase)
    {
        minionTimer = 0f;
        waveTimer = 0f;

        switch (phase)
        {
            case BossPhase.Phase1:
                ApplyFan(phase1FromAngle, phase1ToAngle, phase1AngleStep, phase1FireRate);
                break;
            case BossPhase.Phase2:
                ApplyFan(fiveWayFromAngle, fiveWayToAngle, fiveWayAngleStep, phase2FireRate);
                break;
            case BossPhase.Phase3:
                ApplyFan(fiveWayFromAngle, fiveWayToAngle, fiveWayAngleStep, phase3FireRate);
                ApplyEnraged(true);
                break;
            case BossPhase.Phase4:
                phase4Locked = true;
                SetShootingEnabled(false);
                SpawnLateBurst();
                break;
            case BossPhase.Dead:
                SetShootingEnabled(false);
                ApplyEnraged(false);
                break;
        }
    }

    void ExitPhase(BossPhase phase)
    {
        if (phase == BossPhase.Phase3)
            ApplyEnraged(false);
    }

    void ApplyFan(float fromAngle, float toAngle, float step, float fireRate)
    {
        SetShootingEnabled(true);
        if (shooting != null)
            shooting.SetFanPattern(fromAngle, toAngle, step, fireRate);
    }

    void SetShootingEnabled(bool enabled)
    {
        if (shooting != null)
            shooting.enabled = enabled;
    }

    void ApplyEnraged(bool enraged)
    {
        if (sprite != null)
            sprite.color = enraged ? enragedColor : baseColor;
    }

    void TickRandomMinions()
    {
        if (spawner == null)
            return;

        minionTimer += Time.deltaTime;
        if (minionTimer < minionInterval)
            return;

        minionTimer = 0f;
        spawner.SpawnRandomMinion();
    }

    void TickLateWaves()
    {
        if (spawner == null)
            return;

        waveTimer += Time.deltaTime;
        if (waveTimer < phase4WaveInterval)
            return;

        waveTimer = 0f;
        spawner.SpawnLateEnemies(phase4WaveCount);
    }

    void SpawnLateBurst()
    {
        if (spawner == null)
            spawner = EnemySpawner.FindPrimary();
        if (spawner != null)
            spawner.SpawnLateEnemies(phase4BurstCount);
    }

    void OnPlayerDamaged()
    {
        if (CurrentPhase != BossPhase.Phase3 || boss == null)
            return;

        boss.HealByPercent(lifeStealPercent);
    }
}
