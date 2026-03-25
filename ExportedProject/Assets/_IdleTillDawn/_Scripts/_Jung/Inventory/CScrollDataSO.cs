using UnityEngine;


[CreateAssetMenu(menuName = "SO/Data/ScrollData", fileName = "ScrollData_")]
public class CScrollDataSO : CItemDataSO
{
    [SerializeField] private float _bonusRate = 0.00f;

    public float BonusRate => _bonusRate;   // 추가 강화 확률
}
