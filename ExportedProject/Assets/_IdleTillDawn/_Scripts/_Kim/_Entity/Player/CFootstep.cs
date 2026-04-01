using UnityEngine;

/// <summary>
/// 풀 기반 발자국 오브젝트
/// Init()으로 초기화되고 페이드 아웃 후 자동으로 CFootstepPoolManager에 반환된다
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CFootstep : MonoBehaviour
{
    #region 인스펙터
    [Header("페이드 설정")]
    [SerializeField] private float _maxLifeTime = 1.2f;
    #endregion

    #region 내부 변수
    private SpriteRenderer _renderer;
    private float _lifeTime;
    private Color _baseColor;   // Init 시 지정된 색상 (alpha 제외) — 페이드 기준값
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _lifeTime -= Time.deltaTime;

        float t = _lifeTime / _maxLifeTime;

        _renderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, t);

        if (_lifeTime <= 0f)
        {
            CFootstepPoolManager.Instance?.Return(this);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 발자국 초기화 — 위치, 회전, 스프라이트를 설정하고 페이드 타이머를 리셋한다
    /// </summary>
    /// <param name="position">생성 월드 좌표</param>
    /// <param name="dir">이동 방향 (정규화)</param>
    /// <param name="sprite">표시할 스프라이트</param>
    /// <param name="flipX">좌우 반전 여부 (좌우 발 구분)</param>
    /// <param name="color">발자국 색상 (alpha는 페이드 아웃으로 덮어쓴다)</param>
    public void Init(Vector2 position, Vector2 dir, Sprite sprite, bool flipX, Color color)
    {
        transform.position = position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        _renderer.sprite = sprite;
        _renderer.flipX  = flipX;

        _baseColor        = new Color(color.r, color.g, color.b, 1f);
        _renderer.color   = _baseColor;

        _lifeTime = _maxLifeTime;
    }
    #endregion
}
