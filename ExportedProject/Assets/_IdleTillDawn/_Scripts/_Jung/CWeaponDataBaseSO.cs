using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 무기 SO들을 한 곳에 묶어놓는 데이터 베이스


[CreateAssetMenu(menuName = "SO/Data/WeaponDataBase", fileName = "WeaponDataBase")]
public class CWeaponDataBaseSO : ScriptableObject
{
    [SerializeField] private List<CWeaponDataSO> _weaponDataBase = new List<CWeaponDataSO>();

    public CWeaponDataSO GetWeaponDataByName(string name)
    {
        return _weaponDataBase.Find(w => w.ItemName == name);
    }

    public CWeaponDataSO GetWeaponDataById(string id)
    {
        return _weaponDataBase.Find(w => w.ItemId == id);
    }

    public CWeaponDataSO GetWeaponDataByIndex(int index)
    {
        return _weaponDataBase[index];
    }

    public int WeaponDataBaseSOCount()
    {
        return _weaponDataBase.Count;
    }
}
