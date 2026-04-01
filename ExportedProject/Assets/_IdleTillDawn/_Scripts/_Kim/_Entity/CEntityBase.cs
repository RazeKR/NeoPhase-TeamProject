using System;
using System.Collections;
using UnityEngine;

// TODO : 빙결, 화상, 넉백 상태 플래그
[Flags]
public enum EStatusEffect
{
    None = 0,
    Freeze = 1 << 0,
    Burn = 1 << 1,
    Knockback = 1 << 2,
}

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
    [SerializeField] private float    _hitFlashDuration = 0.1f; // 피격 시 흰색 플래시가 유지되는 시간 (초)
    [SerializeField] private Material _hitFlashMaterial;        // SpriteFlash 셰이더를 사용하는 전용 머티리얼 (인스펙터에서 연결)
    #endregion

    #region 내부 변수
    public event Action<float, float> OnHealthChanged;

    private Rigidbody2D _rb;
    private Transform _currentTarget;
    private float _scanTimer = 0f;
    private Collider2D[] _targetColliders = new Collider2D[50];

    private Coroutine _burnCoroutine;              // 화상 상태이상 지속 코루틴 핸들
    private Coroutine _freezeCoroutine;            // 빙결 상태이상 지속 코루틴 핸들
    private float     _currentFreezeSlowRate = 0f; // 현재 빙결 이동속도 감소 비율
    private Coroutine _knockbackCoroutine;          // 넉백 코루틴 핸들

    private SpriteRenderer _hitFlashRenderer;  // 피격 플래시 대상 스프라이트 렌더러
    private Material       _originalMaterial;  // 플래시 전 원본 공유 머티리얼 — 복구 기준값
    private Coroutine      _hitFlashCoroutine; // 동시 피격 시 코루틴 중첩 방지용 핸들
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

    public virtual float CurrentMoveSpeed
    {
        get
        {
            float finalSpeed = MoveSpeed;

            if (HasStatus(EStatusEffect.Freeze))
            {
                finalSpeed *= _currentFreezeSlowRate;
            }

            return finalSpeed;
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

    public EStatusEffect CurrentStatus { get; protected set; } = EStatusEffect.None;

    protected float CurrentFreezeSlowRate => _currentFreezeSlowRate;
    #endregion

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _rb.gravityScale = _defaultGravityScale;
        _rb.freezeRotation = true;

        // SpriteRenderer 자동 탐색 : 직접 부착된 컴포넌트 우선, 없으면 자식에서 탐색
        _hitFlashRenderer = GetComponent<SpriteRenderer>();
        if (_hitFlashRenderer == null)
            _hitFlashRenderer = GetComponentInChildren<SpriteRenderer>();

        // 원본 공유 머티리얼 저장
        // sharedMaterial 사용으로 불필요한 머티리얼 인스턴스 생성(GC)을 방지한다
        // SpriteRenderer가 없는 엔티티는 HitFlash 연출이 자동으로 생략된다
        if (_hitFlashRenderer != null)
            _originalMaterial = _hitFlashRenderer.sharedMaterial;
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

        // 풀 반환(SetActive false) 시 머티리얼을 원본으로 복구하여 다음 스폰에 플래시 머티리얼이 잔류하지 않도록 한다
        if (_hitFlashRenderer != null)
            _hitFlashRenderer.sharedMaterial = _originalMaterial;
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
    /// 피격 시 SpriteRenderer의 머티리얼을 SpriteFlash 전용 머티리얼로 교체하여 흰색 플래시를 구현한다
    /// SpriteRenderer.color 방식은 원본 tint가 이미 (1,1,1,1)인 경우 시각적 변화가 없으므로
    /// 셰이더 레벨에서 픽셀 RGB를 강제로 흰색으로 교체하는 머티리얼 스왑 방식을 사용한다
    /// 연속 피격 시 기존 코루틴을 즉시 취소하고 새로 시작하여 머티리얼 중첩을 방지한다
    /// _hitFlashMaterial이 null이거나 SpriteRenderer가 없는 엔티티는 자동으로 생략된다
    /// </summary>
    protected void HitFlash()
    {
        if (_hitFlashRenderer == null) return;
        if (_hitFlashMaterial == null) return;

        // 사망 직후 Die() → SetActive(false) 호출로 비활성화된 경우
        // 비활성 오브젝트에서는 StartCoroutine이 불가능하므로 조기 반환한다
        if (!gameObject.activeInHierarchy) return;

        // 이미 플래시 중이면 현재 코루틴 취소 후 재시작 (연속 피격 안정성)
        if (_hitFlashCoroutine != null)
            StopCoroutine(_hitFlashCoroutine);

        _hitFlashCoroutine = StartCoroutine(Co_HitFlash());
    }

    /// <summary>
    /// HitFlash 코루틴 — SpriteFlash 머티리얼로 교체 후 _hitFlashDuration 경과 시 원본 머티리얼로 복구
    /// sharedMaterial 교체이므로 GC 할당이 발생하지 않는다
    /// </summary>
    private IEnumerator Co_HitFlash()
    {
        _hitFlashRenderer.sharedMaterial = _hitFlashMaterial;
        yield return new WaitForSeconds(_hitFlashDuration);
        _hitFlashRenderer.sharedMaterial = _originalMaterial;
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

    #region 상태이상 관리
    /// <summary>
    /// 상태이상 추가
    /// </summary>
    /// <param name="effect"></param>
    public void AddStatus(EStatusEffect effect)
    {
        CurrentStatus |= effect;
    }

    /// <summary>
    /// 상태이상 제거
    /// </summary>
    /// <param name="effect"></param>
    public void RemoveStatus(EStatusEffect effect)
    {
        CurrentStatus &= ~effect;
    }

    /// <summary>
    /// 특정 상태이상에 걸려있는지 확인
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public bool HasStatus(EStatusEffect effect)
    {
        return (CurrentStatus & effect) == effect;
    }
    #endregion

    #region 상태이상 기능
    /// <summary>
    /// 화상을 적용하는 함수
    /// </summary>
    /// <param name="duration">총 지속 시간</param>
    /// <param name="tickDamage">틱당 데미지</param>
    /// <param name="tickInterval">데미지가 들어가는 간격</param>
    public virtual void ApplyBurn(float duration, float tickDamage, float tickInterval)
    {
        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
        }

        _burnCoroutine = StartCoroutine(CoBurnRoutine(duration, tickDamage, tickInterval));
    }

    private IEnumerator CoBurnRoutine(float duration, float tickDamage, float tickInterval)
    {
        AddStatus(EStatusEffect.Burn);

        float timer = 0f;

        while (timer < duration)
        {
            TakeDamage(tickDamage);

            if (CurrentHealth <= 0f) break;

            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }

        RemoveStatus(EStatusEffect.Burn);
        _burnCoroutine = null;
    }

    /// <summary>
    /// 빙결을 적용하는 함수
    /// </summary>
    /// <param name="duration">총 지속 시간</param>
    /// <param name="slowAmount">둔화율 (30f면 30% 속도로 느려짐)</param>
    public virtual void ApplyFreeze(float duration, float slowAmount)
    {
        if (_freezeCoroutine != null)
        {
            StopCoroutine(_freezeCoroutine);
        }

        _freezeCoroutine = StartCoroutine(CoFreezeRoutine(duration, slowAmount));
    }

    private IEnumerator CoFreezeRoutine(float duration, float slowAmount)
    {
        AddStatus(EStatusEffect.Freeze);

        _currentFreezeSlowRate = slowAmount / 100f;

        yield return new WaitForSeconds(duration);

        RemoveStatus(EStatusEffect.Freeze);
        _currentFreezeSlowRate = 1f;
        _freezeCoroutine = null;
    }

    /// <summary>
    /// 넉백을 적용하는 함수
    /// </summary>
    /// <param name="force">밀리는 힘</param>
    /// <param name="duration">넉백 지속 시간</param>
    public virtual void ApplyKnockback(Vector2 force, float duration)
    {
        if (HasStatus(EStatusEffect.Knockback)) return;

        _knockbackCoroutine = StartCoroutine(CoKnockbackRoutine(force, duration));
    }

    private IEnumerator CoKnockbackRoutine(Vector2 force, float duration)
    {
        AddStatus(EStatusEffect.Knockback);

        Rb.velocity = force;

        yield return new WaitForSeconds(duration);

        RemoveStatus(EStatusEffect.Knockback);
        Rb.velocity = Vector2.zero;
        _knockbackCoroutine = null;
    }

    public virtual void ClearAllStatuses()
    {
        CurrentStatus = EStatusEffect.None;
        _currentFreezeSlowRate = 1f;

        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
            _burnCoroutine = null;
        }

        if (_freezeCoroutine != null)
        {
            StopCoroutine(_freezeCoroutine);
            _freezeCoroutine = null;
        }

        if (_knockbackCoroutine != null)
        {
            StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = null;
        }
    }
    #endregion
}
