using System.Collections.Generic;

[System.Serializable]
public class CSkillSaveData
{
    public List<CSkillInstance> skillList = new List<CSkillInstance>();
    public int remainingPoints;
}
