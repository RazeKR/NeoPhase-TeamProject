using UnityEngine;

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

    [Header("적 탐지 설정")]
    [SerializeField] private float _scanRadius = 10f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _scanInterval = 0.2f;

    [Header("공격 기본 설정")]
    [SerializeField] private float _defaultAttackRange = 1.5f;
    #endregion

    #region 내부 변수
    // CWeaponDataSO, CWeaponInstance
    public ScriptableObject EquippedWeapon { get; private set; }

    private CPlayerInputHandler _inputHandler;
    private float _lastInputTime = 0f;

    private int _hashSpeed;

    private Transform _currentTarget;
    private float _scanTimer = 0f;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, CurrentAttackRange);
    }

    private void Start()
    {
        if (_characterData != null)
        {
            _moveSpeed = _characterData.MoveSpeed;
            _currentHealth = _characterData.MaxHealth;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        FindNearestTarget();
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
    /// 자동 이동 메서드
    /// </summary>
    private Vector2 AutoMove()
    {
        if (_currentTarget == null)
        {
            _rb.velocity = Vector2.zero;
            return Vector2.zero;
        }

        float distance = Vector2.Distance(transform.position, _currentTarget.position);
        Vector2 dirToTarget = (_currentTarget.position - transform.position).normalized;

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

    /// <summary>
    /// 캐릭터 좌우 반전
    /// </summary>
    /// <param name="velocityX"></param>
    private void FlipCharacter(float velocityX)
    {
        if (velocityX > 0.001f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (velocityX < -0.001f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    /// <summary>
    /// 가장 가까운 적을 찾는 메서드
    /// </summary>
    private void FindNearestTarget()
    {
        _scanTimer += Time.fixedDeltaTime;

        if (_scanTimer < _scanInterval) return;
        _scanTimer = 0f;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _scanRadius, _enemyLayer);

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider2D collider in colliders)
        {
            float distance = Vector2.Distance(transform.position, collider.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = collider.transform;
            }
        }

        _currentTarget = nearestEnemy;
    }
}
