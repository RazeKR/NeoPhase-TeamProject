using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CParabolaProjectile : MonoBehaviour
{
    #region 인스펙터
    [Header("참초")]
    [SerializeField] private Transform _projectileSprite;

    [Header("포물선 설정")]
    [SerializeField] private float _height = 2f;
    [SerializeField] private float _flightTime = 1.5f;

    [Header("공격 설정")]
    [SerializeField] private LayerMask _playerLayer;
    #endregion

    #region 내부 변수
    private Vector2 _startPos;
    private Vector2 _targetPos;
    private GameObject _currentIndicator;

    private Coroutine _flyCoroutine;

    private float _finalDamage;

    private IObjectPool<CParabolaProjectile> _projectilePool;
    private IObjectPool<GameObject> _indicatorPool;
    #endregion

    public void SetPools(IObjectPool<CParabolaProjectile> projPool, IObjectPool<GameObject> indPool)
    {
        _projectilePool = projPool;
        _indicatorPool = indPool;
    }

    public void Fire(Vector2 startPos, Vector2 targetPos, float damage)
    {
        _finalDamage = damage;

        _startPos = startPos;
        _targetPos = targetPos;
        transform.position = _startPos;

        if (_indicatorPool != null)
        {
            _currentIndicator = _indicatorPool.Get();
            _currentIndicator.transform.position = _targetPos;
        }

        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
        }

        _flyCoroutine = StartCoroutine(CoFly());
    }

    private IEnumerator CoFly()
    {
        float time = 0f;

        while (time < _flightTime)
        {
            time += Time.deltaTime;
            float t = time / _flightTime;

            Vector2 currentGroundPos = Vector2.Lerp(_startPos, _targetPos, t);
            transform.position = currentGroundPos;

            float height = Mathf.Sin(t * Mathf.PI) * _height;
            _projectileSprite.localPosition = new Vector3(0f, height, 0f);

            yield return null;
        }

        Impact();
    }

    private void Impact()
    {
        float impactRadius = 1f;

        if (_currentIndicator != null)
        {
            impactRadius = _currentIndicator.transform.localScale.x * 0.5f;

            if (_indicatorPool != null)
            {
                _indicatorPool.Release(_currentIndicator);
            }
            else
            {
                Destroy(_currentIndicator);
            }

            _currentIndicator = null;
        }

        Collider2D hitCollider = Physics2D.OverlapCircle(transform.position, impactRadius, _playerLayer);

        if (hitCollider != null)
        {
            IDamageable target = hitCollider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(_finalDamage);
            }
        }

        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
            _flyCoroutine = null;
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (_flyCoroutine != null)
        {
            StopCoroutine(_flyCoroutine);
            _flyCoroutine = null;
        }

        _projectileSprite.localPosition = Vector3.zero;

        try
        {
            if (_projectilePool != null)
            {
                _projectilePool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        catch
        {
            Destroy(gameObject);
        }
    }
}
