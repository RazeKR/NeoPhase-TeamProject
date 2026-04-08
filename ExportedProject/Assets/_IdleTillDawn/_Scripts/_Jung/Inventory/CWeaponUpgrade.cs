using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


/// <summary>
/// 무기 업그레이드 기능을 담당합니다.
/// 업그레이드 확률 배열을 설정해서 확률을 관리합니다.
/// </summary>

public class CWeaponUpgrade : MonoBehaviour
{    
    public static CWeaponUpgrade Instance { get; private set; }

    [SerializeField] private float[] _customUpgradeRates;
    [SerializeField] private bool _debugLog = true;
    [SerializeField] private bool _breakable = true;
    [SerializeField] private int _breakableLevel = 10;
    [SerializeField] private float _breakableRate = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    /// <summary>
    /// 무기 강화를 시도합니다.
    /// </summary>
    public bool TryUpgrade(CWeaponInstance target)
    {
        // 0 이하 방어 코드
        if (target._upgrade < 0) target._upgrade = 0;

        if (target._upgrade >= 20)
        {
            target._upgrade = 20;
            Debug.Log("이미 최대 업그레이드 입니다.");
            return false;
        }

        if (_customUpgradeRates.Length <= target._upgrade) return false;

        float rate = _customUpgradeRates[target._upgrade];
        if (Random.value < rate)
        {
            target._upgrade ++;
            if (_debugLog) Debug.Log($"강화 성공! : {target._itemData.ItemName}  |  강화 등급 : {target._upgrade}");
            return false;
        }
        else
        {
            if (_debugLog) Debug.Log($"강화 실패.. : {target._itemData.ItemName}");

            if (!_breakable) return false;

            if (_breakableLevel <= target._upgrade &&  Random.value < _breakableRate)
            {
                target._upgrade = 0;
                return true;
            }
        }

        return false;
    }
}
