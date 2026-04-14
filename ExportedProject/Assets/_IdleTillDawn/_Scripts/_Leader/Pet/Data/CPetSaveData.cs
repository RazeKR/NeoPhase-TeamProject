/// <summary>
/// 펫 하나의 세이브 데이터입니다.
/// CPetInventorySaveData 리스트의 원소로 사용됩니다.
/// </summary>
[System.Serializable]
public class CPetSaveData
{
    public int    itemID;      // CPetDataSO 식별자 (CDataManager 키)
    public string instanceID;  // 펫 인스턴스 고유 ID (GUID)
    public int    rank;        // 등급 (0=Common / 1=Rare / 2=Epic / 3=Legendary)
    public int    upgrade;     // 강화 단계 (0 ~ 10)
    public bool   isEquipped;  // 현재 장착 여부
}
