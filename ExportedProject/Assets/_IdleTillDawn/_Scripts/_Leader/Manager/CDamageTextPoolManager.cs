using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CDamageText 오브젝트 풀을 관리하는 싱글턴 매니저
/// 런타임 중 Instantiate / Destroy 없이 SetActive 기반 재사용으로 GC 스파이크를 방지한다
/// CEntityBase.TakeDamage(float, Vector2) 에서 호출되며,
/// CDamageText가 페이드 완료 후 스스로 Return을 호출하여 풀에 복귀한다
/// </summary>
public class CDamageTextPoolManager : MonoBehaviour
{
    #region 인스펙터
    [Header("풀 설정")]
    [SerializeField] private CDamageText _prefab;   // CDamageText 컴포넌트가 부착된 프리팹
    [SerializeField] private int _poolSize = 20;    // 씬 시작 시 미리 생성할 풀 크기 (동시 최대 표시 수 이상으로 설정)
    #endregion

    #region 내부 변수
    private Queue<CDamageText> _pool; // 비활성 상태인 CDamageText 인스턴스 큐
    #endregion

    #region 프로퍼티
    /// <summary>씬 전역에서 단일 접근점을 제공하는 싱글턴 인스턴스</summary>
    public static CDamageTextPoolManager Instance { get; private set; }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // 씬 중복 인스턴스 방지 : 이미 존재하면 자신을 파괴
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
    /// 풀에서 CDamageText를 꺼내 지정 위치에 초기화하고 활성화한다
    /// 피격 방향·크리티컬 여부에 따른 시각 차별화를 지원한다
    /// </summary>
    /// <param name="damage">화면에 표시할 데미지 수치</param>
    /// <param name="worldPosition">텍스트 생성 기준 월드 좌표 (피격 대상의 transform.position)</param>
    /// <param name="hitDir">피격 방향 벡터 — CDamageText 내부에서 오프셋 방향으로 사용된다</param>
    /// <param name="isCritical">크리티컬 히트 여부 — 색상 및 폰트 크기 분기에 사용된다</param>
    public void ShowDamage(int damage, Vector3 worldPosition, Vector2 hitDir, bool isCritical = false)
    {
        CDamageText text = GetFromPool();
        text.gameObject.SetActive(true);
        text.Init(damage, worldPosition, hitDir, isCritical);
    }

    /// <summary>
    /// CDamageText가 페이드 완료 후 스스로 호출하는 풀 반환 메서드
    /// 비활성화 후 큐에 다시 삽입하여 재사용 가능 상태로 전환한다
    /// </summary>
    /// <param name="text">반환할 CDamageText 인스턴스</param>
    public void Return(CDamageText text)
    {
        text.gameObject.SetActive(false);
        _pool.Enqueue(text);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 씬 시작 시 _poolSize만큼 CDamageText를 미리 Instantiate하고 비활성화하여 풀을 초기화한다
    /// 이 시점 이후로는 정상 사용 시 추가 Instantiate가 발생하지 않는다
    /// </summary>
    private void InitPool()
    {
        _pool = new Queue<CDamageText>(_poolSize);

        for (int i = 0; i < _poolSize; i++)
        {
            CDamageText text = Instantiate(_prefab, transform);
            text.gameObject.SetActive(false);
            _pool.Enqueue(text);
        }
    }

    /// <summary>
    /// 풀에서 비활성 CDamageText를 꺼낸다
    /// 풀이 고갈된 경우 경고와 함께 긴급 생성한다 — _poolSize를 늘려 예방하는 것을 권장한다
    /// </summary>
    /// <returns>사용 준비된 CDamageText 인스턴스 (비활성 상태)</returns>
    private CDamageText GetFromPool()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();

        // 풀 고갈 방어 처리 : GC 발생이 허용되므로 _poolSize 증가로 근본 원인을 해결할 것
        CDebug.LogWarning("[CDamageTextPoolManager] 풀 고갈 — 긴급 생성 발생. Inspector의 _poolSize 증가를 권장합니다.", this);
        return Instantiate(_prefab, transform);
    }
    #endregion
}
