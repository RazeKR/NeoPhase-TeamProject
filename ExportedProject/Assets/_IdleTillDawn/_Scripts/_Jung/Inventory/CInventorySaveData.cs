using System.Collections.Generic;

// 인벤토리 리스트 세이브 데이터
// Json 저장에 활용하는 요소

[System.Serializable]
public class CInventorySaveData
{
    public List<CItemSaveData> items = new List<CItemSaveData>();
}
