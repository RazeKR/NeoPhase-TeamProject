using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 무작위 종류와 희귀도를 가진 무기를 생성하여 인벤토리에 추가해줍니다.
/// 인스펙터를 통해 생성 확률을 설정할 수 있습니다.
/// </summary>


public class CGenerateItem : MonoBehaviour
{
    #region Inspectors

    [SerializeField] private int _commonRate = 85;
    [SerializeField] private int _rareRate = 10;
    [SerializeField] private int _epicRate = 4;
    [SerializeField] private int _legendaryRate = 1;

    [SerializeField] private CWeaponDataBaseSO _weaponDataBaseSO = null;

    [Header("무기 상자 보유 수량 HUD Text")]
    [SerializeField] private Text _weaponBoxCountText = null;

    #endregion

    #region Properties

    public int CommonRate => _commonRate;
    public int RareRate => _rareRate;
    public int EpicRate => _epicRate;
    public int LegendaryRate => _legendaryRate;

    #endregion

    #region Unity Methods

    private void Start()
    {
        RefreshWeaponBoxCountText();
        CGoldShopUI.OnWeaponBoxCountChanged += RefreshWeaponBoxCountText;
    }

    private void OnDestroy()
    {
        CGoldShopUI.OnWeaponBoxCountChanged -= RefreshWeaponBoxCountText;
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// 무기 상자를 1개 소모하여 무작위 종류와 희귀도를 가진 무기를 생성하고 인벤토리에 추가합니다.
    /// 무기 상자가 없으면 실행되지 않습니다.
    /// </summary>
    public void GenerateRandomRankItem()
    {
        if (CJsonManager.Instance == null)
        {
            Debug.LogError("[CGenerateItem] CJsonManager.Instance가 null입니다.");
            return;
        }

        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();

        if (data.weaponBoxCount <= 0)
        {
            Debug.Log("[CGenerateItem] 무기 상자가 없습니다. 상점에서 구매해주세요.");
            return;
        }

        if (CInventorySystemJ.Instance.IsFull)
        {
            Debug.Log("[CGenerateItem] 인벤토리가 가득 차 무기 상자를 열 수 없습니다.");
            CInventorySystemJ.Instance.NotifyInventoryFull();
            return;
        }

        data.weaponBoxCount--;
        CJsonManager.Instance.Save(data);
        RefreshWeaponBoxCountText(data.weaponBoxCount);

        int r = Random.Range(0, _commonRate + _rareRate + _epicRate + _legendaryRate + 1);

        int desiredRank;

        if      (r > _commonRate + _rareRate + _epicRate) desiredRank = 3;
        else if (r > _commonRate + _rareRate)             desiredRank = 2;
        else if (r > _commonRate)                         desiredRank = 1;
        else                                              desiredRank = 0;

        int rIndex = Random.Range(0, _weaponDataBaseSO.WeaponDataBaseSOCount());
        CWeaponDataSO so = _weaponDataBaseSO.GetWeaponDataByIndex(rIndex);

        CInventorySystemJ.Instance.AddItem(so.Id, 1, desiredRank);

        Debug.Log($"[CGenerateItem] 무기 소환 완료 (ID:{so.Id}, Rank:{desiredRank}) | 남은 상자: {data.weaponBoxCount}개");
    }

    #endregion

    #region Private Methods

    private void RefreshWeaponBoxCountText(int count)
    {
        if (_weaponBoxCountText != null)
            _weaponBoxCountText.text = $"x{count}";
    }

    private void RefreshWeaponBoxCountText()
    {
        if (CJsonManager.Instance == null) return;
        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();
        RefreshWeaponBoxCountText(data.weaponBoxCount);
    }

    #endregion
}
