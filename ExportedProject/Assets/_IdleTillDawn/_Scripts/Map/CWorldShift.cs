using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 플레이어 워프 방식 대신 "월드 전체를 이동"시키는 World Shift 방식으로 무한 맵을 구현한다
///
/// [플레이어 워프 방식의 근본적 문제]
/// 1. 경계 떨림: 워프 후 새 위치가 반대편 경계와 거의 일치하여 즉시 재워프가 발생한다
/// 2. 몬스터 동기화 끊김: 플레이어만 이동하고 몬스터는 고정 → 거리 단절 → AI 방향 오계산
///
/// [World Shift의 해결 원리]
/// - 플레이어는 절대 이동시키지 않는다
/// - 대신 WorldRoot(타일맵 부모)를 플레이어 반대 방향으로 mapWidth만큼 이동한다
/// - 플레이어의 WorldRoot 로컬 좌표가 경계 내부로 이동하므로 떨림이 구조적으로 발생하지 않는다
/// - 모든 활성 몬스터를 동일한 offset으로 이동시켜 타일맵과의 상대 위치를 보존한다
///
/// [씬 구조 요구사항]
/// WorldRoot (빈 오브젝트) ← 이 스크립트가 붙는 오브젝트
///   └── Grid
///        ├── Grass Tilemap
///        ├── Deco_1 Tilemap
///        └── Deco_2 Tilemap
/// Player (WorldRoot의 자식 아님)
/// Camera (WorldRoot의 자식 아님)
/// </summary>
public class CWorldShift : MonoBehaviour
{
    #region Inspector Variables

    [Header("References")]
    [SerializeField] private Transform     _worldRoot;    // 타일맵을 포함하는 최상위 부모 Transform (Grid의 부모)
    [SerializeField] private Transform     _player;       // 경계 감지 기준 플레이어 Transform
    [SerializeField] private Grid          _grid;         // 경계 계산 기준 Grid — 자식 Tilemap 전체를 합산하여 최대 경계를 산출한다
    [SerializeField] private CSpawnManager _spawnManager; // 몬스터 동기화를 위한 스폰매니저 참조

    [Header("Shift Settings")]
    [SerializeField] private float _checkInterval = 0.05f; // 경계 감지 주기 (초) — Update 매 프레임 방지, 0.05f = 초당 20회

    #endregion

    #region Private Variables

    private float localMinX; // 타일맵의 WorldRoot 기준 로컬 좌측 경계 X
    private float localMaxX; // 타일맵의 WorldRoot 기준 로컬 우측 경계 X
    private float localMinY; // 타일맵의 WorldRoot 기준 로컬 하단 경계 Y
    private float localMaxY; // 타일맵의 WorldRoot 기준 로컬 상단 경계 Y
    private float mapWidth;  // 타일맵 전체 가로 크기 (월드 단위)
    private float mapHeight; // 타일맵 전체 세로 크기 (월드 단위)
    private bool  isShifting; // 시프트 실행 중 플래그 — 코루틴 재진입 및 중복 시프트 방지

    #endregion

    #region Unity Methods

    /// <summary>
    /// 타일맵 경계를 로컬 좌표로 캐싱하고 경계 감지 루프 코루틴을 시작한다
    /// 로컬 좌표를 사용하는 이유: WorldRoot가 이동해도 타일맵의 로컬 경계값은 변하지 않으므로
    /// InverseTransformPoint로 플레이어의 상대 위치를 항상 올바르게 계산할 수 있다
    /// </summary>
    private void Start()
    {
        CacheBounds();
        StartCoroutine(Co_ShiftLoop());
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Grid 아래 모든 Tilemap의 경계를 합산하여 최종 맵 경계를 WorldRoot 로컬 좌표로 캐싱한다
    /// 단일 Tilemap 참조 대신 Grid를 받는 이유:
    ///   - Grass / Deco_1 / Deco_2 등 레이어마다 페인팅 범위가 다를 수 있다
    ///   - 각 Tilemap의 localBounds를 Encapsulate로 합산하면 전체 맵의 실제 최대 경계를 구할 수 있다
    ///   - 어느 레이어가 가장 넓게 칠해져 있든 자동으로 가장 바깥 경계를 기준으로 삼는다
    /// 빈 Tilemap(페인팅 없음)은 localBounds.size == 0 이므로 합산에서 자동으로 무시된다
    /// </summary>
    private void CacheBounds()
    {
        Tilemap[] tilemaps = _grid.GetComponentsInChildren<Tilemap>(); // Grid 자식 Tilemap 전체 수집

        // 초기값을 역방향 무한대로 설정하여 첫 Encapsulate가 항상 적용되도록 한다
        Bounds combined = new Bounds(Vector3.zero, Vector3.zero);
        bool   initialized = false; // 유효한 첫 경계가 들어오기 전까지 초기화 대기 플래그

        foreach (Tilemap tilemap in tilemaps)
        {
            tilemap.CompressBounds(); // 빈 셀 제거 후 실제 페인팅 영역 집계

            if (tilemap.localBounds.size == Vector3.zero) continue; // 페인팅 없는 빈 레이어 건너뜀

            if (!initialized)
            {
                combined    = tilemap.localBounds; // 첫 유효 Tilemap으로 초기화
                initialized = true;
                continue;
            }

            combined.Encapsulate(tilemap.localBounds); // 기존 경계에 현재 Tilemap 경계를 합산
        }

        localMinX = combined.min.x; // 합산된 좌측 경계
        localMaxX = combined.max.x; // 합산된 우측 경계
        localMinY = combined.min.y; // 합산된 하단 경계
        localMaxY = combined.max.y; // 합산된 상단 경계
        mapWidth  = localMaxX - localMinX; // 전체 가로 크기
        mapHeight = localMaxY - localMinY; // 전체 세로 크기
    }

    /// <summary>
    /// _checkInterval 주기마다 경계를 감지하고 필요 시 시프트를 수행하는 루프 코루틴
    /// Update 매 프레임 실행 대신 주기적 코루틴으로 불필요한 연산을 줄인다
    /// WaitForSeconds를 루프 밖에서 한 번만 생성하여 GC 힙 할당을 방지한다
    /// isShifting 플래그가 true인 동안은 체크를 건너뛰어 시프트 실행 중 재진입을 막는다
    /// </summary>
    private IEnumerator Co_ShiftLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(_checkInterval); // GC 방지용 한 번만 할당

        while (true)
        {
            yield return wait;
            if (!isShifting) CheckAndShift(); // 시프트 중이 아닐 때만 체크
        }
    }

    /// <summary>
    /// 플레이어 월드 좌표를 WorldRoot 로컬 좌표로 변환하여 경계 초과 여부를 판정한다
    ///
    /// [핵심 설계 이유]
    /// InverseTransformPoint를 사용하면 WorldRoot가 어디로 이동해도
    /// 플레이어의 "타일맵 내 상대 위치"를 항상 올바르게 계산할 수 있다
    /// 시프트 방향 공식:
    ///   - 플레이어가 오른쪽 초과 → WorldRoot를 오른쪽(+mapWidth)으로 → 플레이어는 왼쪽에 위치
    ///   - 플레이어가 왼쪽 초과  → WorldRoot를 왼쪽(-mapWidth)으로  → 플레이어는 오른쪽에 위치
    /// 시프트 후 플레이어의 로컬 좌표가 경계 내부로 이동하므로 떨림이 구조적으로 발생하지 않는다
    /// </summary>
    private void CheckAndShift()
    {
        // 플레이어 월드 좌표 → WorldRoot 로컬 좌표 변환 (WorldRoot 이동에 자동 추적)
        Vector3 localPos    = _worldRoot.InverseTransformPoint(_player.position);
        Vector3 shiftOffset = Vector3.zero;

        // X축 경계 판정
        if      (localPos.x > localMaxX) shiftOffset.x =  mapWidth;  // 우측 초과 → 세계를 오른쪽으로
        else if (localPos.x < localMinX) shiftOffset.x = -mapWidth;  // 좌측 초과 → 세계를 왼쪽으로

        // Y축 경계 판정
        if      (localPos.y > localMaxY) shiftOffset.y =  mapHeight; // 상단 초과 → 세계를 위로
        else if (localPos.y < localMinY) shiftOffset.y = -mapHeight; // 하단 초과 → 세계를 아래로

        if (shiftOffset == Vector3.zero) return; // 경계 내부 → 시프트 불필요

        ExecuteShift(shiftOffset);
    }

    /// <summary>
    /// WorldRoot를 이동시키고 모든 몬스터를 플레이어 기준 토로이달 최근접 위치로 재배치한다
    ///
    /// [핵심 설계 변경 이유]
    /// 기존: 몬스터에 WorldRoot와 동일한 offset 적용 → 플레이어 기준 부호 거리 반전 → 추적 방향 역전
    /// 변경: WorldRoot만 이동 후, 몬스터를 플레이어 기준 토로이달 최근접 위치로 재배치
    ///       → 플레이어 뒤에서 쫓아오던 몬스터는 시프트 후에도 반드시 뒤에서 쫓아온다
    ///       → 반대편에 멀리 있던 몬스터는 반대편에서 접근해 오는 최단경로로 자동 재배치된다
    ///
    /// isShifting 플래그로 ExecuteShift 실행 중 코루틴이 재진입하는 상황을 원천 차단한다
    /// </summary>
    /// <param name="offset">WorldRoot에 적용할 이동 벡터</param>
    private void ExecuteShift(Vector3 offset)
    {
        isShifting = true;

        _worldRoot.position += offset;                                          // 타일맵 전체(WorldRoot) 이동
        _spawnManager.SyncEnemiesToPlayer(_player.position, mapWidth, mapHeight); // 토로이달 최근접 위치로 재배치

        isShifting = false;
    }

    #endregion

    #region Gizmos

    /// <summary>
    /// 씬 뷰에서 WorldRoot 기준 현재 타일맵 경계를 시각화한다
    /// WorldRoot가 이동해도 실제 타일맵 경계를 정확하게 추적하여 표시한다
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_worldRoot == null || _grid == null) return;

        // 로컬 경계를 WorldRoot의 현재 월드 좌표 기준으로 변환
        Vector3 worldMin = _worldRoot.TransformPoint(new Vector3(localMinX, localMinY, 0));
        Vector3 worldMax = _worldRoot.TransformPoint(new Vector3(localMaxX, localMaxY, 0));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube((worldMin + worldMax) * 0.5f, worldMax - worldMin); // 타일맵 경계 박스
    }

    #endregion
}
