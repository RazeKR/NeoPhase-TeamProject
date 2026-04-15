using UnityEngine;


/// <summary>펫의 종류를 구분하는 열거형입니다.</summary>
public enum EPetType
{
    ProjectileBoost,  // 플레이어 투사체 증가 - 플레이어 기준 원형 회전, 2초마다 투사체 발사
    AttackPowerBoost, // 플레이어 공격력 증가 - 플레이어 기준 원형 회전, 적 접촉 시 근거리 데미지
    AttackSpeedBoost  // 플레이어 공격속도 증가 - 플레이어 기준 원형 회전, 2초마다 투사체 발사
}


/// <summary>
/// 펫 데이터 ScriptableObject입니다.
/// CItemDataSO를 상속하므로 CDataManager를 통해 int ID 기반으로 접근 가능합니다.
///
/// [등급 (rank)]
///   0 = Common / 1 = Rare / 2 = Epic / 3 = Legendary
///
/// [등급별 공통 버프]
///   경험치 증가: 기본 50%, 등급 1 오를 때마다 25% 추가
///
/// [타입별 등급 버프]
///   ProjectileBoost  : 기본 +1개, 등급당 +1개
///   AttackPowerBoost : 기본 +20%, 등급당 +20%
///   AttackSpeedBoost : 기본 +20%, 등급당 +20%
///
/// [강화 (upgrade, 최대 10)]
///   펫 기본 공격력 : 100 + upgrade × 100
///   ProjectileBoost  : upgrade × 1개 추가
///   AttackPowerBoost : upgrade × 10% 추가
///   AttackSpeedBoost : upgrade × 10% 추가
/// </summary>
[CreateAssetMenu(menuName = "SO/Data/PetData", fileName = "PetData_")]
public class CPetDataSO : CItemDataSO
{
    #region Inspector

    [Header("펫 종류")]
    [SerializeField] private EPetType _petType;

    [Header("투사체 발사 설정 (ProjectileBoost / AttackSpeedBoost 전용)")]
    [Tooltip("총알 프리팹, 속도, 생존 시간 등을 설정하는 무기 SO를 연결합니다.")]
    [SerializeField] private CWeaponDataSO _bulletConfig = null;
    [Tooltip("투사체 발사 간격(초). 기본값 2초.")]
    [SerializeField] private float _fireInterval = 2f;
    [Tooltip("한 번에 발사하는 투사체 수. 2 이상이면 SpreadAngle 간격으로 퍼져 나갑니다.")]
    [SerializeField] private int _projectileCount = 1;
    [Tooltip("다중 투사체일 때 인접 투사체 사이 각도(도). 3발이면 -Angle, 0, +Angle 방향으로 발사됩니다.")]
    [SerializeField] private float _projectileSpreadAngle = 15f;

    [Header("화상 상태이상 (ProjectileBoost / AttackSpeedBoost 전용)")]
    [Tooltip("투사체 피격 시 화상 상태이상을 적용합니다.")]
    [SerializeField] private bool  _applyBurnOnHit   = false;
    [Tooltip("화상 지속 시간(초)")]
    [SerializeField] private float _burnDuration     = 3f;
    [Tooltip("화상 틱당 데미지")]
    [SerializeField] private float _burnTickDamage   = 10f;
    [Tooltip("화상 틱 간격(초)")]
    [SerializeField] private float _burnTickInterval = 1f;

    [Header("스프라이트 애니메이션 (ProjectileBoost / AttackSpeedBoost 전용)")]
    [SerializeField] private Sprite[] _idleSprites  = null;
    [SerializeField] private Sprite[] _attackSprites = null;

    [Header("단일 회전 스프라이트 (AttackPowerBoost 전용)")]
    [Tooltip("플레이어 기준 원형 궤도를 돌며 스프라이트 이미지 자체가 회전합니다.")]
    [SerializeField] private Sprite _meleeSprite = null;
    [Tooltip("궤도 회전과 별개로 스프라이트가 자체 축을 기준으로 자전하는 속도(도/초). 기본 360(1초에 1바퀴).")]
    [SerializeField] private float _selfRotationSpeed = 360f;

    [Header("궤도 설정 (공통)")]
    [Tooltip("플레이어로부터의 궤도 반경")]
    [SerializeField] private float _orbitRadius = 2f;
    [Tooltip("시계 방향 회전 속도(도/초)")]
    [SerializeField] private float _orbitSpeed  = 90f;

    #endregion

    #region Properties

    public EPetType PetType              => _petType;
    public CWeaponDataSO BulletConfig   => _bulletConfig;
    public float FireInterval           => _fireInterval;
    public int   ProjectileCount        => Mathf.Max(1, _projectileCount);
    public float ProjectileSpreadAngle  => _projectileSpreadAngle;
    public bool  ApplyBurnOnHit         => _applyBurnOnHit;
    public float BurnDuration           => _burnDuration;
    public float BurnTickDamage         => _burnTickDamage;
    public float BurnTickInterval       => _burnTickInterval;
    public Sprite[] IdleSprites         => _idleSprites;
    public Sprite[] AttackSprites       => _attackSprites;
    public Sprite MeleeSprite           => _meleeSprite;
    public float SelfRotationSpeed      => _selfRotationSpeed;
    public float OrbitRadius            => _orbitRadius;
    public float OrbitSpeed             => _orbitSpeed;

    #endregion

    #region Grade Buff Calculations  (rank: 0=Common ~ 3=Legendary)

    /// <summary>경험치 획득량 증가 퍼센트. 기본 50%, 등급당 +25%, 강화당 +10%.</summary>
    public float GetXpBoostPercent(int rank, int upgrade = 0) => 50f + rank * 25f + upgrade * 10f;

    /// <summary>투사체 증가 수량 (등급 기반). 기본 1, 등급당 +1.</summary>
    public int GetGradeProjectileBonus(int rank) => 1 + rank;

    /// <summary>공격력 증가 퍼센트 (등급 기반). 기본 20%, 등급당 +20%.</summary>
    public float GetGradeAttackPowerPercent(int rank) => 20f + rank * 20f;

    /// <summary>공격속도 증가 퍼센트 (등급 기반). 기본 20%, 등급당 +20%.</summary>
    public float GetGradeAttackSpeedPercent(int rank) => 20f + rank * 20f;

    #endregion

    #region Enhancement Buff Calculations  (upgrade: 0 ~ 10)

    /// <summary>펫 자체 공격력. 기본 100, 강화 1단계당 +100.</summary>
    public float GetPetAttackPower(int upgrade) => 100f + upgrade * 100f;

    /// <summary>강화로 추가되는 투사체 수량. 강화 1단계당 +1.</summary>
    public int GetUpgradeProjectileBonus(int upgrade) => upgrade;

    /// <summary>강화로 추가되는 공격력 퍼센트. 강화 1단계당 +10%.</summary>
    public float GetUpgradeAttackPowerPercent(int upgrade) => upgrade * 10f;

    /// <summary>강화로 추가되는 공격속도 퍼센트. 강화 1단계당 +10%.</summary>
    public float GetUpgradeAttackSpeedPercent(int upgrade) => upgrade * 10f;

    #endregion

    #region Combined Buff Helpers  (rank + upgrade)

    /// <summary>등급 + 강화 합산 투사체 증가 수량.</summary>
    public int GetTotalProjectileBonus(int rank, int upgrade)
        => GetGradeProjectileBonus(rank) + GetUpgradeProjectileBonus(upgrade);

    /// <summary>등급 + 강화 합산 공격력 증가 퍼센트.</summary>
    public float GetTotalAttackPowerPercent(int rank, int upgrade)
        => GetGradeAttackPowerPercent(rank) + GetUpgradeAttackPowerPercent(upgrade);

    /// <summary>등급 + 강화 합산 공격속도 증가 퍼센트.</summary>
    public float GetTotalAttackSpeedPercent(int rank, int upgrade)
        => GetGradeAttackSpeedPercent(rank) + GetUpgradeAttackSpeedPercent(upgrade);

    #endregion
}
