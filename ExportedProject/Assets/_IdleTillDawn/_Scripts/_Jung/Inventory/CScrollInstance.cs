
// 스크롤 고유 정보를 저장하는 인스턴스 클래스

[System.Serializable]
public class CScrollInstance : CItemInstance
{
    public int _amount;
    public int _maxAmount = 999;
    public CScrollDataSO _data => _itemData as CScrollDataSO;

    // 새로운 스크롤 생성 호출하는 생성자 함수
    public CScrollInstance(CScrollDataSO data, int amount) : base(data)
    {
        this._amount = amount;
    }
}
