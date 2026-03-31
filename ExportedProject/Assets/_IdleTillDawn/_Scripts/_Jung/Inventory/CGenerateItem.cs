using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    #endregion

    #region Properties

    public int CommonRate => _commonRate;
    public int RareRate => _rareRate;
    public int EpicRate => _epicRate;
    public int LegendaryRate => _legendaryRate;

    #endregion

    #region PublicMethods

    /// <summary>
    /// 무작위 종류와 희귀도를 가진 무기를 생성하여 인벤토리에 추가해줍니다.
    /// </summary>
    public void GenerateRandomRankItem()
    {
        int  r = Random.Range(0, _commonRate + _rareRate + _epicRate + _legendaryRate + 1);

        int desiredRank = 0;

        int desiredWeaponID = 5;

        if (r > _commonRate + _rareRate + _epicRate) desiredRank = 3;

        else if (r > _commonRate + _rareRate) desiredRank = 2;
        
        else if (r > _commonRate) desiredRank = 1;

        else desiredRank = 0;

        int rIndex = Random.Range(0, _weaponDataBaseSO.WeaponDataBaseSOCount());

        CWeaponDataSO so = _weaponDataBaseSO.GetWeaponDataByIndex(rIndex);

        desiredWeaponID = so.Id;

        CInventorySystemJ.Instance.AddItem(desiredWeaponID, 1, desiredRank);
    }

    #endregion
}
