using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 모든 펫 SO를 한 곳에서 관리하는 데이터베이스 ScriptableObject입니다.
/// CGeneratePet에서 가챠 시 무작위 펫 종류를 선택할 때 사용됩니다.
/// </summary>
[CreateAssetMenu(menuName = "SO/Data/PetDataBase", fileName = "PetDataBase")]
public class CPetDataBaseSO : ScriptableObject
{
    [SerializeField] private List<CPetDataSO> _petDataBase = new List<CPetDataSO>();

    public CPetDataSO GetPetDataByIndex(int index) => _petDataBase[index];

    public CPetDataSO GetPetDataById(string id)
        => _petDataBase.Find(p => p.ItemId == id);

    public int PetDataBaseCount() => _petDataBase.Count;
}
