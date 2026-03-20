using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 아이템 정보 세이브 데이터
// InventorySaveData 리스트의 요소로 사용됨

[System.Serializable]
public class CItemSaveData
{
    public string itemID;     // SO 식별자
    public string instanceID; // 개별 식별자 (무기)
    public int rank;          // 등급 정보 (무기)
    public bool isEquipped;   // 장착 여부 (무기)
    public int amount;        // 수량 (포션)
    public EItemType type;    // 종류 구분
}
