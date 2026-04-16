using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CProjectilePool : MonoBehaviour
{
    #region Properties
    public static CProjectilePool Instance { get; private set; }
    #endregion

    [System.Serializable]
    public class PoolConfig
    {
        public string PoolKey;
        public GameObject Prefab;
        public int InitialCount = 20;
    }

    #region Inspector
    [Header("투사체 풀 설정")]
    [SerializeField] private List<PoolConfig> _poolConfigs;
    #endregion

    #region Private Variables
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var config in _poolConfigs)
        {
            _pools[config.PoolKey] = new Queue<GameObject>();
            _prefabs[config.PoolKey] = config.Prefab;

            GameObject container = new GameObject($"Pool_{config.PoolKey}");
            container.transform.SetParent(transform);

            for (int i = 0; i < config.InitialCount; i++)
            {
                GameObject obj = Instantiate(config.Prefab, container.transform);
                obj.SetActive(false);
                _pools[config.PoolKey].Enqueue(obj);
            }
        }
    }

    /// <summary>
    /// 투사체를 풀에서 꺼내 발사합니다. (Instantiate 대신 사용)
    /// </summary>
    public GameObject SpawnProjectile(string poolKey, Vector3 position, Quaternion rotation)
    {
        if (!_pools.ContainsKey(poolKey))
        {
            Debug.LogWarning($"[CProjectilePool] '{poolKey}' 풀이 존재하지 않습니다!");
            return null;
        }

        GameObject proj = null;

        while (_pools[poolKey].Count > 0)
        {
            proj = _pools[poolKey].Dequeue();
            if (proj != null) break;
        }

        if (proj == null)
        {
            proj = Instantiate(_prefabs[poolKey], transform.Find($"Pool_{poolKey}"));
        }

        proj.transform.position = position;
        proj.transform.rotation = rotation;

        var trail = proj.GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
            trail.enabled = false;
        }

        proj.SetActive(true);

        if (trail != null) trail.enabled = true;

        return proj;
    }

    /// <summary>
    /// 투사체가 풀로 반환 될 때 호출합니다.
    /// </summary>
    /// <param name="poolKey"></param>
    /// <param name="obj"></param>
    public void ReturnToPool(string poolKey, GameObject obj)
    {
        if (_pools.ContainsKey(poolKey))
        {
            _pools[poolKey].Enqueue(obj);
        }
    }
}
