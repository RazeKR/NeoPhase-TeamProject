using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 데미지를 입을 때 호출 — 피격 방향 정보 없이 순수 데미지만 적용
    /// </summary>
    /// <param name="damage">입은 데미지의 양</param>
    void TakeDamage(float damage);

    /// <summary>
    /// 데미지를 입을 때 호출 — 피격 방향 정보를 함께 전달하여
    /// HitFlash 및 데미지 텍스트 방향 연출에 활용된다
    /// </summary>
    /// <param name="damage">입은 데미지의 양</param>
    /// <param name="hitDir">피격 방향 벡터 (공격자→피격자 방향, 정규화 불필요)</param>
    void TakeDamage(float damage, Vector2 hitDir);

    /// <summary>
    /// 체력이 0이 되면 호출
    /// </summary>
    void Die();

    //void RecoverHP(float amount);
    //void RecoverMP(float amount);
    //void RegenHP(float amount);
    //void RegenMP(float amount);
}
