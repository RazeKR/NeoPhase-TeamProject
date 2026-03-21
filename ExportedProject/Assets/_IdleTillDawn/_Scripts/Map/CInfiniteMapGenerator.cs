using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 타일맵의 실제 페인팅 경계를 기준으로 플레이어가 가장자리를 넘으면
/// 반대편 끝으로 순간이동시켜 맵이 무한히 이어지는 것처럼 보이게 하는 클래스
/// 순간이동 시 카메라도 동일한 오프셋만큼 즉시 이동시켜 화면 끊김이 없도록 한다
/// LateUpdate 이전에 카메라를 선보정하기 때문에 Lerp 보간이 개입할 틈이 없어 자연스러운 전환이 가능하다
/// </summary>
public class CInfiniteMapGenerator : MonoBehaviour
{
    #region Inspector Variables

    [Header("References")]
    [SerializeField] private Tilemap _tilemap;    // 경계 기준이 될 타일맵 (Grass 레이어 권장)
    [SerializeField] private Transform _player;   // 순환 이동 대상 플레이어 Transform
    [SerializeField] private Transform _camera;   // 화면 끊김 방지를 위해 함께 이동시킬 카메라 Transform

    [Header("Wrap Margin")]
    [SerializeField] private float _margin = 0.5f; // 경계 판정 여유 거리 (타일 경계선과 겹치는 픽셀 방지용)

    #endregion

    #region Private Variables

    private float mapWidth;   // 타일맵 전체 가로 길이 (월드 단위)
    private float mapHeight;  // 타일맵 전체 세로 길이 (월드 단위)
    private float minX;       // 타일맵 좌측 경계 X (월드 좌표)
    private float maxX;       // 타일맵 우측 경계 X (월드 좌표)
    private float minY;       // 타일맵 하단 경계 Y (월드 좌표)
    private float maxY;       // 타일맵 상단 경계 Y (월드 좌표)

    #endregion

    #region Unity Methods

    /// <summary>
    /// 씬 시작 시 1회 호출된다
    /// CompressBounds로 실제 페인팅된 영역만 집계한 뒤
    /// localBounds를 월드 좌표로 변환하여 경계값을 캐싱한다
    /// </summary>
    private void Start()
    {
        _tilemap.CompressBounds();

        Vector3 worldMin = _tilemap.transform.TransformPoint(_tilemap.localBounds.min);
        Vector3 worldMax = _tilemap.transform.TransformPoint(_tilemap.localBounds.max);

        minX = worldMin.x;
        maxX = worldMax.x;
        minY = worldMin.y;
        maxY = worldMax.y;

        mapWidth  = maxX - minX; // 가로 길이 = 우측 경계 - 좌측 경계
        mapHeight = maxY - minY; // 세로 길이 = 상단 경계 - 하단 경계
    }

    /// <summary>
    /// 매 프레임 플레이어 위치를 검사하여 경계를 벗어났을 경우 순환 이동을 수행한다
    /// Update에서 카메라를 선보정하면 이후 LateUpdate의 Lerp가 이미 보정된 위치에서 출발하므로
    /// 카메라 끊김 없이 자연스러운 전환이 이루어진다
    /// </summary>
    private void Update()
    {
        WrapPlayer();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 플레이어가 타일맵 경계를 초과했는지 확인하고 반대편으로 순간이동시킨다
    /// 이동량(offset)을 계산하여 플레이어와 카메라에 동일하게 적용함으로써
    /// 상대적 위치가 유지되어 화면이 끊기지 않는다
    /// </summary>
    private void WrapPlayer()
    {
        Vector3 pos    = _player.position;
        Vector3 offset = Vector3.zero;

        if (pos.x > maxX - _margin) offset.x = -mapWidth;       // 오른쪽 경계 초과 → 왼쪽으로
        else if (pos.x < minX + _margin) offset.x = mapWidth;   // 왼쪽 경계 초과 → 오른쪽으로

        if (pos.y > maxY - _margin) offset.y = -mapHeight;      // 상단 경계 초과 → 하단으로
        else if (pos.y < minY + _margin) offset.y = mapHeight;  // 하단 경계 초과 → 상단으로

        if (offset == Vector3.zero) return;

        _player.position += offset; // 플레이어 순간이동
        _camera.position += offset; // 카메라 동시 이동 (끊김 방지)
    }

    #endregion
}
