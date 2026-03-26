using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTentacleController : CEnemyBase
{
    #region 인스펙터
    [Header("촉수 설정")]
    [SerializeField] private float _orbitDistance = 1.5f;
    [SerializeField] private float _chargeTime = 1f;

    [Header("참조")]
    [SerializeField] private GameObject _indicatorObj;
    [SerializeField] private BoxCollider2D _indicatorCollider;
    [SerializeField] private Animator _tentacleAnimator;
    [SerializeField] private Animator _indicatorAnimator;

    [Header("에니메이터 관련 설정")]
    [SerializeField] private string _paramAttack = "tAttack";
    [SerializeField] private string _indicatorAnimation = "A_Tentacle_Indicator";
    #endregion

    #region 내부 변수
    private Transform _playerTr;
    private bool _isActivated = false;

    private int _hashAttack;
    private bool _hasAttackParam;
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

    public void InitTentacle(Transform playerTr)
    {
        _playerTr = playerTr;

        if (_indicatorObj != null) _indicatorObj.SetActive(false);
    }

    protected override void HandleAttack()
    {
        if (_isActivated || CurrentTarget == null) return;

        float distance = Vector2.Distance(transform.position, CurrentTarget.position);
        if (distance <= AttackRange)
        {
            StartCoroutine(CoActivatePattern());
        }
    }

    protected override void ExecuteAttack() { }

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
    }

    private void SetIndicatorOrbit()
    {
        Vector2 dir = (_playerTr.position - transform.position).normalized;

        _indicatorObj.transform.position = (Vector2)transform.position + (dir * _orbitDistance);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _indicatorObj.transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
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
