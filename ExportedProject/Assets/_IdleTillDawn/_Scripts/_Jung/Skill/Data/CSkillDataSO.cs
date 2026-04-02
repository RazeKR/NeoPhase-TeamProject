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

/// <summary>스킬의 부가효과 열거형입니다.</summary>
public enum EEffectType
{
    Knockback,
    Freeze,
    Burn,
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
        
    #endregion
}

[Serializable]
public class ActiveLevelData
{
    public int damage;
    public int coolDown;
    public int requiredMana;
    public int projectileAmount = 1;
}

[Serializable]
public class PassiveLevelData
{
    public int statUp;
}


[Serializable]
public class CSkillEffect
{
    public EEffectType type;        // 효과 종류
    public float value;             // 세부 적용 수치
    public float duration;          // 지속시간
}