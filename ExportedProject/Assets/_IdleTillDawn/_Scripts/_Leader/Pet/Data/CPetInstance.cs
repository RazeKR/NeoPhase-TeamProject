using System;


/// <summary>
/// 런타임에서 펫 하나의 상태를 나타내는 인스턴스 클래스입니다.
/// CItemInstance를 상속하여 중앙 인벤토리 허브와 동일한 ID 체계를 공유합니다.
///
/// [등급 상수]
///   Rank 0 = Common / 1 = Rare / 2 = Epic / 3 = Legendary
///
/// [강화 상수]
///   최대 강화 단계: MaxUpgrade (10)
/// </summary>
[System.Serializable]
public class CPetInstance : CItemInstance
{
    public const int MaxUpgrade = 10;

    public int  _rank;
    public int  _upgrade;
    public bool _isEquipped;

    public CPetDataSO _data => _itemData as CPetDataSO;

    // ─── 생성 (신규) ──────────────────────────────────────────────────────

    public CPetInstance(CPetDataSO data, int rank = 0) : base(data)
    {
        _rank       = rank;
        _upgrade    = 0;
        _isEquipped = false;
    }

    // ─── 생성 (세이브 데이터 복구) ────────────────────────────────────────

    public CPetInstance(CPetDataSO data, string savedInstanceID) : base(data)
    {
        _instanceID = savedInstanceID;
    }

    // ─── 계산 헬퍼 ───────────────────────────────────────────────────────

    /// <summary>현재 등급 + 강화를 반영한 펫 자체 공격력.</summary>
    public float GetPetAttackPower() => _data.GetPetAttackPower(_upgrade);

    /// <summary>현재 등급 + 강화 합산 경험치 증가 퍼센트.</summary>
    public float GetXpBoostPercent() => _data.GetXpBoostPercent(_rank, _upgrade);

    /// <summary>현재 등급 + 강화 합산 투사체 증가 수량 (ProjectileBoost 타입).</summary>
    public int GetTotalProjectileBonus() => _data.GetTotalProjectileBonus(_rank, _upgrade);

    /// <summary>현재 등급 + 강화 합산 공격력 증가 퍼센트 (AttackPowerBoost 타입).</summary>
    public float GetTotalAttackPowerPercent() => _data.GetTotalAttackPowerPercent(_rank, _upgrade);

    /// <summary>현재 등급 + 강화 합산 공격속도 증가 퍼센트 (AttackSpeedBoost 타입).</summary>
    public float GetTotalAttackSpeedPercent() => _data.GetTotalAttackSpeedPercent(_rank, _upgrade);

    /// <summary>강화가 최대 단계인지 여부.</summary>
    public bool IsMaxUpgrade => _upgrade >= MaxUpgrade;
}
