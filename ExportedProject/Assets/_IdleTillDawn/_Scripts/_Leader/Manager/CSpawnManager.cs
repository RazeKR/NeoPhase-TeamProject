using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CEnemyBase 기반 오브젝트 풀링 스폰 매니저
/// CStageManager의 StartSpawning / StopSpawning 명령을 받아 코루틴 스폰 루프를 제어한다
/// 런타임 중 Instantiate/Destroy 없이 SetActive로만 적을 활성/비활성화하여 GC 스파이크를 방지한다
/// </summary>
public class CSpawnManager : MonoBehaviour
{
    #region Inspector Variables

    [Header("적 타입별 풀 설정")]
    [SerializeField] private CEnemyPoolConfig[] _enemyPoolConfigs;

    [Header("스폰 영역")]
    [SerializeField] private Transform _player;
    [SerializeField] private float     _spawnMinRadius;
    [SerializeField] private float     _spawnMaxRadius;

    [Header("스테이지 매니저 연결")]
    [SerializeField] private CStageManager _stageManager;

    #endregion

    #region Private Variables

    private Dictionary<string, Queue<CEnemyBase>> pools;
    private Dictionary<CEnemyBase, string>        enemyToPoolKey;
    private HashSet<CEnemyBase>                   activeEnemies;
    private List<CEnemyBase>                      killBuffer;
    private Coroutine                             spawnCoroutine;
    private CStageData                            currentStageData;

    #endregion

    #region Properties

    /// <summary>현재 활성 적 GameObject 목록 (읽기 전용)</summary>
    public IEnumerable<GameObject> ActiveEnemies
    {
        get
        {
            foreach (CEnemyBase enemy in activeEnemies)
                yield return enemy.gameObject;
        }
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        pools          = new Dictionary<string, Queue<CEnemyBase>>();
        enemyToPoolKey = new Dictionary<CEnemyBase, string>();
        activeEnemies  = new HashSet<CEnemyBase>();
        killBuffer     = new List<CEnemyBase>();
        InitializePools();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 스테이지 데이터를 주입받아 스폰 코루틴을 시작한다
    /// </summary>
    public void StartSpawning(CStageData stageData)
    {
        currentStageData = stageData;
        spawnCoroutine   = StartCoroutine(Co_SpawnLoop());
    }

    /// <summary>스폰 코루틴을 즉시 중단한다</summary>
    public void StopSpawning()
    {
        if (spawnCoroutine == null) return;
        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }

    /// <summary>
    /// 활성 적 전체를 즉시 풀에 반환한다 (보스 전투 시작 / 스테이지 클리어 시)
    /// </summary>
    public void ReturnAllActiveEnemies()
    {
        killBuffer.Clear();
        foreach (CEnemyBase enemy in activeEnemies) killBuffer.Add(enemy);
        foreach (CEnemyBase enemy in killBuffer)    ReturnToPool(enemy, false);
        killBuffer.Clear();
    }

    /// <summary>
    /// World Shift 발생 후 모든 활성 몬스터를 토로이달 최근접 위치로 재배치한다
    /// </summary>
    public void SyncEnemiesToPlayer(Vector3 playerPos, float mapWidth, float mapHeight)
    {
        float halfWidth  = mapWidth  * 0.5f;
        float halfHeight = mapHeight * 0.5f;

        foreach (CEnemyBase enemy in activeEnemies)
        {
            if (enemy == null) continue;

            Vector3 ePos = enemy.transform.position;

            float dx = ePos.x - playerPos.x;
            if      (dx >  halfWidth)  dx -= mapWidth;
            else if (dx < -halfWidth)  dx += mapWidth;

            float dy = ePos.y - playerPos.y;
            if      (dy >  halfHeight) dy -= mapHeight;
            else if (dy < -halfHeight) dy += mapHeight;

            enemy.transform.position = new Vector3(playerPos.x + dx, playerPos.y + dy, ePos.z);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>각 적 타입별로 poolSize만큼 인스턴스를 미리 생성하고 비활성화한다</summary>
    private void InitializePools()
    {
        foreach (CEnemyPoolConfig config in _enemyPoolConfigs)
        {
            Queue<CEnemyBase> pool = new Queue<CEnemyBase>();

            for (int i = 0; i < config._poolSize; i++)
            {
                CEnemyBase enemy = Instantiate(config._prefab).GetComponent<CEnemyBase>();
                enemy.gameObject.SetActive(false);
                enemyToPoolKey[enemy] = config._poolKey;
                pool.Enqueue(enemy);
            }

            pools[config._poolKey] = pool;
        }
    }

    /// <summary>스폰 주기마다 최대 활성 수까지 적을 추가 스폰하는 루프 코루틴</summary>
    private IEnumerator Co_SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentStageData._spawnInterval);

            int spawnCount = currentStageData._maxActiveCount - activeEnemies.Count;
            for (int i = 0; i < spawnCount; i++) SpawnOne();
        }
    }

    /// <summary>랜덤 타입의 적 1기를 플레이어 주변 링 영역에 스폰한다</summary>
    private void SpawnOne()
    {
        CEnemyPoolConfig config = GetRandomConfig();
        if (!pools.TryGetValue(config._poolKey, out Queue<CEnemyBase> pool)) return;
        if (pool.Count == 0) return;

        CEnemyBase enemy = pool.Dequeue();
        enemy.transform.position = GetRandomSpawnPosition();

        // 현재 스테이지 기반 스탯 초기화 (1-based)
        int stageNumber = currentStageData.StageIndex + 1;
        enemy.InitEnemy(stageNumber);
        enemy.SetTarget(_player); // 플레이어 직접 주입 (스캔 없음)

        enemy.gameObject.SetActive(true);
        activeEnemies.Add(enemy);

        // 사망 이벤트 구독 (중복 방지)
        enemy.OnDied -= OnEnemyDied;
        enemy.OnDied += OnEnemyDied;
    }

    /// <summary>적 사망 이벤트 콜백 — 킬카운트 전달 후 풀 반환</summary>
    private void OnEnemyDied(CEnemyBase enemy) => ReturnToPool(enemy, true);

    /// <summary>적 인스턴스를 비활성화하고 풀에 반환한다</summary>
    private void ReturnToPool(CEnemyBase enemy, bool registerKill)
    {
        if (registerKill) _stageManager.RegisterKill();

        enemy.ResetForPool();
        enemy.gameObject.SetActive(false);
        activeEnemies.Remove(enemy);

        if (enemyToPoolKey.TryGetValue(enemy, out string key))
            pools[key].Enqueue(enemy);
    }

    /// <summary>가중치 기반으로 적 타입을 무작위 선택한다</summary>
    private CEnemyPoolConfig GetRandomConfig()
    {
        float total = 0f;
        foreach (CEnemyPoolConfig config in _enemyPoolConfigs)
            total += Mathf.Max(0f, config._spawnWeight);

        float roll = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;
        foreach (CEnemyPoolConfig config in _enemyPoolConfigs)
        {
            cumulative += Mathf.Max(0f, config._spawnWeight);
            if (roll < cumulative) return config;
        }

        return _enemyPoolConfigs[_enemyPoolConfigs.Length - 1];
    }

    /// <summary>플레이어 주변 링(도넛) 영역 내 무작위 스폰 위치를 반환한다</summary>
    private Vector3 GetRandomSpawnPosition()
    {
        float   angle  = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float   radius = UnityEngine.Random.Range(_spawnMinRadius, _spawnMaxRadius);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        return _player.position + (Vector3)offset;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (_player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_player.position, _spawnMinRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_player.position, _spawnMaxRadius);
    }

    #endregion
}

/// <summary>
/// 적 오브젝트 풀 타입별 설정 — 프리팹은 CEnemyBase 서브클래스를 반드시 포함해야 한다
/// </summary>
[Serializable]
public class CEnemyPoolConfig
{
    [SerializeField] public string     _poolKey;
    [SerializeField] public GameObject _prefab;       // CEnemyBase 컴포넌트 필수 (CBoomerController 등)
    [SerializeField] public int        _poolSize;
    [SerializeField] public float      _spawnWeight = 1f; // 스폰 비율 가중치 (높을수록 자주 등장)
}
