using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerTentacle : MonoBehaviour
{
    #region 인스펙터
    [Header("촉수 설정")]
    [SerializeField] private float _lifeTime = 10f;
    [SerializeField] private float _attackInterval = 3f;
    [SerializeField] private float _chargeTime = 1f;
    [SerializeField] private float _orbitDistance = 1f;
    [SerializeField] private float _detectRadius = 3f;

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
    private int _hashAttack;
    private float _damage;
    private LayerMask _enemyLayer;
    private Transform _currentTarget;
    #endregion

    #region 이벤트
    public static event System.Action<CPlayerTentacle> OnPlayerTentacleReturned;
    #endregion

    private void Awake()
    {
        if (!string.IsNullOrEmpty(_paramAttack))
        {
            _hashAttack = Animator.StringToHash(_paramAttack);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (_indicatorObj != null)
        {
            _indicatorObj.SetActive(false);
        }
    }

    public void InitAndAttack(float damage, LayerMask enemyLayer)
    {
        _damage = damage;
        _enemyLayer = enemyLayer;

        StartCoroutine(CoLifeTimeTimer());
        StartCoroutine(CoAttackLoop());
    }

    private IEnumerator CoLifeTimeTimer()
    {
        yield return new WaitForSeconds(_lifeTime);

        OnPlayerTentacleReturned?.Invoke(this);
    }

    private IEnumerator CoAttackLoop()
    {
        while (true)
        {
            FindNearestTarget();

            if (_currentTarget != null)
            {
                yield return StartCoroutine(CoActivatePattern());
                yield return new WaitForSeconds(_attackInterval);
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private IEnumerator CoActivatePattern()
    {
        if (_currentTarget != null && _currentTarget.gameObject.activeInHierarchy)
        {
            SetIndicatorOrbit();
        }
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
    }

    private void SetIndicatorOrbit()
    {
        if (_currentTarget == null) return;

        float heightOffset = 0.5f;
        Vector2 bottomPos = (Vector2)transform.position + Vector2.down * heightOffset;

        Vector2 dir = ((Vector2)_currentTarget.position - bottomPos).normalized;

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

        Collider2D[] hits = Physics2D.OverlapBoxAll(worldPos, size, angle, _enemyLayer);

        foreach (Collider2D hit in hits)
        {
            CEntityBase target = hit.GetComponent<CEntityBase>();
            if (target != null)
            {
                target.TakeDamage(_damage, Vector2.zero);
            }
        }
    }

    private void FindNearestTarget()
    {
        _currentTarget = null;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _detectRadius, _enemyLayer);

        float closestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                _currentTarget = hit.transform;
            }
        }
    }
}
