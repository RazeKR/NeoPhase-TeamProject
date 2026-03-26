using System.Collections.Generic;
using UnityEngine;


public enum ESkillType
{
    Passive,
    Active,
}


// health mana damage attackSpeed healthRegen manaRegen moveSpeed expMultiplier
public enum EStatus
{
    // 패시브에 활용
}


[CreateAssetMenu(menuName ="SO/Data/SkillData", fileName = "SkillData_")]
public class CSkillDataSO : ScriptableObject
{
    public string skillName;        // 스킬명
    public ESkillType skillType;    // 스킬 타입
    public Sprite icon;             // 스킬 아이콘
    public int maxLevel;            // 마스터 레벨
    public int requiredPoints;      // 요구 포인트
    public float damage;            // 데미지 값 (패시브 액티브 공통)

    // 선행 스킬
    public List<CSkillDataSO> prerequisiteSkills;

    [Header("패시브")]
    public float statUp;

    [Header("액티브")]
    public string actionName;       // 차후 실행할 함수 이름을 연동
    public float coolDown;
    public float requiredMana;

    [Header("부가효과")]
    public List<CSkillEffect> skillEffects;
}