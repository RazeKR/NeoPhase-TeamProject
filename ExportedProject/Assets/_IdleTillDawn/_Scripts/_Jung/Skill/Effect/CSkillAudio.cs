using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType
{ 
    Cast,
    Area
}

public class CSkillAudio : MonoBehaviour, ISkill
{
    public SFXType sfxType;

    public void Init(CSkillDataSO data, int level)
    {
        switch (sfxType)
        {
            case SFXType.Cast:
                CAudioManager.Instance?.Play(data.CastSFX, transform.position);
                Debug.Log("Cast¿Œ¿’ ================");
                break;

            case SFXType.Area:
                StartCoroutine(RepeatSFX((int)(data.lifeTime / data.damageInterval), data.damageInterval, data));                
                break;
        }
    }

    private IEnumerator RepeatSFX(int count, float interval, CSkillDataSO data)
    {
        if (count == 0) count = 1;

        for (int i = 0; i < count; i++)
        {
            CAudioManager.Instance?.Play(data.AreaSFX, transform.position);
            Debug.Log("Area¿Œ¿’ ================");
            yield return new WaitForSeconds(interval);
        }
    }
}
