using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
ㆍCGenerateItem
- 무작위 정보를 저장한 아이템을 생성 후 인벤토리에 추가해주는 컴포넌트
- 인스펙터를 통해 생성 확률 바운더리를 설정 가능
*/

public class CGenerateItem : MonoBehaviour
{
    [SerializeField] private int _commonRate = 85;
    [SerializeField] private int _rareRate = 10;
    [SerializeField] private int _epicRate = 4;
    [SerializeField] private int _legendaryRate = 1;

    [SerializeField] private CWeaponDataBaseSO _weaponDataBaseSO = null;

    public void GenerateRandomRankItem()
    {
        int  r = Random.Range(0, _commonRate + _rareRate + _epicRate + _legendaryRate + 1);

        int desiredRank = 0;

        string desiredWeaponID = "weapon_01";

        if (r > _commonRate + _rareRate + _epicRate) desiredRank = 3;

        else if (r > _commonRate + _rareRate) desiredRank = 2;
        
        else if (r > _commonRate) desiredRank = 1;

        else desiredRank = 0;

        int rIndex = Random.Range(0, _weaponDataBaseSO.WeaponDataBaseSOCount());

        CWeaponDataSO so = _weaponDataBaseSO.GetWeaponDataByIndex(rIndex);

        desiredWeaponID = so.ItemId;

        CInventoryManager.Instance.AddItem(desiredWeaponID, 1, desiredRank);
    }
}
