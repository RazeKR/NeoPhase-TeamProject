using UnityEngine;

/// <summary>
/// 펫 투사체에 동적으로 추가되어 피격 시 화상 상태이상을 적용하는 컴포넌트입니다.
///
/// CPetOrbitController.FireProjectileAt()에서 ApplyBurnOnHit = true인 경우
/// 인스턴스화된 투사체 GameObject에 AddComponent로 부착됩니다.
///
/// Projectile.OnTriggerEnter2D가 데미지를 처리하고,
/// 이 컴포넌트의 OnTriggerEnter2D는 독립적으로 화상을 적용합니다.
/// </summary>
public class CPetBurnApplier : MonoBehaviour
{
    private float _duration;
    private float _tickDamage;
    private float _tickInterval;
    private GameObject _owner;

    /// <summary>CPetOrbitController에서 투사체 생성 직후 호출합니다.</summary>
    public void Init(float duration, float tickDamage, float tickInterval, GameObject owner)
    {
        _duration     = duration;
        _tickDamage   = tickDamage;
        _tickInterval = tickInterval;
        _owner        = owner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_owner != null && other.gameObject == _owner) return;

        CEntityBase entity = other.GetComponentInParent<CEntityBase>();
        if (entity != null)
            entity.ApplyBurn(_duration, _tickDamage, _tickInterval);
    }
}
