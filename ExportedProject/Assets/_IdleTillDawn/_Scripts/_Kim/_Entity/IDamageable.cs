public interface IDamageable
{
    /// <summary>
    /// 데미지를 입을 때 호출
    /// </summary>
    /// <param name="damage">입은 데미지의 양</param>
    void TakeDamage(float damage);

    /// <summary>
    /// 체력이 0이 되면 호출
    /// </summary>
    void Die();
}
