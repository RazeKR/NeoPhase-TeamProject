using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStatType
{
    None,
    Strength,
    Hp,
    Mp,
    Speed,
}

[CreateAssetMenu(menuName = "SO/Data/PotionData", fileName = "PotionData_")]
public class CPotionDataSO : CItemDataSO
{
    [SerializeField] private int _healAmount = 0;
    [SerializeField] private int _manaHealAmount = 0;
    [SerializeField] private int _statUpAmount = 0;
    [SerializeField] private EStatType _statType = EStatType.None;

    public int HealAmount => _healAmount;           // HP 회복량
    public int ManaHealAmount => _manaHealAmount;   // MP 회복량
    public int StatusUpAmount => _statUpAmount;     // 능력치 상승량
    public EStatType StatType => _statType;         // 대상 능력치 타입
}
