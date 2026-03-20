
// 포션 고유 정보를 저장하는 인스턴스 클래스

[System.Serializable]
public class CPotionInstance : CItemInstance
{
    public int _amount;
    public CPotionDataSO _data => _itemData as CPotionDataSO;

    // 새로운 포션 생성 호출하는 생성자 함수
    public CPotionInstance(CPotionDataSO data, int amount) : base(data)
    {
        this._amount = amount;
    }
}
