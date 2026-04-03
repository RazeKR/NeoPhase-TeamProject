using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBuffSkill : MonoBehaviour, ISkill
{
    private CSkillDataSO _data;
    private int _level;

    public void Init(CSkillDataSO data, int level)
    {
        _data = data;
        _level = level;
    }

    private void ApplyBuff()
    {
        
    }
}
