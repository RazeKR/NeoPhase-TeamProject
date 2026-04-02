
// 스킬에 붙을 부가효과

[System.Serializable]
public class CSkillEffect
{
    public enum EEffectType
    {
        Knockback,
        Freeze,
        Burn,
    }

    public EEffectType type;        // 효과 종류
    public float value;             // 세부 적용 수치
}
