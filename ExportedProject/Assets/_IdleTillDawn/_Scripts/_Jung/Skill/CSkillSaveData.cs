using System.Collections.Generic;

// 스킬 저장 정보
[System.Serializable]
public class CSkillSaveData
{
    public List<CSkillInstance> skillList = new List<CSkillInstance>();
    public int remainingPoints;

    public string[] equippedSkillName = new string[3];
}
