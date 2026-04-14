/// <summary>
/// 장착된 펫의 버프를 플레이어 스탯에 실시간으로 반영합니다.
/// CPlayerStatManager._petModifiers 슬롯을 사용하므로 스킬/패시브 버프와 충돌하지 않습니다.
///
/// [펫 타입별 적용 대상]
///   (공통)           경험치 획득량  → EPlayerStatType.ExpMultiplier
///   ProjectileBoost  투사체 수      → CWeaponEquip.SetPetProjectileBonus()
///   AttackPowerBoost 플레이어 공격력 → EPlayerStatType.Damage
///   AttackSpeedBoost 플레이어 공격속도 → EPlayerStatType.AttackSpeed
///
/// [부착 위치]
///   Pet_InventoryPanel
/// </summary>
public class CPetBuffApplier : UnityEngine.MonoBehaviour
{
    #region Unity Methods

    private void Start()
    {
        if (CPetInventorySystem.Instance != null)
        {
            CPetInventorySystem.Instance.OnPetEquipped   += ApplyPetBuff;
            CPetInventorySystem.Instance.OnPetUnequipped += RemoveAllPetBuffs;
        }

        if (CGameManager.Instance != null)
            CGameManager.Instance.OnPlayerRegistered += OnPlayerRegistered;

        // 이미 장착된 펫이 있고 플레이어가 등록된 상태면 즉시 적용
        RefreshAllBuffs(CGameManager.Instance?.CachedStatManager);
    }

    private void OnDestroy()
    {
        if (CPetInventorySystem.Instance != null)
        {
            CPetInventorySystem.Instance.OnPetEquipped   -= ApplyPetBuff;
            CPetInventorySystem.Instance.OnPetUnequipped -= RemoveAllPetBuffs;
        }

        if (CGameManager.Instance != null)
            CGameManager.Instance.OnPlayerRegistered -= OnPlayerRegistered;
    }

    #endregion

    #region Private Methods

    private void ApplyPetBuff(CPetInstance pet)
    {
        RefreshAllBuffs(CGameManager.Instance?.CachedStatManager);
    }

    private void RemoveAllPetBuffs()
    {
        var statManager = CGameManager.Instance?.CachedStatManager;
        if (statManager != null)
        {
            statManager.SetPetStatUpgrade(EPlayerStatType.ExpMultiplier, 0f);
            statManager.SetPetStatUpgrade(EPlayerStatType.Damage,        0f);
            statManager.SetPetStatUpgrade(EPlayerStatType.AttackSpeed,   0f);
        }

        CWeaponEquip.Instance?.SetPetProjectileBonus(0);
    }

    private void OnPlayerRegistered(CPlayerStatManager statManager)
    {
        RefreshAllBuffs(statManager);
    }

    /// <summary>현재 장착 펫의 모든 버프를 적용합니다. 장착 펫이 없으면 전부 0으로 초기화합니다.</summary>
    private void RefreshAllBuffs(CPlayerStatManager statManager)
    {
        CPetInstance pet = CPetInventorySystem.Instance?.EquippedPet;

        if (pet == null)
        {
            if (statManager != null)
            {
                statManager.SetPetStatUpgrade(EPlayerStatType.ExpMultiplier, 0f);
                statManager.SetPetStatUpgrade(EPlayerStatType.Damage,        0f);
                statManager.SetPetStatUpgrade(EPlayerStatType.AttackSpeed,   0f);
            }
            CWeaponEquip.Instance?.SetPetProjectileBonus(0);
            return;
        }

        // ── 공통 버프: 경험치 획득량 ──────────────────────────────────────
        statManager?.SetPetStatUpgrade(EPlayerStatType.ExpMultiplier,
            pet.GetXpBoostPercent() / 100f);

        // ── 타입별 버프 (한 펫은 한 타입만) ──────────────────────────────
        if (pet._data == null) return;

        switch (pet._data.PetType)
        {
            case EPetType.ProjectileBoost:
                CWeaponEquip.Instance?.SetPetProjectileBonus(pet.GetTotalProjectileBonus());
                statManager?.SetPetStatUpgrade(EPlayerStatType.Damage,      0f);
                statManager?.SetPetStatUpgrade(EPlayerStatType.AttackSpeed, 0f);
                break;

            case EPetType.AttackPowerBoost:
                statManager?.SetPetStatUpgrade(EPlayerStatType.Damage,
                    pet.GetTotalAttackPowerPercent() / 100f);
                CWeaponEquip.Instance?.SetPetProjectileBonus(0);
                statManager?.SetPetStatUpgrade(EPlayerStatType.AttackSpeed, 0f);
                break;

            case EPetType.AttackSpeedBoost:
                statManager?.SetPetStatUpgrade(EPlayerStatType.AttackSpeed,
                    pet.GetTotalAttackSpeedPercent() / 100f);
                CWeaponEquip.Instance?.SetPetProjectileBonus(0);
                statManager?.SetPetStatUpgrade(EPlayerStatType.Damage, 0f);
                break;
        }
    }

    #endregion
}
