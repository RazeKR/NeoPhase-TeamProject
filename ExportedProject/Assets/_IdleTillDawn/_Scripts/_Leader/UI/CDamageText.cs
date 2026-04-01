using UnityEngine;

/// <summary>
/// 피격 시 화면에 표시되는 월드 스페이스 데미지 수치 텍스트 컴포넌트
/// Legacy TextMesh를 사용하여 Canvas 없이 3D 월드 공간에서 직접 렌더링한다
/// CDamageTextPoolManager에 의해 풀 방식으로 관리되며,
/// Update에서 위 방향 이동과 알파 페이드를 처리한 뒤 스스로 풀에 반환한다
/// </summary>
[RequireComponent(typeof(TextMesh))]
public class CDamageText : MonoBehaviour
{
    #region 인스펙터
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 1.5f; // 위쪽 이동 속도 (units/sec)

    [Header("페이드 설정")]
    [SerializeField] private float _fadeDuration = 0.8f; // 활성화 후 완전히 투명해질 때까지 걸리는 시간 (초)

    [Header("오프셋 설정")]
    [SerializeField] private float _hitDirOffset = 0.7f;      // 피격 방향 기준 생성 오프셋 거리 (권장 0.5~1.0)
    [SerializeField] private float _randomOffsetRadius = 0.2f; // 자연스러움을 위한 랜덤 오프셋 반경

    [Header("색상 설정")]
    [SerializeField] private Color _normalColor   = Color.yellow; // 일반 피격 텍스트 색상 (몬스터 HitFlash 흰색과 구분)
    [SerializeField] private Color _criticalColor = Color.red;    // 크리티컬 히트 시 텍스트 색상

    [Header("폰트 크기 설정")]
    [SerializeField] private int _criticalFontSize = 22;  // 크리티컬 히트 시 폰트 크기 (고정)
    [SerializeField] private int _normalFontSizeMin = 14; // 일반 히트 폰트 크기 최솟값 (랜덤)
    [SerializeField] private int _normalFontSizeMax = 18; // 일반 히트 폰트 크기 최댓값 (랜덤)

    [Header("렌더링 설정")]
    [SerializeField] private int _sortingOrder = 32767; // TextMesh MeshRenderer 소팅 순서 — 최상단 표시용 최대값 권장
    #endregion

    #region 내부 변수
    private TextMesh _textMesh;  // 수치를 표시하는 Legacy TextMesh 컴포넌트
    private float    _timer;     // Init 호출 이후 경과 시간 (페이드 비율 계산 기준)
    private Color    _baseColor; // Init 시 결정된 기본 색상 (알파 연산 기준값)
    private bool     _isRunning; // Update에서 이동/페이드 처리 여부를 제어하는 플래그
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _textMesh = GetComponent<TextMesh>();

        // TextMesh는 MeshRenderer로 렌더링된다
        // SpriteRenderer의 sortingOrder와 달리 Inspector에서 자동 노출되지 않으므로 코드에서 직접 설정해야 한다
        // 32767(short.MaxValue)로 설정하여 몬스터·배경 스프라이트 위에 항상 표시한다
        GetComponent<MeshRenderer>().sortingOrder = _sortingOrder;
    }

    /// <summary>
    /// 매 프레임마다 위쪽 이동과 선형 알파 페이드를 처리한다
    /// _isRunning이 false인 비활성 인스턴스는 연산을 완전히 건너뛰어 성능을 절약한다
    /// </summary>
    private void Update()
    {
        if (!_isRunning) return;

        _timer += Time.deltaTime;

        // 위쪽으로 이동 (월드 좌표 기준 Vector3.up)
        transform.position += Vector3.up * (_moveSpeed * Time.deltaTime);

        // 선형 알파 감소 : 0초→불투명, _fadeDuration초→완전 투명
        float alpha = Mathf.Clamp01(1f - (_timer / _fadeDuration));
        _textMesh.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);

        // 페이드 완료 시 스스로 풀에 반환
        if (_timer >= _fadeDuration)
            ReturnToPool();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 데미지 텍스트를 초기화하고 애니메이션을 시작한다
    /// CDamageTextPoolManager가 풀에서 꺼낸 직후 호출해야 한다
    /// </summary>
    /// <param name="damage">화면에 표시할 데미지 수치</param>
    /// <param name="originPosition">기준 월드 좌표 (주로 피격 대상의 transform.position)</param>
    /// <param name="hitDir">피격 방향 벡터 — 텍스트 생성 위치의 방향 오프셋 결정에 사용 (정규화 불필요)</param>
    /// <param name="isCritical">크리티컬 히트 여부 — 색상과 폰트 크기에 영향을 준다</param>
    public void Init(int damage, Vector3 originPosition, Vector2 hitDir, bool isCritical = false)
    {
        // 수치 텍스트 설정
        _textMesh.text = damage.ToString();

        // 크리티컬 여부에 따라 색상·폰트 크기 분기
        // 일반 피격은 _normalColor(기본 노란색)로 표시 — 몬스터 스프라이트 HitFlash(흰색)와 시각적으로 구분된다
        _baseColor = isCritical ? _criticalColor : _normalColor;
        _textMesh.color = _baseColor;
        _textMesh.fontSize = isCritical
            ? _criticalFontSize
            : Random.Range(_normalFontSizeMin, _normalFontSizeMax + 1);

        // 생성 위치 계산 :
        //   피격 방향 오프셋으로 몬스터 중심에서 적절히 벗어나고
        //   Random.insideUnitCircle로 미세한 랜덤성을 추가하여 동시 다중 피격 시에도 텍스트가 겹치지 않는다
        Vector2 randomOffset  = Random.insideUnitCircle * _randomOffsetRadius;
        Vector3 dirOffset     = (Vector3)(hitDir.normalized * _hitDirOffset);
        transform.position    = originPosition + dirOffset + (Vector3)randomOffset;

        // 타이머 리셋 및 동작 시작
        _timer     = 0f;
        _isRunning = true;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 페이드 완료 후 오브젝트를 비활성화하고 풀 매니저에 반환한다
    /// CDamageTextPoolManager.Instance가 없는 씬에서는 단순히 비활성화만 한다
    /// </summary>
    private void ReturnToPool()
    {
        _isRunning = false;
        CDamageTextPoolManager.Instance?.Return(this);
    }
    #endregion
}
