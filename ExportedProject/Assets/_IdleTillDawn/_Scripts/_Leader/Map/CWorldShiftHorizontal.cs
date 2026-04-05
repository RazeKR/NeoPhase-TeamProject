using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 좌우 방향으로만 맵을 무한 반복시키는 Horizontal World Shift
///
/// [CWorldShift와의 차이]
/// - X축만 경계 감지 및 시프트 수행
/// - Y축은 벽으로 막혀 있으므로 처리하지 않음
/// - 적 동기화도 X축 토로이달만 적용 (Y는 큰 값으로 고정하여 수직 랩 비활성화)
///
/// [씬 구조 요구사항]
/// WorldRoot (빈 오브젝트) ← 이 스크립트가 붙는 오브젝트
///   └── Grid
///        ├── Ground Tilemap
///        └── Wall Tilemap
/// Player (WorldRoot의 자식 아님)
/// Camera (WorldRoot의 자식 아님)
/// </summary>
public class CWorldShiftHorizontal : MonoBehaviour
{
    #region Inspector Variables

    [Header("References")]
    [SerializeField] private Transform     _worldRoot;    // 타일맵을 포함하는 최상위 부모 Transform
    [SerializeField] private Transform     _player;       // 경계 감지 기준 플레이어 Transform
    [SerializeField] private Grid          _grid;         // 경계 계산 기준 Grid
    [SerializeField] private CSpawnManager _spawnManager; // 몬스터 동기화를 위한 스폰매니저 참조

    [Header("Shift Settings")]
    [SerializeField] private float _checkInterval = 0.05f; // 경계 감지 주기 (초)

    [Header("경계 범위 설정")]
    [Tooltip("ON: 아래 수동 값 사용 / OFF: Tilemap에서 자동 계산")]
    [SerializeField] private bool  _useManualBounds = false;
    [SerializeField] private float _manualMinX      = -20f;  // 수동 좌측 경계 X (WorldRoot 로컬 기준)
    [SerializeField] private float _manualMaxX      =  20f;  // 수동 우측 경계 X (WorldRoot 로컬 기준)

    #endregion

    #region Private Variables

    private float _localMinX;  // 실제 적용되는 좌측 경계 X
    private float _localMaxX;  // 실제 적용되는 우측 경계 X
    private float _mapWidth;   // 타일맵 전체 가로 크기 (월드 단위)
    private float _mapHeight;  // 적 동기화 전달용 (Y 시프트 없음 — 단순 참조용)
    private bool  _isShifting; // 시프트 실행 중 플래그 — 코루틴 재진입 방지

    #endregion

    #region Public Methods

    /// <summary>플레이어 Transform을 런타임에 설정합니다.</summary>
    public void SetPlayerTarget(Transform playerTransform) => _player = playerTransform;

    #endregion

    #region Unity Methods

    private void Start()
    {
        CacheBounds();
        StartCoroutine(Co_ShiftLoop());
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 경계를 캐싱한다.
    /// _useManualBounds가 true면 인스펙터 수동값을 사용하고,
    /// false면 Grid 아래 모든 Tilemap 경계를 합산하여 자동 계산한다.
    /// </summary>
    private void CacheBounds()
    {
        if (_useManualBounds)
        {
            _localMinX = _manualMinX;
            _localMaxX = _manualMaxX;
            _mapHeight = 999f; // Y 시프트 없음 — 적 동기화 전달용 더미값
        }
        else
        {
            Tilemap[] tilemaps = _grid.GetComponentsInChildren<Tilemap>();

            Bounds combined    = new Bounds(Vector3.zero, Vector3.zero);
            bool   initialized = false;

            foreach (Tilemap tilemap in tilemaps)
            {
                tilemap.CompressBounds();

                if (tilemap.localBounds.size == Vector3.zero) continue;

                if (!initialized)
                {
                    combined    = tilemap.localBounds;
                    initialized = true;
                    continue;
                }

                combined.Encapsulate(tilemap.localBounds);
            }

            _localMinX = combined.min.x;
            _localMaxX = combined.max.x;
            _mapHeight = combined.max.y - combined.min.y;
        }

        _mapWidth = _localMaxX - _localMinX;

        Debug.Log($"[CWorldShiftHorizontal] 경계 캐싱 완료 ({(_useManualBounds ? "수동" : "자동")}) — X: {_localMinX:F2} ~ {_localMaxX:F2}, MapWidth: {_mapWidth:F2}");
    }

    /// <summary>
    /// _checkInterval 주기마다 X축 경계를 감지하는 루프 코루틴
    /// </summary>
    private IEnumerator Co_ShiftLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(_checkInterval);

        while (true)
        {
            yield return wait;
            if (!_isShifting) CheckAndShift();
        }
    }

    /// <summary>
    /// 플레이어 월드 좌표를 WorldRoot 로컬 좌표로 변환하여 X축 경계 초과 여부를 판정한다
    ///
    /// 시프트 방향 공식 (X축만):
    ///   - 플레이어가 오른쪽 초과 → WorldRoot를 오른쪽(+mapWidth)으로 → 플레이어는 왼쪽에 위치
    ///   - 플레이어가 왼쪽 초과  → WorldRoot를 왼쪽(-mapWidth)으로  → 플레이어는 오른쪽에 위치
    /// </summary>
    private void CheckAndShift()
    {
        Vector3 localPos    = _worldRoot.InverseTransformPoint(_player.position);
        float   shiftX      = 0f;

        if      (localPos.x > _localMaxX) shiftX =  _mapWidth;  // 우측 초과
        else if (localPos.x < _localMinX) shiftX = -_mapWidth;  // 좌측 초과

        if (shiftX == 0f) return;

        ExecuteShift(shiftX);
    }

    /// <summary>
    /// WorldRoot를 X축으로만 이동시키고 몬스터를 토로이달 최근접 위치로 재배치한다
    /// Y축은 이동하지 않는다
    /// </summary>
    /// <param name="shiftX">WorldRoot에 적용할 X 이동량</param>
    private void ExecuteShift(float shiftX)
    {
        _isShifting = true;

        _worldRoot.position += new Vector3(shiftX, 0f, 0f);                        // X축만 이동
        _spawnManager.SyncEnemiesToPlayer(_player.position, _mapWidth, _mapHeight); // 토로이달 최근접 재배치

        _isShifting = false;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (_worldRoot == null) return;
        if (!_useManualBounds && _grid == null) return;

        // 에디터 미실행 중에는 수동값으로 미리보기
        float previewMinX = _useManualBounds || !Application.isPlaying ? _manualMinX : _localMinX;
        float previewMaxX = _useManualBounds || !Application.isPlaying ? _manualMaxX : _localMaxX;

        Vector3 worldMin = _worldRoot.TransformPoint(new Vector3(previewMinX, -50f, 0f));
        Vector3 worldMax = _worldRoot.TransformPoint(new Vector3(previewMaxX,  50f, 0f));

        // 수동: 초록 / 자동: 시안
        Gizmos.color = _useManualBounds ? Color.green : Color.cyan;
        Gizmos.DrawWireCube((worldMin + worldMax) * 0.5f, worldMax - worldMin);
    }

    #endregion
}
