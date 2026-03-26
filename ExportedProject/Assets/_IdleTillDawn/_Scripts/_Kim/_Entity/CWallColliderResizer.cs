using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Wall Tilemap의 특정 스프라이트 콜라이더 크기를 코드로 재정의한다.
/// Wall Tilemap 오브젝트에 부착한다.
/// </summary>
[RequireComponent(typeof(TilemapCollider2D))]
public class CWallColliderResizer : MonoBehaviour
{
    [Header("대상 스프라이트")]
    [SerializeField] private Sprite _targetSprite;

    [Header("콜라이더 크기 (타일 1칸 = 1.0 기준)")]
    [SerializeField] private Vector2 _colliderSize   = new Vector2(0.4f, 0.5f);
    [SerializeField] private Vector2 _colliderOffset = Vector2.zero;

    private void Awake()
    {
        ApplyCustomShape();
    }

    private void ApplyCustomShape()
    {
        if (_targetSprite == null)
        {
            Debug.LogWarning("[CWallColliderResizer] Target Sprite가 비어있음");
            return;
        }

        float hw = _colliderSize.x * 0.5f;
        float hh = _colliderSize.y * 0.5f;
        float ox = _colliderOffset.x;
        float oy = _colliderOffset.y;

        var shape = new Vector2[]
        {
            new Vector2(-hw + ox, -hh + oy),
            new Vector2( hw + ox, -hh + oy),
            new Vector2( hw + ox,  hh + oy),
            new Vector2(-hw + ox,  hh + oy),
        };

        _targetSprite.OverridePhysicsShape(new List<Vector2[]> { shape });

        // TilemapCollider2D 재생성 강제
        TilemapCollider2D col = GetComponent<TilemapCollider2D>();
        col.enabled = false;
        col.enabled = true;

        Debug.Log($"[CWallColliderResizer] Physics Shape 적용 완료: Size={_colliderSize}, Offset={_colliderOffset}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float hw = _colliderSize.x * 0.5f;
        float hh = _colliderSize.y * 0.5f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + (Vector3)_colliderOffset, new Vector3(_colliderSize.x, _colliderSize.y, 0f));
    }
#endif
}
