using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 펫 상자를 1개 소모하여 무작위 등급·종류의 펫을 생성하고 펫 인벤토리에 추가합니다.
/// CGenerateItem(무기 가챠)과 동일한 가중치 방식을 사용합니다.
///
/// [인스펙터 설정]
///   _commonRate / _rareRate / _epicRate / _legendaryRate — 뽑기 가중치 (합계가 확률 범위)
///   _petDataBaseSO — 뽑기 풀에 포함될 펫 목록
///   _petBoxCountText — HUD에 표시할 펫 상자 보유 수량 Text
/// </summary>
public class CGeneratePet : MonoBehaviour
{
    #region Inspector

    [SerializeField] private int _commonRate    = 85;
    [SerializeField] private int _rareRate      = 10;
    [SerializeField] private int _epicRate      = 4;
    [SerializeField] private int _legendaryRate = 1;

    [SerializeField] private CPetDataBaseSO _petDataBaseSO = null;

    [Header("펫 상자 보유 수량 HUD Text")]
    [SerializeField] private Text _petBoxCountText = null;

    #endregion

    #region Properties

    public int CommonRate    => _commonRate;
    public int RareRate      => _rareRate;
    public int EpicRate      => _epicRate;
    public int LegendaryRate => _legendaryRate;

    #endregion

    #region Unity Methods

    private void Start()
    {
        RefreshPetBoxCountText();
        CGoldShopUI.OnPetBoxCountChanged += RefreshPetBoxCountText;
    }

    private void OnDestroy()
    {
        CGoldShopUI.OnPetBoxCountChanged -= RefreshPetBoxCountText;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 펫 상자를 1개 소모하여 무작위 등급·종류의 펫을 생성하고 펫 인벤토리에 추가합니다.
    /// 펫 상자가 없거나 펫 인벤토리가 가득 차면 실행되지 않습니다.
    /// </summary>
    public void GenerateRandomRankPet()
    {
        if (CJsonManager.Instance == null)
        {
            CDebug.LogError("[CGeneratePet] CJsonManager.Instance가 null입니다.");
            return;
        }

        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();

        if (data.petBoxCount <= 0)
        {
            CDebug.Log("[CGeneratePet] 펫 상자가 없습니다. 상점에서 구매해주세요.");
            return;
        }

        data.petBoxCount--;
        CJsonManager.Instance.Save(data);
        RefreshPetBoxCountText(data.petBoxCount);

        int totalWeight = _commonRate + _rareRate + _epicRate + _legendaryRate;
        int r = Random.Range(0, totalWeight + 1);

        int desiredRank;
        if      (r > _commonRate + _rareRate + _epicRate) desiredRank = 3; // Legendary
        else if (r > _commonRate + _rareRate)             desiredRank = 2; // Epic
        else if (r > _commonRate)                         desiredRank = 1; // Rare
        else                                              desiredRank = 0; // Common

        int rIndex = Random.Range(0, _petDataBaseSO.PetDataBaseCount());
        CPetDataSO so = _petDataBaseSO.GetPetDataByIndex(rIndex);

        CPetInventorySystem.Instance.AddPet(so.Id, desiredRank);

        CDebug.Log($"[CGeneratePet] 펫 소환 완료 (ID:{so.Id}, Type:{so.PetType}, Rank:{desiredRank}) | 남은 상자: {data.petBoxCount}개");
    }

    #endregion

    /// <summary>
    /// 펫을 summonCount마리 소환하고 생성된 CPetInstance 목록을 반환합니다.
    /// boxCost개의 소환권을 선차감한 뒤 summonCount마리를 생성합니다.
    ///   예) 33회 소환: summonCount=33, boxCost=30
    /// 소환권 부족 또는 인벤토리 가득 찬 경우 가능한 만큼만 생성합니다.
    /// </summary>
    public List<CPetInstance> GeneratePets(int summonCount, int boxCost)
    {
        List<CPetInstance> results = new List<CPetInstance>();

        if (CJsonManager.Instance == null) return results;

        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();

        if (data.petBoxCount < boxCost)
        {
            CDebug.Log($"[CGeneratePet] 소환권 부족 (보유: {data.petBoxCount}, 필요: {boxCost})");
            return results;
        }

        // 소환권 선차감
        data.petBoxCount -= boxCost;

        for (int i = 0; i < summonCount; i++)
        {
            if (CPetInventorySystem.Instance.IsFull) break;

            int totalWeight = _commonRate + _rareRate + _epicRate + _legendaryRate;
            int r = Random.Range(0, totalWeight + 1);

            int desiredRank;
            if      (r > _commonRate + _rareRate + _epicRate) desiredRank = 3;
            else if (r > _commonRate + _rareRate)             desiredRank = 2;
            else if (r > _commonRate)                         desiredRank = 1;
            else                                              desiredRank = 0;

            int rIndex = Random.Range(0, _petDataBaseSO.PetDataBaseCount());
            CPetDataSO so = _petDataBaseSO.GetPetDataByIndex(rIndex);

            CPetInventorySystem.Instance.AddPet(so.Id, desiredRank);

            var pets = CPetInventorySystem.Instance.Pets;
            if (pets.Count > 0)
                results.Add(pets[pets.Count - 1]);
        }

        CJsonManager.Instance.Save(data);
        RefreshPetBoxCountText(data.petBoxCount);

        return results;
    }

    #region Private Methods

    private void RefreshPetBoxCountText(int count)
    {
        if (_petBoxCountText != null)
            _petBoxCountText.text = $"x{count}";
    }

    private void RefreshPetBoxCountText()
    {
        if (CJsonManager.Instance == null) return;
        CSaveData data = CJsonManager.Instance.GetOrCreateSaveData();
        RefreshPetBoxCountText(data.petBoxCount);
    }

    #endregion
}
