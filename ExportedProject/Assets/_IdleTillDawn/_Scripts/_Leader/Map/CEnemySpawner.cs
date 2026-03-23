using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 여러 종류의 적 프리팹을 오브젝트 풀링으로 관리하고
/// 플레이어 주변 지정 반경 내에 대규모로 스폰하는 스포너 클래스
/// 각 적 타입마다 독립 풀을 보유하며 가중치 기반 랜덤 선택으로 다양한 적이 등장한다
/// 풀이 고갈된 타입은 건너뛰고 최대 활성 수(maxActiveEnemies)를 초과하면 스폰을 일시 중단한다
/// </summary>
public class CEnemySpawner : MonoBehaviour
{
    #region Inspector Variables

    [Header("Enemy Types")]
    [SerializeField] private List<CEnemyType> _enemyTypes; // 스폰할 적 타입 목록 (프리팹 / 풀 크기 / 가중치)

    [Header("Spawn Target")]
    [SerializeField] private Transform _player; // 스폰 기준점이 될 플레이어 Transform

    [Header("Spawn Zone")]
    [SerializeField] private float _spawnMinRadius = 8f;   // 스폰 최소 반경 (플레이어와 너무 가까운 위치 방지)
    [SerializeField] private float _spawnMaxRadius = 15f;  // 스폰 최대 반경 (화면 밖에서 등장하는 느낌 연출)

    [Header("Spawn Settings")]
    [SerializeField] private int   _spawnCountPerWave  = 15;   // 웨이브 1회당 스폰할 적 수
    [SerializeField] private float _spawnInterval      = 2f;   // 웨이브 스폰 주기 (초)
    [SerializeField] private int   _maxActiveEnemies   = 200;  // 동시에 활성화될 수 있는 최대 적 수

    #endregion

    #region Private Variables

    private List<Queue<GameObject>> pools;                        // 적 타입별 오브젝트 풀 (인덱스 = _enemyTypes 인덱스)
    private Dictionary<GameObject, Queue<GameObject>> enemyToPool; // 적 인스턴스 → 소속 풀 역방향 매핑
    private HashSet<GameObject> activeEnemies;                    // 현재 활성화된 적 전체 집합 (중복 방지 및 O(1) 조회)
    private float totalWeight;                                    // 가중치 합산값 (랜덤 선택 시 분모로 사용)

    #endregion

    #region Properties

    /// <summary>
    /// 현재 활성 상태인 적 오브젝트 집합을 외부에서 읽기 전용으로 제공한다
    /// CPlayerController 등 외부 클래스가 적 위치를 순회할 때 사용한다
    /// </summary>
    public IReadOnlyCollection<GameObject> ActiveEnemies => activeEnemies;

    #endregion

    #region Unity Methods

    /// <summary>
    /// 씬 시작 시 1회 호출된다
    /// 모든 적 타입의 풀을 미리 생성하고 스폰 루프 코루틴을 시작한다
    /// </summary>
    private void Start()
    {
        InitializePools();
        StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// 에디터에서 스폰 반경을 시각적으로 확인할 수 있도록 기즈모를 그린다
    /// 내부 원(최소 반경)과 외부 원(최대 반경) 사이가 실제 스폰 영역이다
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_player.position, _spawnMinRadius); // 스폰 최소 반경 시각화

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_player.position, _spawnMaxRadius); // 스폰 최대 반경 시각화
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 지정한 적 오브젝트를 비활성화하고 소속 풀로 반환한다
    /// 활성 목록에 없는 오브젝트는 무시하여 중복 반환으로 인한 풀 오염을 방지한다
    /// </summary>
    /// <param name="enemy">풀로 반환할 적 오브젝트</param>
    public void ReturnToPool(GameObject enemy)
    {
        if (!activeEnemies.Contains(enemy)) return;

        enemy.SetActive(false);
        activeEnemies.Remove(enemy);
        enemyToPool[enemy].Enqueue(enemy); // 역방향 맵으로 O(1) 복귀
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 각 적 타입마다 독립적인 오브젝트 풀을 생성하고
    /// 생성된 인스턴스와 소속 풀 간의 역방향 매핑 딕셔너리를 구성한다
    /// 모든 인스턴스는 비활성 상태로 풀에 대기한다
    /// </summary>
    private void InitializePools()
    {
        pools        = new List<Queue<GameObject>>();
        enemyToPool  = new Dictionary<GameObject, Queue<GameObject>>();
        activeEnemies = new HashSet<GameObject>();
        totalWeight  = 0f;

        foreach (CEnemyType data in _enemyTypes)
        {
            totalWeight += data.Weight; // 전체 가중치 누적

            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < data.PoolSize; i++)
            {
                GameObject go = Instantiate(data.Prefab, transform); // 스포너 하위에 생성하여 씬 정리 유지
                go.SetActive(false);
                queue.Enqueue(go);
                enemyToPool[go] = queue; // 인스턴스 → 풀 역매핑 등록
            }

            pools.Add(queue);
        }
    }

    /// <summary>
    /// _spawnInterval 간격으로 SpawnWave를 반복 호출하는 코루틴이다
    /// 게임이 실행되는 동안 무한 반복되며 게임 오브젝트가 비활성화되면 자동 중단된다
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);
            SpawnWave();
        }
    }

    /// <summary>
    /// 한 웨이브 분량의 적(_spawnCountPerWave 수만큼)을 일괄 스폰한다
    /// 최대 활성 수(_maxActiveEnemies)를 초과하면 해당 웨이브 스폰을 중단한다
    /// </summary>
    private void SpawnWave()
    {
        for (int i = 0; i < _spawnCountPerWave; i++)
        {
            if (activeEnemies.Count >= _maxActiveEnemies) return;
            SpawnOneEnemy();
        }
    }

    /// <summary>
    /// 가중치 기반 랜덤으로 적 타입을 선택하고 풀에서 꺼내 스폰 위치에 배치한다
    /// 선택된 풀이 고갈된 경우(Count == 0) 해당 스폰은 건너뛴다
    /// </summary>
    private void SpawnOneEnemy()
    {
        int typeIndex = GetWeightedRandomIndex();
        Queue<GameObject> pool = pools[typeIndex];

        if (pool.Count == 0) return; // 해당 타입 풀 고갈 시 스킵

        GameObject enemy = pool.Dequeue();
        enemy.transform.position = GetSpawnPosition();
        enemy.SetActive(true);
        activeEnemies.Add(enemy);
    }

    /// <summary>
    /// 각 적 타입의 Weight 비율에 따라 가중치 랜덤으로 인덱스를 반환한다
    /// Weight가 높은 타입일수록 더 자주 선택되어 등장 빈도를 조절할 수 있다
    /// </summary>
    /// <returns>선택된 적 타입의 인덱스</returns>
    private int GetWeightedRandomIndex()
    {
        float roll       = Random.Range(0f, totalWeight); // 0 ~ 전체 가중치 범위에서 무작위 추출
        float cumulative = 0f;

        for (int i = 0; i < _enemyTypes.Count; i++)
        {
            cumulative += _enemyTypes[i].Weight;
            if (roll <= cumulative) return i;
        }

        return _enemyTypes.Count - 1; // 부동소수점 오차 보정용 폴백
    }

    /// <summary>
    /// 플레이어를 중심으로 _spawnMinRadius ~ _spawnMaxRadius 사이의 링 영역에서
    /// 무작위 각도의 스폰 위치를 계산하여 반환한다
    /// 항상 플레이어 시야 밖(최소 반경 이상)에 적이 등장하도록 보장한다
    /// </summary>
    /// <returns>적을 배치할 월드 좌표</returns>
    private Vector3 GetSpawnPosition()
    {
        float angle  = Random.Range(0f, Mathf.PI * 2f);                        // 0 ~ 360도 무작위 각도 (라디안)
        float dist   = Random.Range(_spawnMinRadius, _spawnMaxRadius);          // 최소~최대 반경 사이 무작위 거리
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

        return _player.position + new Vector3(offset.x, offset.y, 0f);
    }

    #endregion
}

/// <summary>
/// 인스펙터에서 적 1종을 정의하기 위한 데이터 클래스
/// 프리팹, 사전 생성 풀 크기, 스폰 가중치를 묶어 관리한다
/// </summary>
[System.Serializable]
public class CEnemyType
{
    [SerializeField] private GameObject _prefab;   // 스폰할 적 프리팹
    [SerializeField] private int        _poolSize = 50;  // 사전 생성할 인스턴스 수 (최대 동시 활성 수 상한)
    [SerializeField] [Range(0.01f, 10f)] private float _weight = 1f; // 스폰 가중치 (높을수록 더 자주 등장)

    public GameObject Prefab    => _prefab;   // 프리팹 읽기 전용 접근자
    public int        PoolSize  => _poolSize; // 풀 크기 읽기 전용 접근자
    public float      Weight    => _weight;   // 가중치 읽기 전용 접근자
}
