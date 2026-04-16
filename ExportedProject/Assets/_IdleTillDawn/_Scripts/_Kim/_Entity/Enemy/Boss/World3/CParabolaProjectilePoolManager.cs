using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CParabolaProjectilePoolManager : MonoBehaviour
{
    #region 인스펙터
    [Header("풀 설정")]
    [SerializeField] private int _defaultCapacity = 50;
    [SerializeField] private int _maxSize = 100;

    [Header("투사체 풀 설정")]
    [SerializeField] private CParabolaProjectile _projectilePrefab;

    [Header("인디케이터 풀 설정")]
    [SerializeField] private GameObject _indicatorPrefab;
    #endregion

    #region 내부 변수
    private IObjectPool<CParabolaProjectile> _projectilePool;
    private IObjectPool<GameObject> _indicatorPool;
    #endregion

    private void Awake()
    {
        InitPool();
    }

    private void InitPool()
    {
        _projectilePool = new ObjectPool<CParabolaProjectile>(
            createFunc: CreateProjectile,
            actionOnGet: p => p.gameObject.SetActive(true),
            actionOnRelease: p => p.gameObject.SetActive(false),
            actionOnDestroy: p => Destroy(p.gameObject),
            collectionCheck: false,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );

        _indicatorPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(_indicatorPrefab),
            actionOnGet: ind => ind.SetActive(true),
            actionOnRelease: ind => ind.SetActive(false),
            actionOnDestroy: ind => Destroy(ind),
            collectionCheck: false,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
    }

    private CParabolaProjectile CreateProjectile()
    {
        CParabolaProjectile projectile = Instantiate(_projectilePrefab);
        projectile.SetPools(_projectilePool, _indicatorPool);
        return projectile;
    }

    private void OnGetProjectile(CParabolaProjectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private void OnReleaseProjectile(CParabolaProjectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    private void OnDestroyProjectile(CParabolaProjectile projectile)
    {
        Destroy(projectile.gameObject);
    }

    public CParabolaProjectile GetProjectile()
    {
        return _projectilePool.Get();
    }
}
