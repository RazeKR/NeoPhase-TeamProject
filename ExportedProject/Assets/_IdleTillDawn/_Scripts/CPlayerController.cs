using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 키보드 입력을 받아 물리 기반 이동을 처리하는 컨트롤러 클래스
/// 방향키(Horizontal / Vertical Axis)를 통해 Rigidbody2D에 속도를 직접 부여한다
/// GetAxisRaw를 사용하여 입력 딜레이 없이 즉각적인 반응을 보장한다
/// 테스트용 킬 반경(_killRadius)을 보유하며 범위 내 적을 즉시 풀로 반환한다
/// </summary>
public class CPlayerController : MonoBehaviour
{
    #region Inspector Variables

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f; // 플레이어 이동 속도 (단위: Unity 유닛/초)

    [Header("Test - Kill Zone")]
    [SerializeField] private CEnemySpawner _spawner;    // 킬 처리 시 ReturnToPool을 호출할 스포너 참조
    [SerializeField] private float _killRadius = 1.5f;  // 이 반경 안에 들어온 적은 즉시 제거 (테스트용)

    #endregion

    #region Private Variables

    private Rigidbody2D rb;                           // 물리 이동 처리를 위해 캐싱한 Rigidbody2D 컴포넌트
    private readonly List<GameObject> killBuffer = new(); // KillNearbyEnemies 처리 시 임시 보관 버퍼 (매 프레임 할당 방지)

    #endregion

    #region Unity Methods

    /// <summary>
    /// 게임 오브젝트가 활성화될 때 최초 1회 호출된다
    /// 매 프레임 GetComponent 호출을 피하기 위해 Start에서 컴포넌트를 캐싱한다
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 매 프레임 호출된다
    /// 수평/수직 축 입력값을 정규화하여 대각선 이동 시에도 속도가 일정하게 유지되도록 한다
    /// velocity에 직접 할당하여 물리 엔진의 마찰 등 외력 없이 순수 이동만 처리한다
    /// 스포너가 연결된 경우 킬 반경 내 적을 매 프레임 검사하여 즉시 제거한다
    /// </summary>
    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"); // 수평 입력값 (-1, 0, 1)
        float y = Input.GetAxisRaw("Vertical");   // 수직 입력값 (-1, 0, 1)

        rb.velocity = new Vector2(x, y).normalized * _moveSpeed;

        KillNearbyEnemies();
    }

    /// <summary>
    /// 에디터에서 킬 반경을 시각적으로 확인할 수 있도록 기즈모 원을 그린다
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _killRadius); // 킬 반경 시각화
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 스포너의 활성 적 목록을 순회하여 _killRadius 이내에 있는 적을 모두 풀로 반환한다
    /// IReadOnlyCollection 순회 중 수정을 막기 위해 대상을 killBuffer에 먼저 수집한 뒤 일괄 처리한다
    /// 스포너 참조가 없을 경우 즉시 반환하여 불필요한 연산을 방지한다
    /// </summary>
    private void KillNearbyEnemies()
    {
        if (_spawner == null) return;

        killBuffer.Clear();

        foreach (GameObject enemy in _spawner.ActiveEnemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position); // 플레이어 ~ 적 거리
            if (dist <= _killRadius) killBuffer.Add(enemy);
        }

        foreach (GameObject enemy in killBuffer)
            _spawner.ReturnToPool(enemy); // 킬 = 풀 반환 (비활성화)
    }

    #endregion
}
