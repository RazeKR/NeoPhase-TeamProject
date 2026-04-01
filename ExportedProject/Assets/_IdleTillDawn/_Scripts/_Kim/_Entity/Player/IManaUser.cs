public interface IManaUser
{
    /// <summary>
    /// 마나를 회복할때 호출하는 메서드
    /// </summary>
    /// <param name="amount"></param>
    void RestoreMana(float amount);
    /// <summary>
    /// 마나를 사용할 때 호출하는 메서드
    /// </summary>
    /// <param name="amount"></param>
    bool ConsumeMana(float amount);
}
