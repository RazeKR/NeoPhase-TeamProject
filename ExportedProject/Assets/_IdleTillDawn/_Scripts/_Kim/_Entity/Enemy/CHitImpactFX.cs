using System.Collections;
using UnityEngine;

/// <summary>
/// 적 피격 시 몸통 위에 표시되는 2단계 스프라이트 이펙트 컴포넌트
/// Phase 0 : HitImpactFX_0 (꽉 찬 원) — 즉시 표시 후 짧게 유지
/// Phase 1 : HitImpactFX_1 (링)        — 점진적 페이드아웃
/// 재생 완료 후 CHitImpactPoolManager에 스스로 반환되어 풀 재사용된다
/// </summary>
public class CHitImpactFX : MonoBehaviour
{
    #region 인스펙터
    [Header("스프라이트 렌더러")]
    [SerializeField] private SpriteRenderer _renderer0; // HitImpactFX_0 : 꽉 찬 원 렌더러
    [SerializeField] private SpriteRenderer _renderer1; // HitImpactFX_1 : 링 렌더러

    [Header("타이밍 설정")]
    [SerializeField] private float _phase0Duration    = 0.07f; // FX_0 유지 시간 (초)
    [SerializeField] private float _phase1FadeDuration = 0.13f; // FX_1 페이드아웃 시간 (초)
    #endregion

    #region 내부 변수
    private Coroutine _playCoroutine;
    #endregion

    /// <summary>
    /// 피격 방향에 따른 월드 좌표에 FX를 배치하고 재생을 시작한다
    /// </summary>
    /// <param name="enemyWorldPos">적의 월드 중심 좌표</param>
    /// <param name="hitDir">피격 방향 벡터 (공격자 → 피격자)</param>
    /// <param name="hitRadius">적 콜라이더 반경 — FX 표시 거리 계산에 사용</param>
    public void Init(Vector3 enemyWorldPos, Vector2 hitDir, float hitRadius)
    {
        // 피격 방향으로 반경의 65% 지점에 FX 배치 (몸통 표면 근처)
        transform.position = enemyWorldPos + (Vector3)(hitDir.normalized * hitRadius * 0.65f);

        if (_playCoroutine != null)
            StopCoroutine(_playCoroutine);

        _playCoroutine = StartCoroutine(Co_Play());
    }

    #region 코루틴
    private IEnumerator Co_Play()
    {
        // --- Phase 0 : 꽉 찬 원 즉시 표시 ---
        _renderer0.enabled = true;
        _renderer1.enabled = false;
        SetAlpha(_renderer0, 1f);

        yield return new WaitForSeconds(_phase0Duration);

        // --- Phase 1 : 링 페이드아웃 ---
        _renderer0.enabled = false;
        _renderer1.enabled = true;
        SetAlpha(_renderer1, 1f);

        float elapsed = 0f;
        while (elapsed < _phase1FadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(_renderer1, Mathf.Clamp01(1f - elapsed / _phase1FadeDuration));
            yield return null;
        }

        _renderer1.enabled = false;
        _playCoroutine = null;

        CHitImpactPoolManager.Instance?.Return(this);
    }
    #endregion

    #region Private Methods
    private static void SetAlpha(SpriteRenderer sr, float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
    #endregion

    #region Unity Methods
    /// <summary>
    /// 풀 반환(SetActive false) 시 코루틴 중단 및 렌더러 상태 초기화
    /// </summary>
    private void OnDisable()
    {
        if (_playCoroutine != null)
        {
            StopCoroutine(_playCoroutine);
            _playCoroutine = null;
        }

        if (_renderer0 != null) { _renderer0.enabled = false; SetAlpha(_renderer0, 1f); }
        if (_renderer1 != null) { _renderer1.enabled = false; SetAlpha(_renderer1, 1f); }
    }
    #endregion
}
