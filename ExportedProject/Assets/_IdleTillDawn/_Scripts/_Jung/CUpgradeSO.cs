using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Data/UpgradeData", fileName = "UpgradeData_")]
public class CUpgradeSO : ScriptableObject
{
    [Header("강화 확률 보정")]
    [Tooltip("낮을 수록 가파르게 감소 (지수 감소 계수)")]
    [Range(0f, 1f)] public float upgradeRate = 0.8f;


    public bool GetUpgradeResult(int currentUpgrade)
    {
        float Chance = Mathf.Pow(upgradeRate, currentUpgrade);
        float randomVal = Random.value; // 0.0 ~ 1.0

        return randomVal < Chance;
    }
}
