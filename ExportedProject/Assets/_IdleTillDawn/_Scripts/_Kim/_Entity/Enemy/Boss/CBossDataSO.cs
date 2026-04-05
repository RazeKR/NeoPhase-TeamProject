using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 몬스터의 패턴 설정, 페이즈 임계값, 보상 정보를 정의하는 ScriptableObject입니다.
/// CEnemyDataSO를 상속하여 일반 몬스터 스탯을 모두 포함합니다.
/// CDataManager.GetBoss(id)를 통해서만 접근합니다.
/// </summary>
[CreateAssetMenu(menuName = "IdleTillDawn/Data/BossData", fileName = "BossData_")]
public class CBossDataSO : CEnemyDataSO
{
    #region InspectorVariables

    [Header("패턴 설정")]
    [SerializeField] private float _specialAttackCooldown = 5f;   // 특수 공격 쿨타임 (초)
    [SerializeField] private GameObject _specialAttackEffect;     // 특수 공격 이펙트 프리팹
    [SerializeField] private float _telegraphDuration = 1f;       // 패턴 예고 시간 (초)
    [SerializeField] private int _maxActivePatternCount = 3;      // 동시 활성 패턴 최대 수

    [Header("페이즈 전환 임계값")]
    [SerializeField] private float _phase2Threshold = 0.5f;   // 2페이즈 전환 체력 비율 (0~1)
    [SerializeField] private float _phase3Threshold = 0.25f;  // 3페이즈 전환 체력 비율 (0~1)

    [Header("보상 정보")]
    [SerializeField] private List<int> _rewardItemIds = new(); // 처치 시 보상 아이템 ID 목록
    [SerializeField] private int _bossExpReward = 500;          // 처치 시 경험치 보상
    [SerializeField] private int _bossGoldReward = 1000;        // 처치 시 골드 보상

    [Header("사운드 (CEnemyDataSO.HitSFX 포함)")]
    [SerializeField] private CSoundData _attackSFX;  // 특수 공격 시전 시 재생 사운드

    #endregion

    #region Properties

    public float       SpecialAttackCooldown  => _specialAttackCooldown;
    public GameObject  SpecialAttackEffect    => _specialAttackEffect;
    public float       TelegraphDuration      => _telegraphDuration;
    public int         MaxActivePatternCount  => _maxActivePatternCount;
    public float       Phase2Threshold        => _phase2Threshold;
    public float       Phase3Threshold        => _phase3Threshold;
    public List<int>   RewardItemIds          => _rewardItemIds;
    public int         BossExpReward          => _bossExpReward;
    public int         BossGoldReward         => _bossGoldReward;

    /// <summary>보스 특수 공격 시전 시 재생할 사운드 데이터. 피격음은 부모 HitSFX 사용</summary>
    public CSoundData  AttackSFX              => _attackSFX;

    #endregion

    #region PublicMethods

    /// <summary>HP 배율을 적용한 스케일된 최대 체력을 반환합니다.</summary>
    public float GetScaledMaxHp(float multiplier) => BaseHealth * multiplier;

    /// <summary>공격 배율을 적용한 스케일된 공격력을 반환합니다.</summary>
    public float GetScaledAttack(float multiplier) => BaseDamage * multiplier;

    /// <summary>현재 HP 비율이 2페이즈 임계값 이하인지 확인합니다.</summary>
    public bool IsPhase2(float hpRatio) => hpRatio <= _phase2Threshold;

    /// <summary>현재 HP 비율이 3페이즈 임계값 이하인지 확인합니다.</summary>
    public bool IsPhase3(float hpRatio) => hpRatio <= _phase3Threshold;

    #endregion
}
