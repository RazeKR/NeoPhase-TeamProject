using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Wall Tilemap의 TilemapCollider2D 대신
/// 타일 종류별로 커스텀 BoxCollider2D를 런타임에 생성한다.
/// Wall Tilemap 오브젝트에 부착한다.
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class CWallColliderBuilder : MonoBehaviour
{
    [System.Serializable]
    public class TileColliderConfig
    {
        [Tooltip("충돌 처리할 타일 에셋 (.asset)")]
        public TileBase tile;

        [Tooltip("콜라이더 크기 (타일 1칸 = 1.0)")]
        public Vector2 size = new Vector2(0.4f, 0.8f);

        [Tooltip("타일 중심 기준 오프셋")]
        public Vector2 offset = Vector2.zero;
    }

    [Header("타일별 콜라이더 설정")]
    [SerializeField] private TileColliderConfig[] _tileConfigs;

    private Tilemap    _tilemap;
    private GameObject _colliderRoot;

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();

        // 기존 TilemapCollider2D 비활성화 (대체하므로 필요 없음)
        TilemapCollider2D tilemapCol = GetComponent<TilemapCollider2D>();
        if (tilemapCol != null) tilemapCol.enabled = false;

        CompositeCollider2D compositeCol = GetComponent<CompositeCollider2D>();
        if (compositeCol != null) compositeCol.enabled = false;

        BuildColliders();
    }

    /// <summary>
    /// 타일맵 전체를 순회해 타일별 설정에 맞는 BoxCollider2D를 생성한다
    /// </summary>
    private void BuildColliders()
    {
        if (_tileConfigs == null || _tileConfigs.Length == 0)
        {
            CDebug.LogWarning("[CWallColliderBuilder] Tile Configs가 비어있음");
            return;
        }

        // 타일 → 설정 빠른 조회용 딕셔너리
        var configMap = new Dictionary<TileBase, TileColliderConfig>();
        foreach (TileColliderConfig config in _tileConfigs)
        {
            if (config.tile != null)
                configMap[config.tile] = config;
        }

        // 콜라이더를 모을 루트 오브젝트 생성 (Wall 레이어 상속)
        _colliderRoot = new GameObject("_WallColliders");
        _colliderRoot.layer = gameObject.layer;
        _colliderRoot.transform.SetParent(transform);
        _colliderRoot.transform.localPosition = Vector3.zero;

        // CompositeCollider2D로 묶어 성능 최적화
        Rigidbody2D rb = _colliderRoot.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D composite = _colliderRoot.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

        // 타일맵 전체 순회
        BoundsInt bounds = _tilemap.cellBounds;
        int count = 0;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = _tilemap.GetTile(cellPos);
            if (tile == null) continue;
            if (!configMap.TryGetValue(tile, out TileColliderConfig config)) continue;

            // 타일 월드 중심 좌표
            Vector3 worldPos = _tilemap.GetCellCenterWorld(cellPos);

            GameObject child = new GameObject($"WallCol_{count}");
            child.layer = gameObject.layer;
            child.transform.SetParent(_colliderRoot.transform);
            child.transform.position = worldPos + (Vector3)config.offset;

            BoxCollider2D box = child.AddComponent<BoxCollider2D>();
            box.size            = config.size;
            box.usedByComposite = true;

            count++;
        }

        CDebug.Log($"[CWallColliderBuilder] 콜라이더 {count}개 생성 완료");
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 콜라이더 미리보기 (씬 뷰 Gizmos)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_tilemap == null) _tilemap = GetComponent<Tilemap>();
        if (_tileConfigs == null) return;

        var configMap = new Dictionary<TileBase, TileColliderConfig>();
        foreach (TileColliderConfig config in _tileConfigs)
            if (config.tile != null) configMap[config.tile] = config;

        Gizmos.color = Color.cyan;
        BoundsInt bounds = _tilemap.cellBounds;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = _tilemap.GetTile(cellPos);
            if (tile == null) continue;
            if (!configMap.TryGetValue(tile, out TileColliderConfig config)) continue;

            Vector3 worldPos = _tilemap.GetCellCenterWorld(cellPos) + (Vector3)config.offset;
            Gizmos.DrawWireCube(worldPos, new Vector3(config.size.x, config.size.y, 0f));
        }
    }
#endif
}
