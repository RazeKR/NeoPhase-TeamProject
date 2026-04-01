using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CFootstep 오브젝트 풀을 관리하는 싱글턴 매니저
/// Instantiate / Destroy 없이 SetActive 기반 재사용으로 GC 스파이크를 방지한다
/// CPlayerFootstepEmitter에서 Spawn을 호출하며,
/// CFootstep이 페이드 완료 후 스스로 Return을 호출하여 풀에 복귀한다
/// </summary>
public class CFootstepPoolManager : MonoBehaviour
{
    #region 인스펙터
    [Header("풀 설정")]
    [SerializeField] private CFootstep _prefab;
    [SerializeField] private int       _poolSize = 30;

    [Header("스프라이트 (2장 권장)")]
    [SerializeField] private Sprite[] _footstepSprites;

    [Header("색상")]
    [SerializeField] private Color _footstepColor = new Color(0.4f, 0.7f, 0.8f, 1f);
    #endregion

    #region 내부 변수
    private Queue<CFootstep> _pool;
    #endregion

    #region 프로퍼티
    /// <summary>씬 전역에서 단일 접근점을 제공하는 싱글턴 인스턴스</summary>
    public static CFootstepPoolManager Instance { get; private set; }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitPool();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 풀에서 발자국을 꺼내 초기화하고 활성화한다
    /// </summary>
    /// <param name="position">생성 월드 좌표</param>
    /// <param name="dir">이동 방향 (정규화)</param>
    /// <param name="flipX">좌우 발 구분을 위한 반전 여부</param>
    public void Spawn(Vector2 position, Vector2 dir, bool flipX)
    {
        if (_footstepSprites == null || _footstepSprites.Length == 0)
        {
            Debug.LogWarning("[CFootstepPoolManager] 발자국 스프라이트가 설정되지 않았습니다.", this);
            return;
        }

        Sprite sprite = _footstepSprites[Random.Range(0, _footstepSprites.Length)];

        CFootstep footstep = GetFromPool();
        footstep.gameObject.SetActive(true);
        footstep.Init(position, dir, sprite, flipX, _footstepColor);
    }

    /// <summary>
    /// CFootstep이 페이드 완료 후 스스로 호출하는 풀 반환 메서드
    /// </summary>
    public void Return(CFootstep footstep)
    {
        footstep.gameObject.SetActive(false);
        _pool.Enqueue(footstep);
    }
    #endregion

    #region Private Methods
    private void InitPool()
    {
        _pool = new Queue<CFootstep>(_poolSize);

        for (int i = 0; i < _poolSize; i++)
        {
            CFootstep footstep = Instantiate(_prefab, transform);
            footstep.gameObject.SetActive(false);
            _pool.Enqueue(footstep);
        }
    }

    private CFootstep GetFromPool()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();

        Debug.LogWarning("[CFootstepPoolManager] 풀 고갈 — 긴급 생성 발생. Inspector의 _poolSize 증가를 권장합니다.", this);
        return Instantiate(_prefab, transform);
    }
    #endregion
}
