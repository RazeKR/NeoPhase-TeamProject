using System;
using System.Collections;
using UnityEngine;

public abstract class CEnemyBase : CEntityBase
{
    #region 인스펙터
    [Header("적 데이터")]
    [SerializeField] private CEnemyDataSO _enemyData;
    #endregion

    #region 이벤트
    /// <summary>
    /// 사망 애니메이션 완료 후 발행 — CSpawnManager가 구독하여 풀 반환 처리
    /// </summary>
    public event Action<CEnemyBase> OnDied;
    #endregion

    #region 내부 변수
    private static readonly int _hashDead = Animator.StringToHash("tDead");

    private float      _lastAttackTime = 0f;
    private Transform  _playerTransform;  // 스폰 시 주입된 플레이어 Transform
    protected Animator _animator;
    private bool       _isDead;
    private Collider2D _selfCollider;     // 피격 FX 위치 계산용 콜라이더 캐시
    private int        _lastInitializedStage = 0; // 마지막으로 InitEnemy가 호출된 스테이지 번호
    #endregion

    #region 프로퍼티
    protected CEnemyDataSO EnemyData => _enemyData;
    protected IDamageable TargetDamageable { get; private set; }
    public float AttackDamage { get; protected set; }
    public float AttackCooltime { get; protected set; }
    public float AttackRange { get; protected set; }
    public float LastAttackTime { get; set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        _animator     = GetComponentInChildren<Animator>();
        _selfCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// 피격 방향 정보를 포함한 데미지 처리 — 기본 연출(HitFlash, 데미지 텍스트)에 더해
    /// 적 전용 HitImpact 스프라이트 이펙트를 피격 위치에 표시한다
    /// </summary>
    public override void TakeDamage(float damage, Vector2 hitDir)
    {
        base.TakeDamage(damage, hitDir);

        if (_enemyData != null && _enemyData.HitSFX != null)
            CAudioManager.Instance?.Play(_enemyData.HitSFX, transform.position);

        // 콜라이더 평균 반경 계산 (원형·박스 모두 대응)
        float radius = _selfCollider != null
            ? (_selfCollider.bounds.extents.x + _selfCollider.bounds.extents.y) * 0.5f
            : 0.3f;

        CHitImpactPoolManager.Instance?.ShowHitImpact(transform.position, hitDir, radius);

        ApplyKnockback(hitDir * 3f, 0.2f);
    }

    /// <summary>
    /// 처치 시 확정 지급할 골드를 SO의 min~max 범위에서 랜덤하게 반환합니다.
    /// CSpawnManager가 킬 처리 시 호출합니다.
    /// </summary>
    public int GetGoldDrop() => _enemyData != null ? _enemyData.GetRandomGoldDrop() : 0;

    public int GetExpReward() => _enemyData != null ? _enemyData.ExpReward : 0;

    /// <summary>
    /// 스캔 없이 플레이어를 직접 타겟으로 설정 — CSpawnManager가 스폰 시 호출
    /// </summary>
    public void SetTarget(Transform player)
    {
        _playerTransform = player;
        CurrentTarget   = player;

        if (player != null)
        {
            TargetDamageable = player.GetComponent<IDamageable>();
        }
    }

    /// <summary>
    /// 스캔 방식 대신 주입된 플레이어 Transform을 항상 타겟으로 사용
    /// 사망 처리 중(_isDead)에는 이동·공격을 모두 중단한다
    /// </summary>
    protected override void FixedUpdate()
    {
        if (_isDead) return;
        CurrentTarget = _playerTransform; // 스캔 없이 직접 추적
        HandleMovement();
        HandleAttack();
    }

    protected virtual void Update()
    {
        if (_animator == null) return;
        // 빙결 상태이면 애니메이션 정지, 해제되면 복구
        _animator.speed = HasStatus(EStatusEffect.Freeze) ? 0f : 1f;
    }

    protected virtual void Start()
    {
        if (_enemyData == null)
            CDebug.LogWarning($"{gameObject.name} EnemyDataSO 없음. 참조 확인");
    }

    /// <summary>
    /// 스폰 시 스테이지에 맞게 스탯 초기화.
    /// 이전에 초기화된 이력이 있으면 기존 스탯에 스테이지 차이만큼만 추가하여 이월한다.
    /// </summary>
    /// <param name="currentStage">현재 스테이지 (1-based)</param>
    public virtual void InitEnemy(int currentStage)
    {
        if (_enemyData == null) return;

        MoveSpeed      = _enemyData.MoveSpeed;
        AttackRange    = _enemyData.AttackRange;
        AttackCooltime = _enemyData.AttackCooltime;

        if (_lastInitializedStage == 0)
        {
            // 최초 스폰: 현재 스테이지 기준으로 처음부터 계산
            int growthMultiplier = Mathf.Max(1, currentStage) - 1;
            MaxHealth    = _enemyData.BaseHealth + (_enemyData.HealthGrowthPerStage * growthMultiplier);
            AttackDamage = _enemyData.BaseDamage  + (_enemyData.DamageGrowthPerStage * growthMultiplier);
        }
        else if (currentStage > _lastInitializedStage)
        {
            // 스테이지 이월: 이전 스탯에 차이만큼만 누적
            int stageDelta = currentStage - _lastInitializedStage;
            MaxHealth    += _enemyData.HealthGrowthPerStage * stageDelta;
            AttackDamage += _enemyData.DamageGrowthPerStage * stageDelta;
        }
        // currentStage == _lastInitializedStage: 같은 스테이지 재스폰, 스탯 변경 없음

        CurrentHealth         = MaxHealth;
        _lastInitializedStage = currentStage;
    }

    /// <summary>
    /// 풀 반환 시 호출 — 상태 초기화
    /// </summary>
    public virtual void ResetForPool()
    {
        _isDead            = false;
        CurrentTarget      = null;
        _lastAttackTime    = 0f;
        Rb.velocity        = Vector2.zero;
        Rb.constraints     = RigidbodyConstraints2D.FreezeRotation; // 사망 시 FreezeAll 복구
        CurrentHealth      = MaxHealth;

        ClearAllStatuses();
    }

    /// <summary>
    /// 사망 처리 — Dead 애니메이션 재생 후 OnDied 발행, 실제 비활성화는 CSpawnManager가 처리
    /// </summary>
    public override void Die()
    {
        if (_isDead) return; // 중복 호출 방지
        _isDead = true;

        if (_enemyData?.DieSFX != null)
            CAudioManager.Instance?.Play(_enemyData.DieSFX, transform.position);

        if (CJsonManager.Instance != null)
            CJsonManager.Instance.AddTotalKillCount(1);

        if (Rb != null && Rb.bodyType != RigidbodyType2D.Static)
        {
            Rb.velocity    = Vector2.zero;
            Rb.constraints = RigidbodyConstraints2D.FreezeAll; // 사망 중 물리 이동 완전 차단
        }

        if (_animator != null && UseDeathAnimation())
            StartCoroutine(Co_DeathAnimation());
        else
            OnDied?.Invoke(this);
    }

    /// <summary>
    /// Dead 트리거 발동 → 전환 완료 대기 → 애니메이션 길이만큼 대기 → 풀 반환 이벤트 발행
    /// </summary>
    private IEnumerator Co_DeathAnimation()
    {
        _animator.SetTrigger(_hashDead);
        yield return null; // 트리거 처리 한 프레임 대기
        yield return new WaitUntil(() => !_animator.IsInTransition(0)); // Dead 상태 진입 대기

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);

        OnDied?.Invoke(this);
    }

    protected override void HandleMovement()
    {
        if (HasStatus(EStatusEffect.Knockback)) return;

        if (CurrentTarget == null)
        {
            Rb.velocity = Vector2.zero;
            return;
        }

        float distance    = Vector2.Distance(transform.position, CurrentTarget.position);
        Vector2 dirToPlayer = (CurrentTarget.position - transform.position).normalized;

        FlipCharacter(dirToPlayer.x);

        if (distance > AttackRange * 0.8f)
            Rb.velocity = dirToPlayer * CurrentMoveSpeed;
        else
            Rb.velocity = Vector2.zero;
    }

    protected override void HandleAttack()
    {
        if (CurrentTarget == null) return;

        float distance = Vector2.Distance(transform.position, CurrentTarget.position);

        if (distance <= AttackRange && Time.time >= _lastAttackTime + AttackCooltime)
        {
            _lastAttackTime = Time.time;
            ExecuteAttack();
        }
    }

    /// <summary>
    /// 실제 공격 실행 — 서브클래스에서 구현
    /// </summary>
    protected abstract void ExecuteAttack();

    /// <summary>
    /// 사망 애니메이션 재생 여부 — 자폭처럼 별도 연출이 있는 경우 false로 오버라이드
    /// </summary>
    protected virtual bool UseDeathAnimation() => true;

    public override void ApplyFreeze(float duration, float slowAmount)
    {
        if (CAudioManager.Instance != null)
        {
            CAudioManager.Instance.Play(_enemyData.FreezeSFX, transform.position);
        }

        base.ApplyFreeze(duration, slowAmount);
    }

    // 화상 상태이상 중 활성화할 FogLightSource (동적 생성)
    private CFogLightSource _burnLight;

    public override void ApplyBurn(float duration, float tickDamage, float tickInterval)
    {
        base.ApplyBurn(duration, tickDamage, tickInterval);
        StartCoroutine(CoBurnLightRoutine(duration));
    }

    private IEnumerator CoBurnLightRoutine(float duration)
    {
        // 이미 붙어있으면 제거 후 재생성 (burn 갱신 시)
        if (_burnLight != null)
        {
            _burnLight.gameObject.SetActive(false);
            Destroy(_burnLight.gameObject);
        }

        GameObject lightObj = new GameObject("BurnFogLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        _burnLight = lightObj.AddComponent<CFogLightSource>();
        _burnLight.SetOuterRadius(2.5f);
        _burnLight.SetInnerRatio(0.4f);
        _burnLight.SetIntensity(0.5f);

        yield return new WaitForSeconds(duration);

        if (_burnLight != null)
        {
            _burnLight.gameObject.SetActive(false);
            Destroy(_burnLight.gameObject);
            _burnLight = null;
        }
    }
}
