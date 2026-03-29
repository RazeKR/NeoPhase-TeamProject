using UnityEngine;

/// <summary>
/// 일반 몬스터의 기본 스탯, 스테이지당 성장치, 드롭 및 보상 정보를 정의하는 ScriptableObject입니다.
/// CDataManager.GetMonster(id)를 통해서만 접근합니다.
/// </summary>
[CreateAssetMenu(menuName = "IdleTillDawn/Data/EnemyData", fileName = "EnemyData_")]
public class CEnemyDataSO : CBaseDataSO
{
    #region InspectorVariables

    [Header("기본 정보")]
    [SerializeField] private string _enemyName = "Enemy Name";       // 몬스터 이름
    [SerializeField] private string _description = "Description";    // 몬스터 설명

    [Header("기본 스탯")]
    [SerializeField] private float _baseHealth = 50f;      // 기본 체력
    [SerializeField] private float _baseDamage = 10f;      // 기본 공격력
    [SerializeField] private float _attackCooltime = 1.5f; // 공격 쿨타임 (초)
    [SerializeField] private float _moveSpeed = 3f;        // 이동 속도
    [SerializeField] private float _attackRange = 1.0f;    // 공격 사거리

    [Header("스테이지당 성장 스탯")]
    [SerializeField] private float _healthGrowthPerStage = 10f;  // 스테이지당 체력 증가량
    [SerializeField] private float _damageGrowthPerStage = 1f;   // 스테이지당 공격력 증가량

    [Header("소환 오브젝트")]
    [SerializeField] private GameObject _prefab; // 몬스터 프리팹

    [Header("드롭 및 보상")]
    [SerializeField] private int _dropItemId = 0;       // 드롭 아이템 ID (0 = 없음, DataManager 조회)
    [SerializeField] private float _dropChance = 0.1f;  // 아이템 드롭 확률 (0~1)
    [SerializeField] private int _expReward = 10;        // 기본 경험치 보상

    #endregion

    #region Properties

    public string    EnemyName             => _enemyName;
    public string    Description           => _description;
    public float     BaseHealth            => _baseHealth;
    public float     BaseDamage            => _baseDamage;
    public float     AttackCooltime        => _attackCooltime;
    public float     MoveSpeed             => _moveSpeed;
    public float     AttackRange           => _attackRange;
    public float     HealthGrowthPerStage  => _healthGrowthPerStage;
    public float     DamageGrowthPerStage  => _damageGrowthPerStage;
    public GameObject Prefab              => _prefab;
    public int       DropItemId            => _dropItemId;
    public float     DropChance            => _dropChance;
    public int       ExpReward             => _expReward;

    #endregion

    #region PublicMethods

    /// <summary>스테이지 인덱스에 따른 스케일된 체력을 반환합니다.</summary>
    public float GetHealthForStage(int stageIndex) => _baseHealth + _healthGrowthPerStage * stageIndex;

    /// <summary>스테이지 인덱스에 따른 스케일된 공격력을 반환합니다.</summary>
    public float GetDamageForStage(int stageIndex) => _baseDamage + _damageGrowthPerStage * stageIndex;

    #endregion
}
