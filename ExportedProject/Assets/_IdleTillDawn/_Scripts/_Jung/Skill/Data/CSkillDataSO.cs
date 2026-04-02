using System;
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
/// 기존 CSkillSystem와의 호환을 위해 public 필드(skillName 등)를 유지합니다.
/// </summary>
[CreateAssetMenu(menuName = "IdleTillDawn/Data/SkillData", fileName = "SkillData_")]
public class CSkillDataSO : CBaseDataSO
{
    #region InspectorVariables

    [Header("기본 정보")]
    public string skillName;      // 스킬명 (CSkillSystem의 string 키 호환 유지)
    public string flavourText;    // 스킬 설명
    public ESkillType skillType;  // 스킬 종류
    public Sprite icon;           // 스킬 아이콘
    public int maxLevel;          // 최대 레벨
    public int requiredPoints;    // 해금 필요 스킬 포인트

    [Header("선행 조건")]
    public List<CSkillDataSO> prerequisiteSkills; // 선행 필요 스킬 목록

    [Header("패시브")]
    public List<PassiveLevelData> PassiveLevelDatas;

    [Header("액티브")]
    public List<ActiveLevelData> ActiveLevelDatas;

    [Header("투사체 설정")]
    public int projectileAmount;  // 투사체 수량
    public float spreadAngle;     // 투사체 탄퍼짐
    public float speed;           // 투사체 속도

    [Header("범위기술 설정")]
    public float damageInterval;        // 피해 간격
    public bool useScaleMagnification;  // 레벨에 따른 범위 증가 여부
    public float scalePreset;           // 스케일 배율 프리셋

    [Header("잔류 시간 설정(투사체, 범위기술 공용)")]    
    public float lifeTime;

    [Header("부가효과(넉백, 화상, 빙결)")]
    public bool useSkillEffect = false; // 스킬 부가효과 여부
    public CSkillEffect skillEffect;    // 스킬 부가효과 상세

    [Header("실사용 오브젝트 프리팹")]
    public GameObject effectPrefab;

    

    //[Header("레벨별 데이터 (index = level - 1)")]    
    //[SerializeField] private List<CSkillLevelDataSO> _levelData = new(); // 레벨별 수치 SO 목록

    #endregion

    #region Properties

    /// <summary>등록된 레벨 데이터 목록의 읽기 전용 접근자입니다.</summary>
    //public IReadOnlyList<CSkillLevelDataSO> LevelData => _levelData;

    #endregion

    #region PublicMethods

    /// <summary>
    /// 특정 레벨에 해당하는 CSkillLevelDataSO를 반환합니다.
    /// 레벨은 1부터 시작하며, 범위를 벗어나면 가장 가까운 항목을 반환합니다.
    /// _levelData가 비어 있으면 null을 반환합니다.
    /// </summary>
    //public CSkillLevelDataSO GetLevelData(int level)
    //{
    //    if (_levelData == null || _levelData.Count == 0) return null;
    //
    //    int index = Mathf.Clamp(level - 1, 0, _levelData.Count - 1);
    //    return _levelData[index];
    //}

    ///// <summary>특정 레벨에서의 실제 데미지를 반환합니다. 레벨 데이터가 없으면 기본 damage를 반환합니다.</summary>
    //public float GetDamageAtLevel(int level)
    //{
    //    CSkillLevelDataSO levelData = GetLevelData(level);
    //    if (levelData == null) return damage;
    //    return levelData.GetFinalDamage(damage);
    //}
    //
    ///// <summary>특정 레벨에서의 쿨타임을 반환합니다. 레벨 데이터가 없으면 기본 coolDown을 반환합니다.</summary>
    //public float GetCooldownAtLevel(int level)
    //{
    //    CSkillLevelDataSO levelData = GetLevelData(level);
    //    if (levelData == null) return coolDown;
    //    return levelData.GetFinalCooldown(coolDown);
    //}
    //
    ///// <summary>특정 레벨에서의 마나 소모량을 반환합니다. 레벨 데이터가 없으면 기본 requiredMana를 반환합니다.</summary>
    //public float GetManaCostAtLevel(int level)
    //{
    //    CSkillLevelDataSO levelData = GetLevelData(level);
    //    if (levelData == null) return requiredMana;
    //    return levelData.GetFinalManaCost(requiredMana);
    //}

    #endregion
}

[Serializable]
public class ActiveLevelData
{
    public int damage;
    public int coolDown;
    public int requiredMana;
}

[Serializable]
public class PassiveLevelData
{
    public int statUp;
}