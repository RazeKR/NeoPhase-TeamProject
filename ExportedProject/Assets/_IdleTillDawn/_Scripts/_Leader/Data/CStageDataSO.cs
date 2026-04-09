using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스테이지의 구성(스폰 몬스터, 보스, 보상)을 ID 기반으로 정의하는 ScriptableObject입니다.
/// 기존 CStageData(프리팹 직접 참조 방식)와 병행 사용하며,
/// DataManager 기반 신규 코드는 이 클래스를 통해 스테이지 데이터를 참조합니다.
/// CDataManager.GetStage(id)를 통해서만 접근합니다.
/// </summary>
[CreateAssetMenu(fileName = "StageData_", menuName = "IdleTillDawn/Data/StageData")]
public class CStageDataSO : CBaseDataSO
{
    #region InspectorVariables

    [Header("스테이지 식별")]
    [SerializeField] private int _world;          // 월드 번호 (1-based)
    [SerializeField] private int _stageNumber;    // 스테이지 번호 (1-based, 해당 월드 내 순번)

    [Header("몬스터 스폰 설정")]
    [SerializeField] private int _killGoal = 30;                  // 보스 도전 조건 처치 수
    [SerializeField] private int _maxActiveCount = 10;            // 동시 활성 최대 몬스터 수
    [SerializeField] private float _spawnInterval = 2f;           // 몬스터 스폰 주기 (초)

    [Header("보스 설정")]
    [SerializeField] private int _bossId;                    // 보스 데이터 ID (0 = 보스 없음)

    #endregion

    #region Properties

    public int            World              => _world;
    public int            StageNumber        => _stageNumber;
    public int            KillGoal           => _killGoal;
    public int            MaxActiveCount     => _maxActiveCount;
    public float          SpawnInterval      => _spawnInterval;
    public int            BossId             => _bossId;

    /// <summary>보스가 존재하는 스테이지인지 여부. BossId가 0보다 크면 보스 있음.</summary>
    public bool HasBoss => _bossId > 0;

    /// <summary>마지막 스테이지(X-10) 여부. 클리어 시 다음 월드로 전환됩니다.</summary>
    public bool IsLastStage => _stageNumber == 10;

    /// <summary>
    /// 0-based 전체 스테이지 인덱스입니다.
    /// (월드-1)×10 + (스테이지번호-1) 공식으로 계산되며 CGameManager와 호환됩니다.
    /// </summary>
    public int StageIndex => (_world - 1) * 10 + (_stageNumber - 1);

    #endregion
}
