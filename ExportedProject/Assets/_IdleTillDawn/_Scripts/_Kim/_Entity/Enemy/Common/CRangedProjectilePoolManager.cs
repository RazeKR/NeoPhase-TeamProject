using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRangedProjectilePoolManager : MonoBehaviour
{
    #region 인스펙터
    [Header("투사체 프리팹")]
    [SerializeField] private CProjectileTest _projectilePrefab;

    [Header("풀 설정")]
    [SerializeField] private int _prewarmSize = 20;
    #endregion

    #region 내부 변수
    private Queue<CProjectileTest> _pool;
    #endregion

    #region 프로퍼티
    public static CRangedProjectilePoolManager Instance { get; private set; }
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitPool();
    }

    private void OnEnable()
    {
        CProjectileTest.OnProjectileReturned += ReturnProjectile;
    }

    private void OnDisable()
    {
        CProjectileTest.OnProjectileReturned -= ReturnProjectile;
    }


    public CProjectileTest SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        CProjectileTest obj;

        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            obj = Instantiate(_projectilePrefab, transform);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);

        return obj;
    }

    private void InitPool()
    {
        _pool = new Queue<CProjectileTest>(_prewarmSize);
        for (int i = 0; i < _prewarmSize; i++)
        {
            CProjectileTest obj = Instantiate(_projectilePrefab, transform);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    private void ReturnProjectile(CProjectileTest projectile)
    {
        projectile.gameObject.SetActive(false);
        _pool.Enqueue(projectile);
    }
}
