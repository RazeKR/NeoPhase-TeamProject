using UnityEngine;

/// <summary>
/// 스킬의 특정 레벨에서 적용되는 수치 변화를 정의하는 ScriptableObject입니다.
/// CSkillDataSO의 _levelData 리스트에서 인덱스(level - 1)로 참조됩니다.
/// 이 클래스는 CBaseDataSO를 상속하지 않습니다.
/// CSkillDataSO를 통해 간접 접근하므로 독립 ID가 필요하지 않습니다.
/// </summary>
[CreateAssetMenu(fileName = "SkillLevelData_", menuName = "IdleTillDawn/Data/SkillLevelData")]
public class CSkillLevelDataSO : ScriptableObject
{
    #region InspectorVariables

    [Header("레벨 정보")]
    [SerializeField] private int _level;                   // 해당 스킬 레벨 (1-based, 문서용)

    [Header("레벨별 수치 변화값")]
    [SerializeField] private float _damageMultiplier = 1f; // 기본 데미지에 곱하는 배율 (1.0 = 변화 없음)
    [SerializeField] private float _manaCostDelta = 0f;    // 마나 소모량 변화값 (음수 가능 - 감소)
    [SerializeField] private float _cooldownDelta = 0f;    // 쿨타임 변화값 (음수 가능 - 감소)
    [SerializeField] private float _durationDelta = 0f;    // 지속 시간 변화값 (음수 가능)
    [SerializeField] private float _rangeMultiplier = 1f;  // 범위 배율

    [Header("설명")]
    [SerializeField] private string _levelDescription;     // 이 레벨에서의 변화 설명 텍스트 (UI용)

    #endregion

    #region Properties

    public int    Level              => _level;
    public float  DamageMultiplier   => _damageMultiplier;
    public float  ManaCostDelta      => _manaCostDelta;
    public float  CooldownDelta      => _cooldownDelta;
    public float  DurationDelta      => _durationDelta;
    public float  RangeMultiplier    => _rangeMultiplier;
    public string LevelDescription   => _levelDescription;

    #endregion

    #region PublicMethods

    /// <summary>기본 데미지에 이 레벨의 배율을 적용한 최종 데미지를 반환합니다.</summary>
    public float GetFinalDamage(float baseDamage) => baseDamage * _damageMultiplier;

    /// <summary>기본 마나 소모량에 이 레벨의 변화값을 더한 최종 마나 소모량을 반환합니다. 최솟값은 0입니다.</summary>
    public float GetFinalManaCost(float baseManaCost) => Mathf.Max(0f, baseManaCost + _manaCostDelta);

    /// <summary>기본 쿨타임에 이 레벨의 변화값을 더한 최종 쿨타임을 반환합니다. 최솟값은 0.1초입니다.</summary>
    public float GetFinalCooldown(float baseCooldown) => Mathf.Max(0.1f, baseCooldown + _cooldownDelta);

    #endregion
}
