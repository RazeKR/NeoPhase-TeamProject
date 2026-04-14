using System.Collections.Generic;


/// <summary>
/// 보유한 모든 펫의 세이브 데이터 컨테이너입니다.
/// CSaveData 안에 단일 필드로 포함됩니다.
/// </summary>
[System.Serializable]
public class CPetInventorySaveData
{
    public List<CPetSaveData> pets = new List<CPetSaveData>();
}
