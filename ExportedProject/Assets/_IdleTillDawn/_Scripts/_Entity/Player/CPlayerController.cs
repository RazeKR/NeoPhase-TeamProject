using UnityEngine;

/// <summary>
/// 게임 저장 시 데이터를 담고 있는 임시 클래스, 추후에 병합 시 삭제
/// </summary>
public class PlayerSavedData
{
    public bool isNewGame;
    public float savedCurrentHealth;
}

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

    [Header("공격 기본 설정")]
    [SerializeField] private float _defaultAttackRange = 1.5f;
    #endregion

    #region 내부 변수
    // CWeaponDataSO, CWeaponInstance
    public ScriptableObject EquippedWeapon { get; private set; }

    private CPlayerInputHandler _inputHandler;
    private float _lastInputTime = 0f;

    private int _hashSpeed;
    #endregion

    /// <summary>
    /// 장착한 무기의 사거리를 넘길 프로퍼티
    /// </summary>
    public float CurrentAttackRange
    {
        get
        {
            if (EquippedWeapon != null)
            {
                // 무기의 사거리 리턴
                return 0f;
            }

            return _defaultAttackRange;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        if (_characterData == null)
        {
            Debug.LogWarning("Data SO 없음, 참조 확인");
            return;
        }

        _inputHandler = GetComponent<CPlayerInputHandler>();
        _animator = GetComponent<Animator>();

        _hashSpeed = Animator.StringToHash(_paramSpeed);
    }

    private void OnEnable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnSkillInput += ExecuteManualSkill;
        }
    }

    private void OnDisable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnSkillInput -= ExecuteManualSkill;
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
        #region 임시, 추후에 삭제
        PlayerSavedData savedData = new PlayerSavedData();
        savedData.isNewGame = true;
        InitPlayer(savedData);
        #endregion
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    /// <summary>
    /// 세이브 데이터와 SO 데이터를 조합해 캐릭터 초기화
    /// </summary>
    /// <param name="saveData">세이브 데이터 (나중에 병합 시 맞는 타입으로 변경)</param>
    public void InitPlayer(PlayerSavedData saveData)
    {
        // TODO : 병합 후 추가 내용 작성

        if (_characterData == null) return;

        if (saveData == null)
        {
            Debug.LogWarning("세이브 데이터 없음 (임시 : 초기 데이터로 게임 시작)");
            _maxHealth = _characterData.BaseHealth;
            _moveSpeed = _characterData.BaseMoveSpeed;
            _currentHealth = _maxHealth;
            return;
        }

        float baseHealth = _characterData.BaseHealth;
        float baseSpeed = _characterData.BaseMoveSpeed;

        // 각종 수치 계산

        // 스탯 적용
        _maxHealth = baseHealth;
        _moveSpeed = baseSpeed;

        // 게임 데이터 불러오기
        if (saveData.isNewGame)
        {
            _currentHealth = _maxHealth;
        }
        else
        {
            _currentHealth = saveData.savedCurrentHealth;
        }

        Debug.Log($"{gameObject.name} 초기화 완료, 현재 체력 : {_currentHealth}");
    }

    /// <summary>
    /// 플레이어 이동 메서드
    /// </summary>
    protected override void HandleMovement()
    {
        Vector2 currentVelocity = Vector2.zero;

        if (_inputHandler.IsManualMove)
        {
            currentVelocity = _inputHandler.MoveInput * _moveSpeed;

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

        _rb.velocity = currentVelocity;

        FlipCharacter(currentVelocity.x);
    }

    /// <summary>
    /// 플레이어 공격 메서드
    /// </summary>
    protected override void HandleAttack()
    {
        // TODO : 발사 이벤트
    }

    /// <summary>
    /// 자동 이동 로직
    /// </summary>
    private Vector2 AutoMove()
    {
        if (_currentTarget == null)
        {
            return Vector2.zero;
        }

        float distance = Vector2.Distance(transform.position, _currentTarget.position);
        Vector2 dirToTarget = (_currentTarget.position - transform.position).normalized;

        // 타겟과의 거리가 장착한 무기의 사거리 보다 멀면 타겟을 향해 이동
        if (distance > CurrentAttackRange)
        {
            return dirToTarget * _moveSpeed;
        }
        else
        {
            return Vector2.zero;
        }
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
}
