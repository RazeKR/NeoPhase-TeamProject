using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CItemInstance
{
    public string _instanceID;
    public CItemDataSO _itemData;
    
    public CItemInstance(CItemDataSO data)
    {
        _itemData = data;
        _instanceID = Guid.NewGuid().ToString();
    }
}
