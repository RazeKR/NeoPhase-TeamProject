public interface IHealable
{
    /// <summary>
    /// 체력을 회복할 때 호출하는 메서드
    /// </summary>
    /// <param name="amount"></param>
    void Heal(float amount);
}
