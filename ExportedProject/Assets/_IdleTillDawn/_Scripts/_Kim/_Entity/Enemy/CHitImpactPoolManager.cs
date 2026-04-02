using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CHitImpactFX 오브젝트 풀을 관리하는 싱글턴 매니저
/// 런타임 중 Instantiate / Destroy 없이 SetActive 기반 재사용으로 GC 스파이크를 방지한다
/// CEnemyBase.TakeDamage(float, Vector2) 에서 호출되며,
/// CHitImpactFX가 재생 완료 후 스스로 Return을 호출하여 풀에 복귀한다
/// </summary>
public class CHitImpactPoolManager : MonoBehaviour
{
    #region 인스펙터
    [Header("풀 설정")]
    [SerializeField] private CHitImpactFX _prefab;   // CHitImpactFX 컴포넌트가 부착된 프리팹
    [SerializeField] private int _poolSize = 15;     // 씬 시작 시 미리 생성할 풀 크기
    #endregion

    #region 내부 변수
    private Queue<CHitImpactFX> _pool;
    #endregion

    #region 프로퍼티
    /// <summary>씬 전역에서 단일 접근점을 제공하는 싱글턴 인스턴스</summary>
    public static CHitImpactPoolManager Instance { get; private set; }
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
    /// 피격 방향에 따라 적 몸통 위에 HitImpact FX를 표시한다
    /// </summary>
    /// <param name="enemyWorldPos">적의 월드 중심 좌표</param>
    /// <param name="hitDir">피격 방향 벡터 (공격자 → 피격자)</param>
    /// <param name="hitRadius">적 콜라이더 반경 — FX 배치 오프셋 계산에 사용</param>
    public void ShowHitImpact(Vector3 enemyWorldPos, Vector2 hitDir, float hitRadius)
    {
        CHitImpactFX fx = GetFromPool();
        fx.gameObject.SetActive(true);
        fx.Init(enemyWorldPos, hitDir, hitRadius);
    }

    /// <summary>
    /// CHitImpactFX가 재생 완료 후 스스로 호출하는 풀 반환 메서드
    /// </summary>
    public void Return(CHitImpactFX fx)
    {
        fx.gameObject.SetActive(false);
        _pool.Enqueue(fx);
    }
    #endregion

    #region Private Methods
    private void InitPool()
    {
        _pool = new Queue<CHitImpactFX>(_poolSize);

        for (int i = 0; i < _poolSize; i++)
        {
            CHitImpactFX fx = Instantiate(_prefab, transform);
            fx.gameObject.SetActive(false);
            _pool.Enqueue(fx);
        }
    }

    private CHitImpactFX GetFromPool()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();

        Debug.LogWarning("[CHitImpactPoolManager] 풀 고갈 — 긴급 생성 발생. Inspector의 _poolSize 증가를 권장합니다.", this);
        return Instantiate(_prefab, transform);
    }
    #endregion
}
