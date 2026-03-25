using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
ㆍCWeaponUpgrade
- 무기 업그레이드 기능 담당
- 업그레이드 목표가 되는 무기 정보를 받아와 무기를 업그레이드 함
- 인스펙터 내에서 확률 조정 가능
*/


public class CWeaponUpgrade : MonoBehaviour
{    
    [SerializeField] private float _upgradeRate = 0.8f; // 감소 계수, 낮을수록 가파르게 확률 감소
    [SerializeField] private bool _debugLog = true;

    // 무기 강화 함수
    public void TryUpgrade(CWeaponInstance target)
    {
        int upgrade = target._upgrade;

        // 0 이하 방어 코드
        if (upgrade < 0) upgrade = 0;


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
