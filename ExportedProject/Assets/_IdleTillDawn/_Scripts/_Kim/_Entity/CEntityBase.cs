using System;
using System.Collections;
using UnityEngine;

// TODO : 빙결, 화상, 넉백 상태 플래그

[RequireComponent(typeof(Rigidbody2D))]
public abstract class CEntityBase : MonoBehaviour, IDamageable
{
    #region 인스펙터
    [Header("테스트 용 설정")]
    [SerializeField] private bool _isPersonalScene = true;

    [Header("공통 스탯")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _moveSpeed;

    [Header("타겟 탐지 설정")]
    [SerializeField] private float _scanRadius = 10f;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _scanInterval = 0.2f;

    [Header("엔티티 정보")]
    [SerializeField] private string _entityName;
    [SerializeField] private string _description;

    [Header("물리 설정")]
    [SerializeField] private float _defaultGravityScale = 0f;

    [Header("피격 연출 설정")]
    [SerializeField] private float _hitFlashDuration = 0.1f; // 피격 시 흰색으로 유지되는 시간 (초)
    #endregion

    #region 내부 변수
    public event Action<float, float> OnHealthChanged;

    private Rigidbody2D _rb;
    private Transform _currentTarget;
    private float _scanTimer = 0f;
    private Collider2D[] _targetColliders = new Collider2D[50];

    private SpriteRenderer _spriteRenderer; // 피격 플래시에 사용할 스프라이트 렌더러
    private Color _originalColor;           // 피격 전 원래 색상 (복구 기준값)
    private Coroutine _hitFlashCoroutine;   // 동시 피격 시 코루틴 중첩 방지용 핸들
    #endregion

    #region 프로퍼티
    public float MaxHealth
    {
        get => _maxHealth;
        protected set => _maxHealth = value;
    }
    public float MoveSpeed
    {
        get => _moveSpeed;
        protected set => _moveSpeed = value;
    }
    public float CurrentHealth
    {
        get => _currentHealth;
        protected set
        {
            _currentHealth = Mathf.Clamp(value, 0f, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }
    public string EntityName => _entityName;
    public string Description => _description;
    public float DefaultGravityScale
    {
        get => _defaultGravityScale;
        protected set => _defaultGravityScale = value;
    }

    public Rigidbody2D Rb => _rb;
    public Transform CurrentTarget
    {
        get => _currentTarget;
        set => _currentTarget = value;
    }

    public bool IsPersonalScene => _isPersonalScene;

    #endregion

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _rb.gravityScale = _defaultGravityScale;
        _rb.freezeRotation = true;

        // SpriteRenderer 자동 탐색 : 직접 부착된 컴포넌트 우선, 없으면 자식에서 탐색
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 원본 색상 저장 : SpriteRenderer 없는 엔티티는 HitFlash 연출이 생략된다
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
        HandleAttack();
    }

    /// <summary>
    /// 오브젝트 비활성화(풀 반환 포함) 시 HitFlash 코루틴을 중단하고 원래 색상으로 복구한다
    /// SetActive(false) 호출 시 자동으로 실행되므로 별도 리셋 호출이 필요 없다
    /// </summary>
    protected virtual void OnDisable()
    {
        if (_hitFlashCoroutine != null)
        {
            StopCoroutine(_hitFlashCoroutine);
            _hitFlashCoroutine = null;
        }

        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }

    /// <summary>
    /// 움직임 제어
    /// </summary>
    protected abstract void HandleMovement();
    /// <summary>
    /// 공격 제어
    /// </summary>
    protected abstract void HandleAttack();

    public virtual void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        //Debug.Log($"[{gameObject.name}] [데미지 : {damage}, 현재 체력 : {CurrentHealth}]");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 피격 방향 정보를 포함한 데미지 처리 메서드
    /// 체력 차감 후 HitFlash 시각 연출과 데미지 텍스트를 함께 발생시킨다
    /// 연속 피격 시에도 코루틴 중첩 없이 안정적으로 동작한다
    /// </summary>
    /// <param name="damage">입은 데미지의 양</param>
    /// <param name="hitDir">피격 방향 벡터 (공격자→피격자 방향) — 텍스트 위치 오프셋 결정에 사용</param>
    public virtual void TakeDamage(float damage, Vector2 hitDir)
    {
        // 체력 차감 및 사망 판정은 기존 로직 재사용
        TakeDamage(damage);

        // 피격 시각 연출 : SpriteRenderer 색상 플래시
        HitFlash();

        // 데미지 수치 텍스트 생성 : 풀 매니저에 위임하여 Instantiate 없이 재사용
        CDamageTextPoolManager.Instance?.ShowDamage((int)damage, transform.position, hitDir);
    }

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} 사망");
        Destroy(gameObject);
    }

    /// <summary>
    /// 피격 시 SpriteRenderer 색상을 흰색으로 잠깐 변경 후 원래 색상으로 복구한다
    /// 연속 피격 시 기존 코루틴을 즉시 취소하고 새로 시작하여 색상 중첩을 방지한다
    /// SpriteRenderer가 없는 엔티티는 자동으로 생략된다
    /// </summary>
    protected void HitFlash()
    {
        if (_spriteRenderer == null) return;

        // 이미 플래시 중이면 현재 코루틴 취소 후 재시작 (연속 피격 안정성)
        if (_hitFlashCoroutine != null)
            StopCoroutine(_hitFlashCoroutine);

        _hitFlashCoroutine = StartCoroutine(Co_HitFlash());
    }

    /// <summary>
    /// HitFlash 코루틴 — 흰색으로 변경 후 _hitFlashDuration 경과 시 원래 색상으로 복구
    /// </summary>
    private IEnumerator Co_HitFlash()
    {
        _spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(_hitFlashDuration);
        _spriteRenderer.color = _originalColor;
        _hitFlashCoroutine = null;
    }

    /// <summary>
    /// 가장 가까운 적을 찾는 메서드
    /// </summary>
    protected void FindNearestTarget()
    {
        _scanTimer += Time.fixedDeltaTime;

        if (_scanTimer < _scanInterval) return;
        _scanTimer = 0f;

        int count = Physics2D.OverlapCircleNonAlloc(transform.position, _scanRadius, _targetColliders, _targetLayer);

        float shortestDistanceSqr = Mathf.Infinity;
        Transform nearestTarget = null;

        for (int i = 0; i < count; i++)
        {
            Collider2D targetCollider = _targetColliders[i];

            Vector2 offset = targetCollider.transform.position - transform.position;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance < shortestDistanceSqr)
            {
                shortestDistanceSqr = sqrDistance;
                nearestTarget = targetCollider.transform;
            }
        }

        _currentTarget = nearestTarget;
    }

    /// <summary>
    /// 캐릭터 좌우 반전
    /// </summary>
    /// <param name="velocityX"></param>
    protected void FlipCharacter(float velocityX)
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
}
