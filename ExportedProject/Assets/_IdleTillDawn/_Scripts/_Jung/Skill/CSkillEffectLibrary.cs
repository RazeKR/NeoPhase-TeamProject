using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 스킬에 붙는 부가효과를 타겟에세 처리해줌
/// 스킬을 실행할 때, CSkillEffectLibrary.ApplyEffect(대상 적, 효과) 를 불러오면 됨.
/// 
/// * 상태이상 구현되면 연동
/// </summary>
public static class CSkillEffectLibrary
{
    public static void ApplyEffect(GameObject target, CSkillEffect effect)
    {
        if (Random.value > effect.chance) return; // 발동 확률 체크

        switch (effect.type)
        {
            case CSkillEffect.EEffectType.Knockback:

                break;

            case CSkillEffect.EEffectType.Freeze:

                break;

            case CSkillEffect.EEffectType.Burn:

                break;
        }
    }
}
