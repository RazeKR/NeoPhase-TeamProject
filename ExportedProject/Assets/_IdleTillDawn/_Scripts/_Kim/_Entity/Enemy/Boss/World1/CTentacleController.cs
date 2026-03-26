using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTentacleController : CEnemyBase
{
    #region 인스펙터
    [Header("촉수 설정")]
    [SerializeField] private float _orbitDistance = 1f;
    [SerializeField] private float _chargeTime = 1f;

    [Header("참조")]
    [SerializeField] private GameObject _indicatorObj;
    [SerializeField] private BoxCollider2D _indicatorCollider;
    [SerializeField] private Animator _tentacleAnimator;
    [SerializeField] private Animator _indicatorAnimator;

    [Header("에니메이터 관련 설정")]
    [SerializeField] private string _paramAttack = "tAttack";
    [SerializeField] private string _indicatorAnimation = "A_Tentacle_Indicator";

    [Header("테스트 용 설정")]
    [SerializeField] private bool _isPersonalScene = true;
    #endregion

    #region 내부 변수
    private bool _isActivated = false;
    private Coroutine _attackCoroutine;

    private int _hashAttack;
    private bool _hasAttackParam;
    #endregion

    #region 이벤트
    public static event System.Action OnTentacleDestroyed;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        _hasAttackParam = !string.IsNullOrEmpty(_paramAttack);
        if (_hasAttackParam)
        {
            _hashAttack = Animator.StringToHash(_paramAttack);
        }

        if (Rb != null)
        {
            Rb.bodyType = RigidbodyType2D.Static;
            Rb.simulated = true;
        }
    }

    private void OnEnable()
    {
        InitTentacle();
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }

    public override void Die()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        if (_indicatorObj != null)
        {
            _indicatorObj.SetActive(false);
        }

        OnTentacleDestroyed?.Invoke();
        
        if (_isPersonalScene)
        {
            Destroy(gameObject);
        }
        else
        {
            base.Die();
        }
    }

    public void InitTentacle()
    {
        _isActivated = false;

        if (_indicatorObj != null) _indicatorObj.SetActive(false);
    }

    /// <summary>
    /// CEnemyBase의 ResetForPool을 오버라이드하여 촉수 전용 상태 초기화
    /// </summary>
    public override void ResetForPool()
    {
        base.ResetForPool(); //

        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        _isActivated = false;
        if (_indicatorObj != null) _indicatorObj.SetActive(false);
    }

    protected override void HandleMovement()
    {
        
    }

    protected override void HandleAttack()
    {
        if (_isActivated || CurrentTarget == null) return;

        float distance = Vector2.Distance(transform.position, CurrentTarget.position);
        if (distance <= AttackRange)
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
            }

            _attackCoroutine = StartCoroutine(CoActivatePattern());
        }
    }

    protected override void ExecuteAttack()
    {

    }

    private IEnumerator CoActivatePattern()
    {
        _isActivated = true;

        SetIndicatorOrbit();
        _indicatorObj.SetActive(true);

        float timer = 0f;
        while (timer < _chargeTime)
        {
            timer += Time.deltaTime;
            float progress = timer / _chargeTime;

            _indicatorAnimator.Play(_indicatorAnimation, 0, progress * 0.75f);
            yield return null;
        }

        _indicatorAnimator.Play(_indicatorAnimation, 0, 1f);
        _tentacleAnimator.SetTrigger(_hashAttack);

        ApplyDamage();

        yield return new WaitForSeconds(2f / 7f);

        _indicatorObj.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        LastAttackTime = Time.time;
        _isActivated = false;
    }

    private void SetIndicatorOrbit()
    {
        if (CurrentTarget == null) return;

        float heightOffset = 0.5f;
        Vector2 bottomPos = (Vector2)transform.position + Vector2.down * heightOffset;

        Vector2 dir = ((Vector2)CurrentTarget.position - bottomPos).normalized;

        _indicatorObj.transform.position = bottomPos + (dir * _orbitDistance);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _indicatorObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void ApplyDamage()
    {
        if (_indicatorCollider == null) return;

        Vector2 worldPos = _indicatorCollider.transform.TransformPoint(_indicatorCollider.offset);
        Vector2 size = _indicatorCollider.size;
        float angle = _indicatorCollider.transform.eulerAngles.z;

        Collider2D hit = Physics2D.OverlapBox(worldPos, size, angle, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            IDamageable target = hit.gameObject.GetComponent<IDamageable>();
            target?.TakeDamage(AttackDamage);
        }
    }
}
