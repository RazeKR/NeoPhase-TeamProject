using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일반 몬스터의 오브젝트 풀링 기반 스폰을 전담하는 매니저
/// CStageManager의 StartSpawning / StopSpawning 명령을 받아 코루틴 스폰 루프를 제어한다
/// 모든 Instantiate / Destroy 호출은 씬 시작 시 InitializePools에서만 허용되며
/// 런타임 중에는 SetActive로만 적을 활성/비활성화하여 GC 스파이크를 방지한다
/// </summary>
public class CSpawnManager : MonoBehaviour
{
    #region Inspector Variables

    [Header("적 타입별 풀 설정")]
    [SerializeField] private CEnemyPoolConfig[] _enemyPoolConfigs; // 적 종류 배열 (인스펙터에서 추가)

    [Header("스폰 영역")]
    [SerializeField] private Transform _player;       // 스폰 반경의 기준이 되는 플레이어 Transform
    [SerializeField] private float     _spawnMinRadius; // 스폰 최소 반경 (카메라 밖을 권장)
    [SerializeField] private float     _spawnMaxRadius; // 스폰 최대 반경

    [Header("스테이지 매니저 연결")]
    [SerializeField] private CStageManager _stageManager; // 킬 카운트 전달 대상

    #endregion

    #region Private Variables

    private Dictionary<string, Queue<CEnemy>> pools;         // 풀키(태그)별 대기 큐
    private Dictionary<CEnemy, string>        enemyToPoolKey; // 인스턴스 → 풀키 역방향 맵 (O(1) 반환용)
    private HashSet<CEnemy>                   activeEnemies;  // 현재 활성 적 집합 (외부 조회용 프로퍼티로 노출)
    private List<CEnemy>                      killBuffer;     // 순회 중 제거 대상 임시 버퍼
    private Coroutine                         spawnCoroutine; // 스폰 코루틴 핸들
    private CStageData                        currentStageData; // 현재 스테이지 스폰 설정 캐시

    #endregion

    #region Properties

    /// <summary>
    /// 현재 활성 상태인 적 GameObject 목록을 읽기 전용으로 노출한다
    /// CPlayerController의 킬존 검사가 이 프로퍼티를 통해 활성 적을 순회한다
    /// HashSet을 직접 노출하지 않고 변환하여 외부에서 컬렉션을 수정하지 못하도록 보호한다
    /// </summary>
    public IEnumerable<GameObject> ActiveEnemies
    {
        get
        {
            foreach (CEnemy enemy in activeEnemies) yield return enemy.gameObject; // GameObject로 변환하여 반환
        }
    }

    #endregion

    #region Unity Methods

    /// <summary>
    /// 씬 시작 시 모든 적 타입의 풀을 미리 생성한다
    /// Awake에서 실행하여 다른 스크립트의 Start보다 먼저 풀이 준비되도록 보장한다
    /// </summary>
    private void Awake()
    {
        pools          = new Dictionary<string, Queue<CEnemy>>();
        enemyToPoolKey = new Dictionary<CEnemy, string>();
        activeEnemies  = new HashSet<CEnemy>();
        killBuffer     = new List<CEnemy>();
        InitializePools();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 스테이지 데이터를 주입받아 스폰 코루틴을 시작한다
    /// CStageManager의 Farming 상태 EnterState에서 호출된다
    /// </summary>
    /// <param name="stageData">현재 스테이지의 스폰 간격 및 최대 수 설정</param>
    public void StartSpawning(CStageData stageData)
    {
        currentStageData = stageData;
        spawnCoroutine   = StartCoroutine(Co_SpawnLoop());
    }

    /// <summary>
    /// 스폰 코루틴을 즉시 중단한다
    /// BossReady 또는 BossFight 진입 시 CStageManager가 호출한다
    /// </summary>
    public void StopSpawning()
    {
        if (spawnCoroutine == null) return;
        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }

    /// <summary>
    /// GameObject를 기준으로 해당 적을 풀에 반환한다
    /// CPlayerController의 킬존에서 GameObject 단위로 반환 요청을 보낼 때 사용한다
    /// CEnemy 컴포넌트가 없는 오브젝트는 무시하여 안전하게 처리한다
    /// </summary>
    /// <param name="enemyObj">반환할 적 GameObject</param>
    public void ReturnToPoolByObject(GameObject enemyObj)
    {
        CEnemy enemy = enemyObj.GetComponent<CEnemy>(); // CEnemy 컴포넌트 조회
        if (enemy == null) return;                       // 컴포넌트 없으면 무시
        if (!activeEnemies.Contains(enemy)) return;      // 이미 반환된 적이면 무시

        ReturnToPool(enemy, true); // 킬카운트 반영하여 반환
    }

    /// <summary>
    /// World Shift 발생 후 모든 활성 몬스터를 플레이어 기준 토로이달 최근접 위치로 재배치한다
    ///
    /// [기존 ShiftAllEnemies의 문제]
    /// WorldRoot와 동일한 offset을 몬스터에 더하면 플레이어 기준 부호 거리가 반전된다
    /// 예: 21유닛 뒤에서 추적하던 몬스터가 79유닛 앞에 나타남
    ///
    /// [토로이달 최근접 위치 원리]
    /// 루프 맵은 원환면(Torus) 위상 구조다
    /// 두 점 사이의 "진짜 거리"는 유클리드 거리가 아닌 맵 폭 절반을 기준으로 결정된다
    /// 부호 거리 d가 +halfWidth를 초과하면 반대편에서 오는 경로가 더 짧으므로 d -= mapWidth
    /// 부호 거리 d가 -halfWidth 미만이면 반대편 경로가 더 짧으므로 d += mapWidth
    /// 결과: 플레이어 뒤에서 추적하던 몬스터는 시프트 후에도 항상 뒤에서 쫓아온다
    /// </summary>
    /// <param name="playerPos">플레이어 현재 월드 좌표 (재배치 기준점)</param>
    /// <param name="mapWidth">타일맵 전체 가로 크기</param>
    /// <param name="mapHeight">타일맵 전체 세로 크기</param>
    public void SyncEnemiesToPlayer(Vector3 playerPos, float mapWidth, float mapHeight)
    {
        float halfWidth  = mapWidth  * 0.5f; // X축 토로이달 기준: 이 값을 초과하면 반대편이 더 가깝다
        float halfHeight = mapHeight * 0.5f; // Y축 토로이달 기준

        foreach (CEnemy enemy in activeEnemies)
        {
            if (enemy == null) continue;

            Vector3 ePos = enemy.transform.position;

            // X: 플레이어 기준 부호 거리를 [-halfWidth, +halfWidth]로 보정
            float dx = ePos.x - playerPos.x; // 현재 플레이어 기준 X 부호 거리
            if      (dx >  halfWidth) dx -= mapWidth;  // 오른쪽으로 너무 멀면 왼쪽으로 감기
            else if (dx < -halfWidth) dx += mapWidth;  // 왼쪽으로 너무 멀면 오른쪽으로 감기

            // Y: 플레이어 기준 부호 거리를 [-halfHeight, +halfHeight]로 보정
            float dy = ePos.y - playerPos.y; // 현재 플레이어 기준 Y 부호 거리
            if      (dy >  halfHeight) dy -= mapHeight; // 위쪽으로 너무 멀면 아래쪽으로 감기
            else if (dy < -halfHeight) dy += mapHeight; // 아래쪽으로 너무 멀면 위쪽으로 감기

            enemy.transform.position = new Vector3(playerPos.x + dx, playerPos.y + dy, ePos.z); // 최근접 위치 적용
        }
    }

    /// <summary>
    /// 활성 적 전체를 즉시 풀에 반환한다
    /// 보스 전투 시작 전 또는 스테이지 클리어 시 잔여 적을 정리하는 용도로 사용한다
    /// </summary>
    public void ReturnAllActiveEnemies()
    {
        killBuffer.Clear();
        foreach (CEnemy enemy in activeEnemies) killBuffer.Add(enemy); // 순회 중 수정 방지
        foreach (CEnemy enemy in killBuffer)    ReturnToPool(enemy, false); // 킬카운트 미반영
        killBuffer.Clear();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 각 적 타입별로 poolSize만큼 인스턴스를 미리 생성하고 비활성화한다
    /// enemyToPoolKey 역방향 맵에 등록하여 사망 시 O(1)로 풀을 찾을 수 있도록 한다
    /// </summary>
    private void InitializePools()
    {
        foreach (CEnemyPoolConfig config in _enemyPoolConfigs)
        {
            Queue<CEnemy> pool = new Queue<CEnemy>();

            for (int i = 0; i < config._poolSize; i++)
            {
                CEnemy enemy = Instantiate(config._prefab).GetComponent<CEnemy>(); // 사전 생성
                enemy.gameObject.SetActive(false);
                enemyToPoolKey[enemy] = config._poolKey; // 역방향 맵 등록
                pool.Enqueue(enemy);
            }

            pools[config._poolKey] = pool;
        }
    }

    /// <summary>
    /// 스폰 주기마다 활성 수를 확인하고 부족분만큼 적을 추가 스폰하는 루프 코루틴
    /// maxActiveCount 초과 시 해당 웨이브 스킵하여 과도한 유닛 집결을 방지한다
    /// </summary>
    private IEnumerator Co_SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentStageData._spawnInterval);

            int spawnCount = currentStageData._maxActiveCount - activeEnemies.Count; // 부족분 계산
            for (int i = 0; i < spawnCount; i++) SpawnOne();
        }
    }

    /// <summary>
    /// 랜덤 타입의 적 1기를 플레이어 주변 링 영역에 스폰한다
    /// 풀이 비어있는 타입은 건너뛰어 풀 고갈 시에도 안전하게 동작한다
    /// </summary>
    private void SpawnOne()
    {
        CEnemyPoolConfig config = GetRandomConfig();
        if (!pools.TryGetValue(config._poolKey, out Queue<CEnemy> pool)) return;
        if (pool.Count == 0) return; // 해당 타입 풀 고갈 시 스킵

        CEnemy enemy = pool.Dequeue();
        enemy.transform.position = GetRandomSpawnPosition();
        // StageData SO 배율 대신 CGameManager 누적 공식으로 계산
        // currentStageIndex가 씬 전환 후에도 유지되므로 스테이지가 오를수록 자동 계승된다
        enemy.Initialize(_player, CGameManager.Instance.GetEnemyHpMultiplier());
        enemy.gameObject.SetActive(true);
        activeEnemies.Add(enemy);

        // 사망 이벤트 구독 (중복 방지를 위해 -= 후 +=)
        enemy.OnDied -= OnEnemyDied;
        enemy.OnDied += OnEnemyDied;
    }

    /// <summary>
    /// 적 사망 이벤트 콜백
    /// 킬카운트를 StageManager에 전달하고 해당 인스턴스를 풀에 반환한다
    /// </summary>
    private void OnEnemyDied(CEnemy enemy) => ReturnToPool(enemy, true); // 킬카운트 반영

    /// <summary>
    /// 적 인스턴스를 비활성화하고 풀에 반환한다
    /// registerKill이 true일 때만 StageManager에 킬 신호를 전달한다
    /// </summary>
    /// <param name="enemy">반환할 적 인스턴스</param>
    /// <param name="registerKill">킬카운트에 집계할지 여부</param>
    private void ReturnToPool(CEnemy enemy, bool registerKill)
    {
        if (registerKill) _stageManager.RegisterKill(); // 킬카운트 전달

        enemy.gameObject.SetActive(false);
        activeEnemies.Remove(enemy);

        if (enemyToPoolKey.TryGetValue(enemy, out string key)) // O(1) 풀키 조회
            pools[key].Enqueue(enemy);
    }

    /// <summary>
    /// 풀 설정 배열에서 무작위로 적 타입을 선택한다
    /// 추후 가중치 기반 선택으로 교체할 수 있도록 별도 메서드로 분리한다
    /// </summary>
    /// <returns>무작위 선택된 적 풀 설정</returns>
    private CEnemyPoolConfig GetRandomConfig() =>
        _enemyPoolConfigs[UnityEngine.Random.Range(0, _enemyPoolConfigs.Length)];

    /// <summary>
    /// 플레이어 주변 링(도넛) 영역 내 무작위 위치를 반환한다
    /// 최소 반경을 두어 플레이어 발 밑에 스폰되는 상황을 원천 방지한다
    /// </summary>
    /// <returns>스폰할 월드 좌표</returns>
    private Vector3 GetRandomSpawnPosition()
    {
        float   angle  = UnityEngine.Random.Range(0f, Mathf.PI * 2f);               // 무작위 각도 (라디안)
        float   radius = UnityEngine.Random.Range(_spawnMinRadius, _spawnMaxRadius); // 링 내 랜덤 반경
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        return _player.position + (Vector3)offset;
    }

    #endregion

    #region Gizmos

    /// <summary>
    /// 씬 뷰에서 스폰 최소/최대 반경을 시각화한다
    /// 플레이어 기준으로 노란색(최소), 빨간색(최대) 원을 그려 스폰 링 영역을 확인할 수 있다
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_player.position, _spawnMinRadius); // 최소 반경
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_player.position, _spawnMaxRadius); // 최대 반경
    }

    #endregion
}

/// <summary>
/// 적 오브젝트 풀의 타입별 설정을 묶는 직렬화 가능한 구조체
/// 배열 원소로 사용하여 인스펙터에서 적 종류를 자유롭게 추가/제거할 수 있다
/// </summary>
[Serializable]
public class CEnemyPoolConfig
{
    [SerializeField] public string     _poolKey;  // 풀 식별 키 (타입명과 일치 권장, 예: "Slime", "Orc")
    [SerializeField] public GameObject _prefab;   // 스폰할 적 프리팹 (CEnemy 컴포넌트 필수)
    [SerializeField] public int        _poolSize; // 사전 생성할 인스턴스 수 (최대 동시 활성 수 이상으로 설정)
}
