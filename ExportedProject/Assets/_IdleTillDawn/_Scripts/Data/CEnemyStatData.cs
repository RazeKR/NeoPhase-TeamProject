using UnityEngine;

/// <summary>
/// 적 유닛의 기본 스탯과 스테이지별 스케일링 공식을 보관하는 ScriptableObject
/// 스케일링 공식을 코드가 아닌 데이터로 분리하여 밸런싱 가능하도록 설계한다
/// 적 종류별로 개별 인스턴스를 생성하고 프리팹에 연결하는 방식으로 사용한다
/// </summary>
[CreateAssetMenu(fileName = "EnemyStatData", menuName = "IdleTillDawn/EnemyStatData")]
public class CEnemyStatData : ScriptableObject
{
    #region Inspector Variables

    [Header("기본 스탯 (스테이지 1-1 기준)")]
    [SerializeField] public float _baseHp;        // 1-1 기준 체력
    [SerializeField] public float _baseMoveSpeed; // 기본 이동 속도

    [Header("스케일링 계수")]
    [SerializeField] public float _hpGrowthRate = 0.1f; // 스테이지당 HP 증가율 (0.1 = 10%)

    #endregion

    #region Public Methods

    /// <summary>
    /// 주어진 스테이지 인덱스에 맞는 최종 HP를 계산하여 반환한다
    /// 공식: baseHp × (1 + growthRate × stageIndex)
    /// 선형 증가 방식을 채택하여 초반 진입장벽을 낮추고 후반 난이도를 점진적으로 상승시킨다
    /// 지수 증가가 필요한 경우 growthRate를 Mathf.Pow로 교체하면 된다
    /// </summary>
    /// <param name="stageIndex">0-based 전체 스테이지 인덱스 (CStageData.StageIndex)</param>
    /// <returns>스테이지 배율이 적용된 최종 HP 값</returns>
    public float CalculateHp(int stageIndex) =>
        _baseHp * (1f + _hpGrowthRate * stageIndex); // 선형 스케일링

    #endregion
}
