using UnityEngine;

/// <summary>
/// 스테이지 1개의 정적 설계 데이터를 보관하는 ScriptableObject
/// 인스펙터에서 직접 수치를 조정할 수 있도록 코드와 데이터를 분리한다
/// 런타임 상태(현재 킬수, 보스 처치 여부 등)는 절대 여기에 저장하지 않는다
/// 상태 데이터는 CStageManager가 별도로 관리하여 책임을 명확하게 분리한다
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "IdleTillDawn/StageData")]
public class CStageData : ScriptableObject
{
    #region Inspector Variables

    [Header("스테이지 식별")]
    [SerializeField] public int _world; // 월드 번호 (1, 2, 3...)
    [SerializeField] public int _stage; // 스테이지 번호 (1~10)

    [Header("일반 몬스터 설정")]
    [SerializeField] public int    _killGoal;       // 보스 도전 조건 처치 수
    [SerializeField] public float  _spawnInterval;  // 일반 몬스터 스폰 주기 (초)
    [SerializeField] public int    _maxActiveCount; // 동시 활성 최대 적 수

    [Header("보스 설정")]
    [SerializeField] public GameObject _bossPrefab; // 이 스테이지에서 등장할 보스 프리팹

    [Header("스탯 스케일링 배율")]
    [SerializeField] public float _hpMultiplier;     // 일반 몬스터 HP 배율
    [SerializeField] public float _bossHpMultiplier;  // 보스 HP 배율
    [SerializeField] public float _bossAtkMultiplier; // 보스 공격력 배율

    #endregion

    #region Properties

    /// <summary>
    /// 스테이지 고유 인덱스를 반환한다
    /// (월드-1)×10 + (스테이지-1) 공식으로 0-based 선형 인덱스를 생성한다
    /// 저장 및 배열 접근의 기준 키로 활용된다
    /// </summary>
    public int StageIndex => (_world - 1) * 10 + (_stage - 1); // 0-based 전체 인덱스

    /// <summary>
    /// 마지막 스테이지(X-10) 여부를 반환한다
    /// True이면 클리어 시 다음 월드 씬 전환을 트리거한다
    /// </summary>
    public bool IsLastStage => _stage == 10; // 씬 전환 트리거 조건

    #endregion
}
