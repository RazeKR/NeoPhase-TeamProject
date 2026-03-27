using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

// TODO : 플레이어 스탯 조절용 스크립트 추가

[RequireComponent(typeof(CPlayerInputHandler))]
public class CPlayerController : CEntityBase
{
    #region 인스펙터
    [Header("캐릭터 참조")]
    [SerializeField] private CPlayerDataSO _characterData;
    [SerializeField] private Animator _animator;

    [Header("스킬 참조")]
    // 나중에 스킬 클래스 타입에 맞게 수정해야 함
    [SerializeField] private MonoBehaviour[] _equippedSkills = new MonoBehaviour[3];

    [Header("자동 이동 딜레이")]
    [SerializeField] private float _autoModeDelay = 3.0f;

    [Header("애니메이터 파라미터")]
    [SerializeField] private string _paramSpeed = "aSpeed";

    [Header("공격 기본 옵션")]
    [SerializeField] private float _defaultAttackRange = 1.5f;
    [SerializeField] private float _bulletSpeed = 10f;

    [Header("피격 연출 옵션")]
    [SerializeField] private float _preventTime = 1.5f;
    [SerializeField] private float _blinkInterval = 0.1f;
    #endregion

    #region 내부 변수
    private CPlayerInputHandler _inputHandler;
    private float _lastInputTime  = 0f;
    private float _lastAttackTime = 0f;
    private int   _hashSpeed;

    private CPlayerStatManager _statManager;

    private bool _isApproaching = false;
    private Transform _lastTarget;

    private bool       _isPreventDamage    = false;
    private Coroutine  _preventCoroutine   = null; // 코루틴 참조 — 중단/재시작 안전 관리용
    private SpriteRenderer _spriteRenderer;
    private WaitForSeconds _blinkWait;

    private bool  _isKnockedBack    = false;
    private float _knockbackEndTime = 0f;
    #endregion

    /// <summary>
    /// 장착 무기의 사거리 — 무기 미장착 시 기본값 반환
    /// </summary>
    public float CurrentAttackRange
    {
        get
        {
            CWeaponDataSO data = GetEquippedWeaponData();
            return data != null ? data.WeaponRange : _defaultAttackRange;
        }
    }

    /// <summary>
    /// 최종 공격 속도 (플레이어 기본 공격속도 * 무기 공격 속도)
    /// </summary>
    public float CurrentAttackSpeed
    {
        get
        {
            // 플레이어의 베이스 공격 속도 (없으면 기본값 1.0f)
            CPlayerStatManager statManager = GetComponent<CPlayerStatManager>();
            float playerAttackSpeed = statManager != null ? statManager.GetFinalStat(EPlayerStatType.AttackSpeed) : 1.0f;

            // 장착한 무기의 연사 속도
            CWeaponDataSO weaponDataSO = GetEquippedWeaponData();
            float weaponFireRate = weaponDataSO != null ? weaponDataSO.WeaponFireRate : 1.0f;

            return playerAttackSpeed * weaponFireRate;
        }
    }

    public Vector3 PlayerLocalScale
    {
        get
        {
            return transform.localScale;
        }
    }

    /// <summary>
    /// 외부에서 넉백을 가하는 메서드 (전기벽 등)
    /// duration 동안 플레이어 이동 입력을 무시하고 velocity를 유지한다
    /// </summary>
    public void ApplyKnockback(Vector2 force, float duration)
    {
        Rb.velocity       = force;
        _isKnockedBack    = true;
        _knockbackEndTime = Time.fixedTime + duration;
    }

    protected override void Awake()
    {
        base.Awake();
        
        if (_characterData == null)
        {
            Debug.LogWarning("Data SO 없음, 참조 확인");
            return;
        }

        if (_inputHandler == null)
        {
            _inputHandler = GetComponent<CPlayerInputHandler>();
        }

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
    }

    private void OnEnable()
    {
        // 재활성화 시 무적 상태 강제 초기화
        // 코루틴이 외부 요인(씬 이벤트, OnDisable 등)으로 중단되면
        // _isPreventDamage = false 가 실행되지 않아 영구 고착될 수 있으므로 여기서 보장한다
        _isPreventDamage  = false;
        _preventCoroutine = null;

        if (_inputHandler != null)
        {
            _inputHandler.OnSkillInput += ExecuteManualSkill;
        }

        if (_statManager != null)
        {
            _statManager.OnLevelUp += HandleLevelUp;
            _statManager.OnStatUpgraded += RefreshStats;
        }
    }

    private void OnDisable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnSkillInput -= ExecuteManualSkill;
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
        if (CPlayerDataManager.Instance != null)
        {
            InitPlayer(CPlayerDataManager.Instance.CurrentData);
        }
        else
        {
            InitPlayer(null);
        }
    }

    private void Update()
    {
        // 테스트용 피격
        if (Input.GetKeyDown(KeyCode.O))
        {
            TakeDamage(1);
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
    public void InitPlayer(CPlayerSaveData saveData)
    {
        // TODO : 병합 후 추가 내용 작성

        if (_characterData == null) return;

        if (saveData == null || string.IsNullOrEmpty(saveData.Uid))
        {
            Debug.LogWarning("세이브 데이터 없음 (임시 : 초기 데이터로 게임 시작)");
            MaxHealth = _characterData.GetStatInfo(EPlayerStatType.Health).BaseValue;
            MoveSpeed = _characterData.GetStatInfo(EPlayerStatType.MoveSpeed).BaseValue;
            CurrentHealth = MaxHealth;
            return;
        }

        _statManager.SyncWithSaveData(saveData);

        MaxHealth = _statManager.GetFinalStat(EPlayerStatType.Health);
        MoveSpeed = _statManager.GetFinalStat(EPlayerStatType.MoveSpeed);

        CurrentHealth = saveData.SavedHealth;

        Debug.Log($"{gameObject.name} 초기화 완료, 현재 체력 : {CurrentHealth}");
    }

    /// <summary>
    /// 플레이어 이동 메서드
    /// </summary>
    protected override void HandleMovement()
    {
        if (_isKnockedBack)
        {
            if (Time.fixedTime < _knockbackEndTime) return;
            _isKnockedBack = false;
        }

        Vector2 currentVelocity = Vector2.zero;

        if (_inputHandler.IsManualMove)
        {
            currentVelocity = _inputHandler.MoveInput * MoveSpeed;

            _lastInputTime = 0f;
        }
        else
        {
            _lastInputTime += Time.fixedDeltaTime;

            if (_lastInputTime < _autoModeDelay)
            {
                currentVelocity = Vector2.zero;
            }
            else
            {
                currentVelocity = AutoMove();
            }
        }

        float speed = currentVelocity.magnitude;

        _animator.SetFloat(_hashSpeed, speed);

        Rb.velocity = currentVelocity;

        FlipCharacter(currentVelocity.x);
    }

    /// <summary>
    /// 플레이어 공격 메서드
    /// 가장 가까운 적(_currentTarget)이 사거리 안에 있을 때 무기 발사 속도(FireRate)에 맞춰 투사체를 발사한다
    /// </summary>
    protected override void HandleAttack()
    {
        if (CurrentTarget == null) return;

        float distance = Vector2.Distance(transform.position, CurrentTarget.position);
        if (distance > CurrentAttackRange) return;

        CWeaponDataSO weaponData = GetEquippedWeaponData();
        if (weaponData == null || weaponData.BulletPrefab == null) return;

        float finalAttackSpeed = CurrentAttackSpeed;

        float fireInterval = 1f / Mathf.Max(0.01f, finalAttackSpeed);

        if (Time.time < _lastAttackTime + fireInterval) return;

        CWeaponEquip.Instance.WeaponRebound();

        _lastAttackTime = Time.time;

        Vector2    dir       = (CurrentTarget.position - transform.position).normalized;
        GameObject bulletObj = Instantiate(weaponData.BulletPrefab, transform.position, Quaternion.identity);
        CBullet    bullet    = bulletObj.GetComponent<CBullet>();
        if (bullet != null)
        {
            float finalDamage = _statManager.GetFinalStat(EPlayerStatType.Damage);
            bullet.Init(dir, weaponData.WeaponDamage, _bulletSpeed, weaponData.LifeTime);
        }
    }

    /// <summary>
    /// 자동 이동 로직
    /// </summary>
    private Vector2 AutoMove()
    {
        if (CurrentTarget == null)
        {
            _isApproaching = false;
            _lastTarget = null;
            return Vector2.zero;
        }

        float distance = Vector2.Distance(transform.position, CurrentTarget.position);
        Vector2 dirToTarget = (CurrentTarget.position - transform.position).normalized;

        float maxAttackRange = CurrentAttackRange;
        float stopApproachRange = maxAttackRange * 0.7f;

        if (CurrentTarget != _lastTarget)
        {
            _isApproaching = distance > maxAttackRange;
            _lastTarget = CurrentTarget;
        }
        else
        {
            if (distance > maxAttackRange)
            {
                _isApproaching = true;
            }
            else if (distance <= stopApproachRange)
            {
                _isApproaching = false;
            }
        }

        return _isApproaching ? (dirToTarget * MoveSpeed) : Vector2.zero;
    }

    /// <summary>
    /// 플레이어 사망 — 씬 리로드로 현재 스테이지 재시작
    /// </summary>
    public override void Die()
    {
        if (CPlayerDataManager.Instance != null && CPlayerDataManager.Instance.CurrentData != null && CGameManager.Instance != null)
        {
            int savedStage = CPlayerDataManager.Instance.CurrentData.FinalStage;
            int currentStage = CGameManager.Instance.CurrentStageData.StageIndex;

            CPlayerDataManager.Instance.CurrentData.UpdateProgress
            (
                _statManager.CurrentLevel,
                savedStage >= currentStage ? savedStage : currentStage,
                _statManager.CurrentExp,
                MaxHealth,
                _statManager.BonusModifiers
            );

            CPlayerDataManager.Instance.SavePlayerData(CPlayerDataManager.Instance.CurrentData);
        }

        base.Die();
        CGameManager.Instance.RespawnCurrentStage();
    }

    /// <summary>
    /// 현재 장착된 무기 SO를 반환한다
    /// CInventoryManager 미연결 또는 무기 미장착 시 null 반환
    /// </summary>
    private CWeaponDataSO GetEquippedWeaponData()
    {
        if (CInventoryManager.Instance == null) return null;
        CWeaponInstance weapon = CInventoryManager.Instance.EquippedWeapon;
        if (weapon == null) return null;
        return weapon._itemData as CWeaponDataSO;
    }

    /// <summary>
    /// 스킬 수동 사용
    /// </summary>
    /// <param name="index">스킬 슬롯 번호</param>
    private void ExecuteManualSkill(int index)
    {
        if (index < 0 || index >= _equippedSkills.Length) return;

        var skillToUse = _equippedSkills[index];

        if (skillToUse != null)
        {
            Debug.Log($"{index + 1} 번 슬롯 스킬 사용");
            // 스킬 사용
        }
        else
        {
            Debug.Log($"{index + 1} 번 슬롯 비어있음");
        }
    }

    public override void TakeDamage(float damage)
    {
        if (_isPreventDamage) return;

        base.TakeDamage(damage);

        if (CurrentHealth <= 0) return; // 사망 처리는 Die()에 위임, 코루틴 불필요

        // 혹시 이전 코루틴이 살아있으면 중단 후 새로 시작
        // 정상 흐름에서는 _isPreventDamage 가드가 이중 실행을 막지만
        // 비정상 종료로 참조가 남은 경우를 방어한다
        if (_preventCoroutine != null) StopCoroutine(_preventCoroutine);
        _preventCoroutine = StartCoroutine(CoPreventDamage());
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
    }

    private void RefreshStats()
    {
        float previousMaxHealth = MaxHealth;

        MaxHealth = _statManager.GetFinalStat(EPlayerStatType.Health);
        MoveSpeed = _statManager.GetFinalStat(EPlayerStatType.MoveSpeed);

        if (MaxHealth > previousMaxHealth)
        {
            CurrentHealth += (MaxHealth - previousMaxHealth);
        }
    }
}
