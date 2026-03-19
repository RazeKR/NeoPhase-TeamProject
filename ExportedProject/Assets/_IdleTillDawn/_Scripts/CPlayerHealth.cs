using UnityEngine;

/// <summary>
/// 플레이어의 체력을 관리하는 컴포넌트
/// 보스의 공격을 받아 체력이 감소하고, 체력이 0 이하가 되면 사망 상태를 반환한다
/// CBoss가 이 컴포넌트를 참조하여 데미지를 전달하며, CBossManager는 IsDead를 체크하여 결과를 판정한다
/// </summary>
public class CPlayerHealth : MonoBehaviour
{
    #region Inspector Variables

    [Header("체력 설정")]
    [SerializeField] private float _maxHp = 100f; // 플레이어 최대 체력

    #endregion

    #region Private Variables

    private float currentHp; // 현재 체력 (피격 시 감소)

    #endregion

    #region Properties

    /// <summary>
    /// 플레이어 사망 여부를 반환한다
    /// CBoss.Attack에서 데미지 전달 후 즉시 체크하여 사망 이벤트 발행 여부를 결정한다
    /// </summary>
    public bool IsDead => currentHp <= 0f; // 체력 0 이하 시 사망 판정

    /// <summary>현재 체력 비율을 반환한다 (HP 바 UI 갱신에 활용)</summary>
    public float HpRatio => currentHp / _maxHp; // 0.0 ~ 1.0 범위

    #endregion

    #region Unity Methods

    /// <summary>
    /// 씬 시작 시 체력을 최대값으로 초기화한다
    /// 씬 리로드 방식으로 리스폰하므로 별도의 리셋 메서드 없이 Start만으로 충분하다
    /// </summary>
    private void Start() => currentHp = _maxHp; // 체력 초기화

    #endregion

    #region Public Methods

    /// <summary>
    /// 외부에서 호출하는 피격 처리 메서드
    /// 이미 사망 상태이면 추가 데미지를 무시하여 중복 사망 이벤트를 방지한다
    /// </summary>
    /// <param name="damage">받는 피해량</param>
    public void TakeDamage(float damage)
    {
        if (IsDead) return; // 이미 사망 상태이면 무시
        currentHp -= damage;
        currentHp  = Mathf.Max(currentHp, 0f); // 음수 방지
    }

    #endregion
}
