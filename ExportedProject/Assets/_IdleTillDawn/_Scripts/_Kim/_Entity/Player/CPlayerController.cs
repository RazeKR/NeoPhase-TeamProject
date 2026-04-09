using System;
using System.Collections;
using UnityEngine;

public class CPlayerController : CEntityBase, IHealable
{
    #region 인스펙터
    [Header("캐릭터 참조")]
    [SerializeField] private Animator _animator;

    [Header("자동 이동 딜레이")]
    [SerializeField] private float _autoModeDelay = 3.0f;

    [Header("자동 회피 옵션")]
    [SerializeField] private float _evadeRadius = 2.5f;
    [SerializeField] private LayerMask _hazardLayer;

    [Header("애니메이터 파라미터")]
    [SerializeField] private string _paramSpeed = "aSpeed";

    [Header("공격 기본 옵션")]
    [SerializeField] private float _defaultAttackRange = 0.1f;
    //[SerializeField] private float _bulletSpeed = 10f;
    // SO -> ProjectileSpeed

    [Header("테스트용 무기 직접 참조")]
    [Tooltip("true로 켜면 CInventoryManager를 무시하고 아래 SO를 직접 사용합니다.\n인스펙터에서 FireRate 등을 바로 조정할 때 사용하세요.")]
    [SerializeField] private bool _useTestWeaponOverride = false;
    [SerializeField] private CWeaponInstance _testWeaponData = null;

    [Header("무기 오브젝트")]
    [SerializeField] private GameObject _weaponVisualObj;

    [Header("피격 연출 옵션")]
    [SerializeField] private float _preventTime = 1.5f;
    [SerializeField] private float _blinkInterval = 0.1f;
    #endregion

    #region 내부 변수
    private CPlayerDataSO _characterData;

    private float _lastAttackTime = 0f;
    private int   _hashSpeed;

    private CPlayerStatManager _statManager;

    private bool       _isPreventDamage    = false;
    private Coroutine  _preventCoroutine   = null; // 코루틴 참조 — 중단/재시작 안전 관리용
    private SpriteRenderer _spriteRenderer;
    private WaitForSeconds _blinkWait;
    private float _knockbackEndTime = 0f;

    private bool _isWeaponDisabled = false;

    private float _traitSpeedMultiplier = 1.0f;

    private float _bulletScaleBonus;    // for Passive Skill

    private int _meleeSpinCount;        // for Melee Weapon Special Attack Counting

    #endregion

    #region 프로퍼티
    public float EvadeRadius => _evadeRadius;
    public LayerMask HazardLayer => _hazardLayer;
    public float AutoModeDelay => _autoModeDelay;
    public CPlayerInputHandler InputHandler => CPlayerInputHandler.Instance;

    public RuntimeAnimatorController CurrentAnimator => _animator != null ? _animator.runtimeAnimatorController : null;
    public bool IsWeaponDisabled
    {
        get => _isWeaponDisabled;
        set
        {
            _isWeaponDisabled = value;

            if (_weaponVisualObj != null)
            {
                _weaponVisualObj.SetActive(!_isWeaponDisabled);
            }
        }
    }
    public bool IsAutoEvadeDisabled { get; set; } = false;

    // FSM 관련 변수
    public CPlayerStateMachine StateMachine { get; private set; }
    public CStateManual StateManual { get; private set; }
    public CStateAutoChase StateAutoChase { get; private set; }
    public CStateAutoEvade StateAutoEvade { get; private set; }

    public float BulletScaleBonus { get{ return _bulletScaleBonus; } set { _bulletScaleBonus = value; } } // for Passive Skill

    #endregion

    #region Event
    /// <summary>Damaged Event(for Passive Skill) </summary>
    public event System.Action OnDamaged;
    /// <summary>Shot Event(for Passive Skill) </summary>
    public event System.Action OnShot;

    public event Action OnPlayerAttack;
    #endregion

    /// <summary>
    /// 장착 무기의 사거리 — 무기 미장착 시 기본값 반환
    /// </summary>
    public float CurrentAttackRange
    {
        get
        {
            if (IsWeaponDisabled) return _defaultAttackRange;

            CWeaponDataSO data = GetEquippedWeaponData()._data;
            return data != null ? data.WeaponRange : _defaultAttackRange;
        }
    }

    // CurrentAttackSpeed는 외부 참조용으로 유지 (weaponData를 이미 가진 HandleAttack 내부에서는 사용 안 함)
    public float CurrentAttackSpeed
    {
        get
        {
            CWeaponDataSO data = GetEquippedWeaponData()._data;
            return BaseAttackSpeed * (data != null ? data.WeaponFireRate : 1.0f);
        }
    }

    /// <summary>
    /// 플레이어 기본 공격 속도 (스탯 매니저 기준, 없으면 1.0).
    /// HandleAttack()에서 WeaponFireRate와 곱해 최종 발사 간격을 계산한다.
    /// </summary>
    private float BaseAttackSpeed =>
        _statManager != null ? _statManager.GetFinalStat(EPlayerStatType.AttackSpeed) : 1.0f;

    public Vector3 PlayerLocalScale
    {
        get
        {
            return transform.localScale;
        }
        set
        {
            transform.localScale = value;
        }
    }

    /// <summary>
    /// 외부에서 넉백을 가하는 메서드 (전기벽 등)
    /// duration 동안 플레이어 이동 입력을 무시하고 velocity를 유지한다
    /// </summary>
    public override void ApplyKnockback(Vector2 force, float duration)
    {
        if (HasStatus(EStatusEffect.Knockback)) return;

        AddStatus(EStatusEffect.Knockback);
        Rb.velocity       = force;
        _knockbackEndTime = Time.fixedTime + duration;
    }

    protected override void Awake()
    {
        base.Awake();

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_statManager == null)
        {
            _statManager = GetComponent<CPlayerStatManager>();
        }

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        _hashSpeed = Animator.StringToHash(_paramSpeed);        
        _blinkWait = new WaitForSeconds(_blinkInterval);

        StateMachine = new CPlayerStateMachine();
        StateManual = new CStateManual(this);
        StateAutoChase = new CStateAutoChase(this);
        StateAutoEvade = new CStateAutoEvade(this);

        StateMachine.Initialize(StateManual);
    }

    private void OnEnable()
    {
        // 재활성화 시 무적 상태 강제 초기화
        // 코루틴이 외부 요인(씬 이벤트, OnDisable 등)으로 중단되면
        // _isPreventDamage = false 가 실행되지 않아 영구 고착될 수 있으므로 여기서 보장한다
        _isPreventDamage  = false;
        _preventCoroutine = null;

        IsWeaponDisabled = false;
        IsAutoEvadeDisabled = false;

        if (InputHandler != null)
        {
            InputHandler.OnSkillInput += ExecuteManualSkill;
            InputHandler.OnItemUseInput += ExecuteItemUse;
        }

        if (_statManager != null)
        {
            _statManager.OnLevelUp += HandleLevelUp;
            _statManager.OnStatUpgraded += RefreshStats;
        }
    }

    protected override void OnDisable()
    {
        if (InputHandler != null)
        {
            InputHandler.OnSkillInput -= ExecuteManualSkill;
            InputHandler.OnItemUseInput -= ExecuteItemUse;
        }

        if (_statManager != null)
        {
            _statManager.OnLevelUp -= HandleLevelUp;
            _statManager.OnStatUpgraded -= RefreshStats;
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, CurrentAttackRange);
    }

    private void Start()
    {
        if (CJsonManager.Instance != null)
        {
            CSaveData loadedData = CJsonManager.Instance.GetOrCreateSaveData();

            InitPlayer(loadedData);
        }
        else
        {
            InitPlayer(null);
        }

        if (CGameManager.Instance != null)
        {
            CGameManager.Instance.RegisterPlayer(this, _statManager);
        }

        StartCoroutine(CoRegenLoop());
    }

    private IEnumerator CoRegenLoop()
    {
        WaitForSeconds interval = new WaitForSeconds(10f);

        while (true)
        {
            yield return interval;

            if (CurrentHealth <= 0) continue;

            float hpRegen = _statManager.GetFinalStat(EPlayerStatType.HealthRegen);
            if (hpRegen > 0f)
                Heal(hpRegen);

            float mpRegen = _statManager.GetFinalStat(EPlayerStatType.ManaRegen);
            if (mpRegen > 0f)
                _statManager.RestoreMana(mpRegen);
        }
    }

    private void Update()
    {
        // 테스트용 피격
        if (Input.GetKeyDown(KeyCode.O))
        {
            TakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log($"현재 마나 : {_statManager.CurrentMana}");
        }

        if (StateMachine != null)
        {
            StateMachine.Update();
        }
    }

    protected override void FixedUpdate()
    {
        FindNearestTarget();

        base.FixedUpdate();
    }

    /// <summary>
    /// 세이브 데이터와 SO 데이터를 조합해 캐릭터 초기화
    /// </summary>
    /// <param name="saveData">세이브 데이터 (나중에 병합 시 맞는 타입으로 변경)</param>
    public void InitPlayer(CSaveData saveData)
    {
        int playerId = (saveData != null && saveData.playerStatId >= 0) ? saveData.playerStatId : 0;

        if (CDataManager.Instance != null)
        {
            _characterData = CDataManager.Instance.GetPlayerData(playerId);
        }

        if (_characterData == null)
        {
            Debug.LogError($"CPlayerController : {playerId}의 플레이어 데이터 로드 실패");
            return;
        }

        if (_statManager != null)
        {
            _statManager.InitBaseData(_characterData);

            if (saveData != null && !string.IsNullOrEmpty(saveData.uid))
            {
                _statManager.SyncWithSaveData(_characterData, saveData);
            }

            MaxHealth = _statManager.GetFinalStat(EPlayerStatType.Health);
            MoveSpeed = _statManager.GetFinalStat(EPlayerStatType.MoveSpeed);
        }
        else
        {
            Debug.LogWarning("_statManager가 null입니다. 기본 데이터로 초기화합니다.");
            MaxHealth = _characterData.GetStatInfo(EPlayerStatType.Health).BaseValue;
            MoveSpeed = _characterData.GetStatInfo(EPlayerStatType.MoveSpeed).BaseValue;
        }

        CurrentHealth = (saveData != null && saveData.currentHp > 0) ? saveData.currentHp : MaxHealth;

        Debug.Log($"{gameObject.name} 초기화 완료, 현재 체력 : {CurrentHealth}, 최대 체력 {MaxHealth}");
    }

    /// <summary>
    /// 플레이어 이동 메서드
    /// </summary>
    protected override void HandleMovement()
    {
        if (HasStatus(EStatusEffect.Knockback))
        {
            if (Time.fixedTime < _knockbackEndTime) return;

            RemoveStatus(EStatusEffect.Knockback);
            Rb.velocity = Vector2.zero;
        }

        StateMachine.FixedUpdate();

        float speed = Rb.velocity.magnitude;
        _animator.SetFloat(_hashSpeed, speed);
        FlipCharacter(Rb.velocity.x);
    }

    /// <summary>
    /// 플레이어 공격 메서드.
    /// 가장 가까운 적이 사거리 안에 있을 때 CWeaponDataSO.WeaponFireRate(초당 발사 횟수)에 따라 투사체를 발사한다.
    /// ※ FireRate 설정 위치: CWeaponDataSO 에셋의 WeaponFireRate 필드
    ///   - 1.0 = 초당 1발 / 3.0 = 초당 3발 / 0.5 = 2초에 1발
    /// ※ 테스트 씬에서 CInventoryManager 없이 테스트하려면
    ///   P_Hastur 프리팹의 CPlayerController > _testWeaponData 에 SO를 직접 연결하세요.
    /// </summary>
    protected override void HandleAttack()
    {
        if (IsWeaponDisabled) return;

        if (CurrentTarget == null) return;

        // 무기 데이터를 한 번만 가져온다 (이후 재호출 없음)
        CWeaponInstance weaponInstance = GetEquippedWeaponData();
        CWeaponDataSO weaponData = weaponInstance._data;
        if (weaponInstance == null || weaponData.BulletPrefab == null) return;

        float distance = Vector2.Distance(transform.position, CurrentTarget.position);
        if (distance > CurrentAttackRange) return;

        // fireInterval: 최소 1 FixedUpdate 주기 보장 (interval < fixedDeltaTime이면 매 프레임 연사됨)
        float fireInterval = Mathf.Max(
            Time.fixedDeltaTime,
            1f / Mathf.Max(0.01f, BaseAttackSpeed * weaponData.WeaponFireRate)
        );
        // FixedUpdate에서는 Time.fixedTime 사용 (Time.time은 렌더 프레임 기준이라 오차 발생)
        if (Time.fixedTime < _lastAttackTime + fireInterval) return;

        _lastAttackTime = Time.fixedTime;

#if UNITY_EDITOR
        //Debug.Log($"[Attack] SO: {weaponData.name} | FireRate: {weaponData.WeaponFireRate} | Interval: {fireInterval:F3}s | Override: {_useTestWeaponOverride}");
#endif

        Vector2 dir = (CurrentTarget.position - transform.position).normalized;
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = CWeaponEquip.Instance != null
            ? CWeaponEquip.Instance.MuzzleWorldPosition
            : transform.position;

        int count = CWeaponEquip.Instance.GetProjectileAmount();

        float step = 15f;
        float startAngle = rotZ - (step * (count - 1) / 2f);

        for (int i = 0; i < count; i++)
        {
            float currenAngle = startAngle + i * step;
            Quaternion rot = Quaternion.Euler(0, 0, currenAngle);

            GameObject bulletObj = null;

            // melee bifurcation
            if (weaponData.IsMelee && _meleeSpinCount >= 2)
            {
                bulletObj = Instantiate(weaponData.MeleeSpin, spawnPos, rot);
                _meleeSpinCount = 0;
            }
            else
            {
                bulletObj = Instantiate(weaponData.BulletPrefab, spawnPos, rot);                
                _meleeSpinCount++;
            }

            float finalDamage = _statManager != null
                                          ? _statManager.GetFinalStat(EPlayerStatType.Damage) + weaponInstance.GetActualDamage()
                                          : weaponInstance.GetActualDamage();

            // flanne.Projectile 계열 투사체
            flanne.Projectile proj = bulletObj.GetComponent<flanne.Projectile>();
            if (proj != null)
            {
                proj.damage = finalDamage;
                proj.vector = (rot * Vector3.right) * weaponData.ProjectileSpeed;
                proj.owner = gameObject;
                proj.size = 1f + _bulletScaleBonus;

                // SO의 LifeTime으로 프리팹 내 TimeToLive를 덮어씀 (중복 제거)
                flanne.TimeToLive ttl = bulletObj.GetComponent<flanne.TimeToLive>();
                if (ttl != null)
                    ttl.SetLifetime(weaponData.LifeTime);
            }
        }

        

        // Swing when Melee weapon
        if (weaponData.IsMelee)
            CWeaponEquip.Instance?.PlaySwingAnimation();

        // 총구화염 재생
        CWeaponEquip.Instance?.ShowMuzzleFlash();

        // 발사 사운드 재생
        CAudioManager.Instance?.Play(weaponData.FireSFX, spawnPos);

        OnPlayerAttack?.Invoke();        

        OnShot?.Invoke();
    }

    /// <summary>
    /// 플레이어 사망 — 씬 리로드로 현재 스테이지 재시작
    /// </summary>
    public override void Die()
    {
        if (CJsonManager.Instance != null)
        {
            CSaveData currentData = CJsonManager.Instance.GetOrCreateSaveData();

            for (int i = 0; i < (int)EPlayerStatType.Count; i++)
            {
                float bonusValue = _statManager.BonusModifiers[i];
                if (bonusValue > 0)
                {
                    currentData.SetStatBonus(i, bonusValue);
                }
            }

            int currentStage = CGameManager.Instance != null ? CGameManager.Instance.CurrentStageIndex : 1;


            currentData.UpdateProgress
            (
                _statManager.CurrentLevel,
                _statManager.CurrentExp,
                MaxHealth,
                _statManager.GetFinalStat(EPlayerStatType.Mana),
                currentStage
            );

            CJsonManager.Instance.Save(currentData);
        }

        base.Die();

        if (CGameManager.Instance != null)
        {
            CGameManager.Instance.RespawnCurrentStage();
        }

    }

    /// <summary>
    /// 외부에서 캐릭터의 애니메이터를 교체할 수 있게 해주는 메서드
    /// </summary>
    /// <param name="newController"></param>
    public void ChangeAnimator(RuntimeAnimatorController newController)
    {
        if (_animator != null && newController != null)
        {
            _animator.runtimeAnimatorController = newController;
        }
    }

    public void SetTraitSpeedMultiplier(float multiplier)
    {
        _traitSpeedMultiplier = multiplier;

        RefreshStats();
    }

    /// <summary>
    /// 현재 장착된 무기 SO를 반환한다.
    /// _useTestWeaponOverride가 true이면 인벤토리를 무시하고 _testWeaponData를 반환한다.
    /// </summary>
    private CWeaponInstance GetEquippedWeaponData()
    {
        // 테스트 오버라이드가 켜져 있으면 인벤토리 완전 무시
        if (_useTestWeaponOverride)
            return _testWeaponData;

        if (CInventorySystemJ.Instance != null)
        {
            CWeaponInstance weapon = CInventorySystemJ.Instance.EquippedWeapon;
            if (weapon != null)
                return weapon;
        }

        return _testWeaponData;
    }

    /// <summary>
    /// 스킬 수동 사용
    /// </summary>
    /// <param name="index">스킬 슬롯 번호</param>
    private void ExecuteManualSkill(int index)
    {
        if (CSkillSystem.Instance != null)
        {
            CSkillSystem.Instance.UseBindSkill(index);
        }
        else
        {
            Debug.LogWarning("CSkillSystem이 씬에 없음");
        }
    }

    private void ExecuteItemUse(int index)
    {
        if (CInventorySystemJ.Instance != null)
        {
            CInventorySystemJ.Instance.UseBindPotion(index);
        }
        else
        {
            Debug.LogWarning("CInventorySystemJ이 씬에 없음");
        }
    }

    public override void TakeDamage(float damage)
    {
        if (HasStatus(EStatusEffect.Invincible)) return;

        if (_isPreventDamage) return;

        base.TakeDamage(damage);

        Debug.Log($"{gameObject.name} 데미지를 입음 : 데미지 {damage}");

        if (CurrentHealth <= 0) return; // 사망 처리는 Die()에 위임, 코루틴 불필요

        // 혹시 이전 코루틴이 살아있으면 중단 후 새로 시작
        // 정상 흐름에서는 _isPreventDamage 가드가 이중 실행을 막지만
        // 비정상 종료로 참조가 남은 경우를 방어한다
        if (_preventCoroutine != null) StopCoroutine(_preventCoroutine);
        _preventCoroutine = StartCoroutine(CoPreventDamage());

        OnDamaged?.Invoke();
    }

    private IEnumerator CoPreventDamage()
    {
        _isPreventDamage = true;

        int blinkCount = Mathf.RoundToInt(_preventTime / (_blinkInterval * 2f));

        Color originColor = _spriteRenderer.color;
        Color invisibleTransparent = new Color(originColor.r, originColor.g, originColor.b, 0f);

        for (int i = 0; i < blinkCount; i++)
        {
            _spriteRenderer.color = invisibleTransparent;
            yield return _blinkWait;

            _spriteRenderer.color = originColor;
            yield return _blinkWait;
        }

        _spriteRenderer.color = originColor;
        _isPreventDamage  = false;
        _preventCoroutine = null; // 정상 종료 시 참조 해제
    }

    private void HandleLevelUp(int newLevel)
    {
        RefreshStats();

        int skillPointsToAdd = (newLevel == CPlayerStatManager.MaxLevel) ? 7 : 2;
        CSkillSystem.Instance?.AddSkillPoint(skillPointsToAdd);
    }

    private void RefreshStats()
    {
        float previousMaxHealth = MaxHealth;

        MaxHealth = _statManager.MaxHealth;
        MoveSpeed = _statManager.GetFinalStat(EPlayerStatType.MoveSpeed) * _traitSpeedMultiplier;

        if (MaxHealth > previousMaxHealth)
        {
            CurrentHealth += (MaxHealth - previousMaxHealth);
        }
    }

    #region 인터페이스 구현부
    public void Heal(float amount)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        Debug.Log($"CPlayerController : 체력 회복 (현재 체력 : {CurrentHealth}/{MaxHealth})");
    }
    #endregion
}
