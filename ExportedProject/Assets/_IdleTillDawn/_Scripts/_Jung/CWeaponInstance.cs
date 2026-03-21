using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 무기 고유 정보를 저장하는 인스턴스 클래스

[System.Serializable]
public class CWeaponInstance : CItemInstance
{    
    public int _rank;
    public bool _isEquipped;
    public CWeaponDataSO _data => _itemData as CWeaponDataSO;

    // 새로운 무기 생성 호출하는 생성자 함수
    public CWeaponInstance(CWeaponDataSO data) : base(data)
    {
        this._rank = 0;
        this._isEquipped = false;
    }


    // 세이브된 데이터로부터 복구할 때 호출하는 오버로드 함수 (보류)
    public CWeaponInstance(CWeaponDataSO data, string savedID) : base(data)
    {
        this._instanceID = savedID;
    }
}
