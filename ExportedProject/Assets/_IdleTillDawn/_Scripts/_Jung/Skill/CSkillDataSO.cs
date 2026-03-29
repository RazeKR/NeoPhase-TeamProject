using System.Collections.Generic;
using UnityEngine;

/// <summary>스킬의 종류를 구분하는 열거형입니다.</summary>
public enum ESkillType
{
    Passive, // 패시브 - 장착 시 자동 적용
    Active,  // 액티브 - 수동 발동
}

// 추후 패시브 스탯 연동 시 사용합니다
public enum EStatus
{
    // 추후 활성화 예정
}

/// <summary>
/// 스킬의 기본 정보, 비용, 선행 조건, 레벨별 데이터 참조를 정의하는 ScriptableObject입니다.
/// CDataManager.GetSkill(id)로 int ID 기반 접근을 지원합니다.
/// 기존 CSkillManager와의 호환을 위해 public 필드(skillName 등)를 유지합니다.
/// </summary>
[CreateAssetMenu(menuName = "IdleTillDawn/Data/SkillData", fileName = "SkillData_")]
public class CSkillDataSO : CBaseDataSO
{
    #region InspectorVariables

    [Header("기본 정보")]
    public string skillName;      // 스킬명 (CSkillManager의 string 키 호환 유지)
    public string flavourText;    // 스킬 설명
    public ESkillType skillType;  // 스킬 종류
    public Sprite icon;           // 스킬 아이콘
    public int maxLevel;          // 최대 레벨
    public int requiredPoints;    // 해금 필요 스킬 포인트
    public float damage;          // 기본 데미지 (패시브 스킬에도 활용)

    [Header("선행 조건")]
    public List<CSkillDataSO> prerequisiteSkills; // 선행 필요 스킬 목록

    [Header("패시브")]
    public float statUp;          // 패시브 스탯 보너스 수치

    [Header("액티브")]
    public string actionName;     // 발동할 함수 이름 (리플렉션 또는 switch 키)
    public float coolDown;        // 쿨타임 (초)
    public float requiredMana;    // 마나 소모량

    [Header("이펙트")]
    public List<CSkillEffect> skillEffects;           // 스킬 이펙트 목록
    public GameObject effectPrefab;                    // 스킬 이펙트 프리팹

    [Header("레벨별 데이터 (index = level - 1)")]
    [SerializeField] private List<CSkillLevelDataSO> _levelData = new(); // 레벨별 수치 SO 목록

    #endregion

    #region Properties

    /// <summary>등록된 레벨 데이터 목록의 읽기 전용 접근자입니다.</summary>
    public IReadOnlyList<CSkillLevelDataSO> LevelData => _levelData;

    #endregion

    #region PublicMethods

    /// <summary>
    /// 특정 레벨에 해당하는 CSkillLevelDataSO를 반환합니다.
    /// 레벨은 1부터 시작하며, 범위를 벗어나면 가장 가까운 항목을 반환합니다.
    /// _levelData가 비어 있으면 null을 반환합니다.
    /// </summary>
    public CSkillLevelDataSO GetLevelData(int level)
    {
        if (_levelData == null || _levelData.Count == 0) return null;

        int index = Mathf.Clamp(level - 1, 0, _levelData.Count - 1);
        return _levelData[index];
    }

    /// <summary>특정 레벨에서의 실제 데미지를 반환합니다. 레벨 데이터가 없으면 기본 damage를 반환합니다.</summary>
    public float GetDamageAtLevel(int level)
    {
        CSkillLevelDataSO levelData = GetLevelData(level);
        if (levelData == null) return damage;
        return levelData.GetFinalDamage(damage);
    }

    /// <summary>특정 레벨에서의 쿨타임을 반환합니다. 레벨 데이터가 없으면 기본 coolDown을 반환합니다.</summary>
    public float GetCooldownAtLevel(int level)
    {
        CSkillLevelDataSO levelData = GetLevelData(level);
        if (levelData == null) return coolDown;
        return levelData.GetFinalCooldown(coolDown);
    }

    /// <summary>특정 레벨에서의 마나 소모량을 반환합니다. 레벨 데이터가 없으면 기본 requiredMana를 반환합니다.</summary>
    public float GetManaCostAtLevel(int level)
    {
        CSkillLevelDataSO levelData = GetLevelData(level);
        if (levelData == null) return requiredMana;
        return levelData.GetFinalManaCost(requiredMana);
    }

    #endregion
}
