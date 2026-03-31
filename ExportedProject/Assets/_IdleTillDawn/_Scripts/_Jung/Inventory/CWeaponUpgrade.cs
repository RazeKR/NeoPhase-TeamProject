using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 무기 업그레이드 기능을 담당합니다.
/// 업그레이드 목표가 되는 무기 정보를 받아와 무기를 업그레이드 합니다.
/// 인스펙터 내에서 확률을 조정할 수 있습니다.
/// </summary>

public class CWeaponUpgrade : MonoBehaviour
{    
    public static CWeaponUpgrade Instance { get; private set; }

    [SerializeField] private float _upgradeRate = 0.8f; // 감소 계수, 낮을수록 가파르게 확률 감소
    [SerializeField] private bool _debugLog = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    /// <summary>
    /// 무기 강화를 시도합니다. 설정한 확률에 따라 결과를 제공합니다.
    /// </summary>
    public void TryUpgrade(CWeaponInstance target)
    {
        int upgrade = target._upgrade;

        // 0 이하 방어 코드
        if (upgrade < 0) upgrade = 0;

        if (upgrade >= 10)
        {
            upgrade = 10;
            Debug.Log("이미 최대 업그레이드 입니다.");
        }

        // 지수 감소
        float rate = Mathf.Pow(_upgradeRate, upgrade);

        if (_debugLog) Debug.Log($"강화 대상 : {target._itemData.ItemName}  |  현재 강화 확률 : {rate}");

        float result = Random.Range(0.00f, 1.00f);

        if (result < rate)
        {
            target._upgrade += 1;
            if (_debugLog) Debug.Log($"강화 성공! : {target._itemData.ItemName}  |  강화 등급 : {target._upgrade}");
        }
        else
        {
            if (_debugLog) Debug.Log($"강화 실패.. : {target._itemData.ItemName}  |  강화 등급 : {target._upgrade}");
        }
        
    }
}
