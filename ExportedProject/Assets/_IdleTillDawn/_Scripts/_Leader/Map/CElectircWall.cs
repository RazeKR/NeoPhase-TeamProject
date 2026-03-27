using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 소환 시 플레이어를 봉쇄하는 전기벽 시스템
///
/// [동작 흐름]
/// Activate() 호출
///  → 4개 벽을 카메라 밖에 생성
///  → 슬라이드 인 연출 (SmoothStep)
///  → 스프라이트 루프 애니메이션 재생
///  → 플레이어 접촉 시 데미지 + 넉백
///
/// [물리 설계]
/// - Wall Layer: 플레이어와 충돌 ON, 적·보스 충돌 OFF (Layer Collision Matrix)
/// - 각 벽에 BoxCollider2D (Static) → 플레이어 이동 차단
/// - Physics2D.OverlapBox 폴링 → 데미지 + 넉백 적용
///
/// [스프라이트]
/// - 64x32 px, 6프레임 애니메이션
/// - 수평벽: 원본 방향
/// - 수직벽: 90도 회전 적용
/// </summary>
public class CElectircWall : MonoBehaviour
{
    #region Inspector

    [Header("스프라이트 & 애니메이션")]
    [SerializeField] private Sprite[] _animFrames;          // 6장 프레임
    [SerializeField] private float    _animFPS      = 10f;
    [SerializeField] private int      _sortingOrder  = 10;
    [SerializeField] private float    _tileScale     = 2f;  // 타일 크기 배율 (1 = 원본)

    [Header("봉쇄 영역 (WorldRoot 기준 월드 좌표)")]
    [SerializeField] private Vector2 _areaSize      = new Vector2(20f, 10f);
    [SerializeField] private float   _cornerOverlap = 0.05f; // 코너 겹침 보정 (틈 제거용)

    [Header("등장·퇴장 연출")]
    [SerializeField] private float _revealDuration = 1.2f;
    [SerializeField] private float _hideDuration   = 0.8f;

    [Header("데미지 & 넉백")]
    [SerializeField] private float     _damage          = 1f;
    [SerializeField] private float     _knockbackForce  = 15f;
    [SerializeField] private float     _damageInterval  = 0.5f;  // 데미지 간격 (초)
    [SerializeField] private float     _detectPadding   = 0.3f;  // 감지 영역 여유
    [SerializeField] private LayerMask _playerLayer;

    #endregion

    #region Inner Types

    /// <summary>벽 1면의 런타임 데이터</summary>
    private class WallSide
    {
        public Transform Root;        // 벽 부모 Transform
        public Vector2   TargetPos;   // 슬라이드 후 최종 위치
        public Vector2   KnockDir;    // 이 벽에 닿았을 때 넉백 방향
        public Vector2   DetectSize;  // OverlapBox 크기
    }

    #endregion

    #region Private Fields

    private WallSide _topWall, _botWall, _leftWall, _rightWall;
    private WallSide[] _allWalls;

    private SpriteRenderer[] _allSprites;
    private Vector2          _tileSize;     // 스프라이트 1장 월드 크기
    private Vector2          _activeCenter; // Activate() 호출 시 기준 중심

    #endregion

    #region Unity

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region Public API

    /// <summary>
    /// 보스 소환 시 호출. center = 봉쇄 영역 중심(월드 좌표)
    /// </summary>
    public void Activate(Vector2 center)
    {
        _activeCenter = center;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Co_BuildAndReveal());
    }

    /// <summary>
    /// 중심을 스크립트 위치로 설정하여 활성화
    /// </summary>
    public void Activate()
    {
        Activate(transform.position);
    }

    /// <summary>
    /// 보스 처치 시 호출 — 슬라이드 아웃 후 비활성화
    /// </summary>
    public void Deactivate()
    {
        StopAllCoroutines();
        StartCoroutine(Co_SlideOut());
    }

    #endregion

    #region Build

    private IEnumerator Co_BuildAndReveal()
    {
        if (_animFrames == null || _animFrames.Length == 0)
        {
            Debug.LogWarning("[CElectircWall] AnimFrames가 비어있음");
            yield break;
        }
        _tileSize = _animFrames[0].bounds.size * _tileScale;
        BuildWalls();
        PlaceWallsOutsideCamera();
        StartCoroutine(Co_Animate());
        StartCoroutine(Co_DamageLoop());
        yield return Co_SlideIn();
    }

    private void BuildWalls()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        float halfW  = _areaSize.x * 0.5f;
        float halfH  = _areaSize.y * 0.5f;
        float tileH  = _tileSize.y;
        float innerW = _areaSize.x - tileH * 2f + _cornerOverlap * 2f;
        float insetH = halfH - tileH * 0.5f;
        float insetW = halfW - tileH * 0.5f;

        _leftWall  = CreateVerticalWall("LeftWall",   _activeCenter + new Vector2(-insetW,  0f), _areaSize.y, Vector2.right);
        _rightWall = CreateVerticalWall("RightWall",  _activeCenter + new Vector2( insetW,  0f), _areaSize.y, Vector2.left);
        _topWall   = CreateHorizontalWall("TopWall",  _activeCenter + new Vector2(0f,  insetH),  innerW,      Vector2.down);
        _botWall   = CreateHorizontalWall("BotWall",  _activeCenter + new Vector2(0f, -insetH),  innerW,      Vector2.up);

        _allWalls = new WallSide[] { _topWall, _botWall, _leftWall, _rightWall };

        var list = new List<SpriteRenderer>();
        foreach (WallSide w in _allWalls)
            list.AddRange(w.Root.GetComponentsInChildren<SpriteRenderer>());
        _allSprites = list.ToArray();
    }

    private WallSide CreateHorizontalWall(string wallName, Vector2 pos, float length, Vector2 knockDir)
    {
        GameObject obj = new GameObject(wallName);
        obj.transform.SetParent(transform);
        obj.transform.position = pos;
        obj.layer = gameObject.layer;

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(length, _tileSize.y);

        SpawnTilesAlongWall(obj.transform, length, false);

        return new WallSide
        {
            Root       = obj.transform,
            TargetPos  = pos,
            KnockDir   = knockDir,
            DetectSize = new Vector2(length, _tileSize.y + _detectPadding),
        };
    }

    private WallSide CreateVerticalWall(string wallName, Vector2 pos, float length, Vector2 knockDir)
    {
        GameObject obj = new GameObject(wallName);
        obj.transform.SetParent(transform);
        obj.transform.position = pos;
        obj.layer = gameObject.layer;

        if (length > 0f)
        {
            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(_tileSize.y, length);

            SpawnTilesAlongWall(obj.transform, length, true);
        }

        return new WallSide
        {
            Root       = obj.transform,
            TargetPos  = pos,
            KnockDir   = knockDir,
            DetectSize = new Vector2(_tileSize.y + _detectPadding, length),
        };
    }

    // FloorToInt 개의 완전한 타일 + 나머지는 마지막 타일 scaleX로 정확히 채움
    // fullCount * tileSize.x + remaining == length (항등) → 틈 없음
    private void SpawnTilesAlongWall(Transform parent, float wallLength, bool vertical)
    {
        int   full      = Mathf.FloorToInt(wallLength / _tileSize.x);
        float remainder = wallLength - full * _tileSize.x;
        Quaternion rot  = vertical ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;

        for (int i = 0; i < full; i++)
        {
            float   t   = -wallLength * 0.5f + _tileSize.x * (i + 0.5f);
            Vector2 lp  = vertical ? new Vector2(0f, t) : new Vector2(t, 0f);
            SpawnOneTile(parent, lp, rot, Vector3.one * _tileScale);
        }

        if (remainder > 0.001f)
        {
            float   t      = wallLength * 0.5f - remainder * 0.5f;
            Vector2 lp     = vertical ? new Vector2(0f, t) : new Vector2(t, 0f);
            float   sxLast = (remainder / _tileSize.x) * _tileScale;
            SpawnOneTile(parent, lp, rot, new Vector3(sxLast, _tileScale, 1f));
        }
    }

    private void SpawnOneTile(Transform parent, Vector2 localPos, Quaternion rotation, Vector3 scale)
    {
        GameObject tile = new GameObject("Tile");
        tile.transform.SetParent(parent);
        tile.transform.localPosition = localPos;
        tile.transform.localRotation = rotation;
        tile.transform.localScale    = scale;

        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
        sr.sprite       = _animFrames[0];
        sr.sortingOrder = _sortingOrder;
    }

    #endregion

    #region Slide Animation

    private void PlaceWallsOutsideCamera()
    {
        Camera cam    = Camera.main;
        float  offset = cam != null ? cam.orthographicSize + 4f : 12f;

        _topWall.Root.position   = _topWall.TargetPos   + Vector2.up    * offset;
        _botWall.Root.position   = _botWall.TargetPos   + Vector2.down  * offset;
        _leftWall.Root.position  = _leftWall.TargetPos  + Vector2.left  * offset;
        _rightWall.Root.position = _rightWall.TargetPos + Vector2.right * offset;
    }

    private IEnumerator Co_SlideIn()
    {
        yield return Co_SlideWalls(_revealDuration, false);
    }

    private IEnumerator Co_SlideOut()
    {
        Camera cam    = Camera.main;
        float  offset = cam != null ? cam.orthographicSize + 4f : 12f;

        // 퇴장 목표 위치를 카메라 밖으로 재설정
        Vector2[] exitTargets = new Vector2[]
        {
            _topWall.TargetPos   + Vector2.up    * offset,
            _botWall.TargetPos   + Vector2.down  * offset,
            _leftWall.TargetPos  + Vector2.left  * offset,
            _rightWall.TargetPos + Vector2.right * offset,
        };

        Vector2[] startPositions = new Vector2[]
        {
            _topWall.Root.position,
            _botWall.Root.position,
            _leftWall.Root.position,
            _rightWall.Root.position,
        };

        float elapsed = 0f;
        while (elapsed < _hideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / _hideDuration);
            for (int i = 0; i < _allWalls.Length; i++)
                _allWalls[i].Root.position = Vector2.Lerp(startPositions[i], exitTargets[i], t);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator Co_SlideWalls(float duration, bool reverse)
    {
        Vector2[] starts = new Vector2[_allWalls.Length];
        for (int i = 0; i < _allWalls.Length; i++)
            starts[i] = _allWalls[i].Root.position;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            for (int i = 0; i < _allWalls.Length; i++)
                _allWalls[i].Root.position = Vector2.Lerp(starts[i], _allWalls[i].TargetPos, t);
            yield return null;
        }

        for (int i = 0; i < _allWalls.Length; i++)
            _allWalls[i].Root.position = _allWalls[i].TargetPos;
    }

    #endregion

    #region Sprite Animation

    private IEnumerator Co_Animate()
    {
        WaitForSeconds wait     = new WaitForSeconds(1f / Mathf.Max(1f, _animFPS));
        int            frameIdx = 0;

        while (true)
        {
            frameIdx = (frameIdx + 1) % _animFrames.Length;
            Sprite frame = _animFrames[frameIdx];
            foreach (SpriteRenderer sr in _allSprites)
                if (sr != null) sr.sprite = frame;
            yield return wait;
        }
    }

    #endregion

    #region Damage

    private IEnumerator Co_DamageLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(_damageInterval);
        while (true)
        {
            yield return wait;
            foreach (WallSide wall in _allWalls)
                DetectAndDamage(wall);
        }
    }

    private void DetectAndDamage(WallSide wall)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            wall.Root.position,
            wall.DetectSize,
            0f,
            _playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            // 데미지
            IDamageable damageable = hit.GetComponent<IDamageable>();
            damageable?.TakeDamage(_damage);

            // 넉백: 플레이어 컨트롤러에 위임하여 이동 override 없이 적용
            CPlayerController player = hit.GetComponent<CPlayerController>();
            if (player != null)
                player.ApplyKnockback(wall.KnockDir * _knockbackForce, _damageInterval * 0.6f);
        }
    }

    #endregion

    #region Gizmos

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 봉쇄 영역 미리보기
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        Vector2 center = Application.isPlaying ? _activeCenter : (Vector2)transform.position;
        Gizmos.DrawWireCube(center, _areaSize);

        // 각 벽 두께 시각화
        if (!Application.isPlaying || _tileSize == Vector2.zero) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        foreach (WallSide w in _allWalls)
            Gizmos.DrawWireCube(w.Root.position, w.DetectSize);
    }
#endif

    #endregion
}
