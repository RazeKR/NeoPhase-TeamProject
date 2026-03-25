using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="SO/Data/SkillData", fileName = "SkillData_")]
public class CSkillDataSO : ScriptableObject
{
    public string skillName;
    public Sprite icon;         // 스킬 아이콘
    public int maxLevel;        // 마스터 레벨
    public int requiredPoints;  // 요구 포인트

    // 선행 스킬
    public List<CSkillDataSO> prerequisiteSkills;
}