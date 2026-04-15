using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


/// <summary>
/// 무기 업그레이드 기능을 담당합니다.
/// 업그레이드 확률 배열을 참조해서 확률을 계산합니다.
/// </summary>

public class CWeaponUpgrade : MonoBehaviour
{
    public static CWeaponUpgrade Instance { get; private set; }

    [SerializeField] private float[] _customUpgradeRates;
    [SerializeField] private bool _debugLog = true;
    [SerializeField] private bool _breakable = true;
    [SerializeField] private int _breakableLevel = 10;
    [SerializeField] private float _breakableRate = 0.1f;

    [Header("강화 사운드")]
    [SerializeField] private AudioClip _upgradeSuccessClip = null;  // 강화 성공 사운드
    [SerializeField] private AudioClip _upgradeFailClip    = null;  // 강화 실패 사운드

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
        // 0 미만 예외 자동
        if (target._upgrade < 0) target._upgrade = 0;

        if (target._upgrade >= 20)
        {
            target._upgrade = 20;
            CDebug.Log("이미 최대 업그레이드 입니다.");
            return false;
        }

        if (_customUpgradeRates.Length <= target._upgrade) return false;

        float rate = _customUpgradeRates[target._upgrade];
        if (Random.value < rate)
        {
            target._upgrade ++;
            if (_debugLog) CDebug.Log($"강화 성공! : {target._itemData.ItemName}  |  강화 단계 : {target._upgrade}");
            CAudioManager.Instance?.PlaySFX(_upgradeSuccessClip);
            CUpgradePopUp.Instance.Show(true, target._itemData.ItemSprite, target._rank, $"강화단계가 {target._upgrade - 1} > {target._upgrade}로 올라갔습니다!");
            return false;
        }
        else
        {
            if (_debugLog) CDebug.Log($"강화 실패.. : {target._itemData.ItemName}");
            CAudioManager.Instance?.PlaySFX(_upgradeFailClip);

            if (_breakable && _breakableLevel <= target._upgrade &&  Random.value < _breakableRate)
            {
                CUpgradePopUp.Instance.Show(false, target._itemData.ItemSprite, target._rank, $"무기가 파괴되어 강화 단계가 0이 되었습니다.");
                target._upgrade = 0;
                return true;
            }
            else
            {
                CUpgradePopUp.Instance.Show(false, target._itemData.ItemSprite, target._rank, $"강화에 실패하여 강화 단계가 유지됩니다.");
            }
        }

        CInventoryUI.Instance.RefreshUI();

        return false;
    }
}
