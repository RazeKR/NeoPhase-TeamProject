using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CHasturTentaclePoolManager : MonoBehaviour
{
    #region 인스펙터
    [Header("하스터 촉수 설정")]
    [SerializeField] private CPlayerTentacle _tentaclePrefab;
    [SerializeField] private int _tentaclePoolSize = 10;
    #endregion

    #region 내부 변수
    private Queue<CPlayerTentacle> _tentaclePool;
    #endregion

    #region 프로퍼티
    public static CHasturTentaclePoolManager Instance { get; private set; }
    public int CurrentTentacleCount { get; private set; } = 0;
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
        CPlayerTentacle.OnPlayerTentacleReturned += ReturnTentacleToPool;

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        CPlayerTentacle.OnPlayerTentacleReturned -= ReturnTentacleToPool;

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void SpawnTentacle(Vector2 position, float damage, LayerMask enemyLayer)
    {
        CPlayerTentacle tentacle = GetFromPool();
        tentacle.transform.position = position;
        tentacle.gameObject.SetActive(true);

        tentacle.InitAndAttack(damage, enemyLayer);
        CurrentTentacleCount++;
    }

    private void InitPool()
    {
        _tentaclePool = new Queue<CPlayerTentacle>(_tentaclePoolSize);
        for (int i = 0; i < _tentaclePoolSize; i++)
        {
            CPlayerTentacle obj = Instantiate(_tentaclePrefab, transform);
            obj.gameObject.SetActive(false);
            _tentaclePool.Enqueue(obj);
        }
    }

    private void ReturnTentacleToPool(CPlayerTentacle tentacle)
    {
        tentacle.gameObject.SetActive(false);
        _tentaclePool.Enqueue(tentacle);

        CurrentTentacleCount = Mathf.Max(0, CurrentTentacleCount - 1);
    }

    private void ReturnAllTentacles()
    {
        _tentaclePool.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            CPlayerTentacle tentacle = transform.GetChild(i).GetComponent<CPlayerTentacle>();

            if (tentacle != null)
            {
                tentacle.gameObject.SetActive(false);
                _tentaclePool.Enqueue(tentacle);
            }
        }

        CurrentTentacleCount = 0;
    }

    private CPlayerTentacle GetFromPool()
    {
        if (_tentaclePool.Count > 0)
        {
            return _tentaclePool.Dequeue();
        }
        return Instantiate(_tentaclePrefab, transform);
    }

    private void OnSceneUnloaded(Scene currentScene)
    {
        ReturnAllTentacles();
    }
}
