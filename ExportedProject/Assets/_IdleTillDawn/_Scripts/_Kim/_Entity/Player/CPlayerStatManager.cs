using System;
using System.Collections;
using UnityEngine;

public class CPlayerStatManager : MonoBehaviour, IManaUser
{
    #region 내부 변수
    private CPlayerDataSO _baseData;
    // 레벨업 모디파이어
    private float[] _levelModifiers = new float[(int)EPlayerStatType.Count];
    // 추가 스탯 모디파이어
    private float[] _bonusModifiers = new float[(int)EPlayerStatType.Count];
    // buff Stat Modifier
    private float[] _temporaryModifiers = new float[(int)EPlayerStatType.Count];
    #endregion

    #region 프로퍼티
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0f;

    public float[] BonusModifiers => _bonusModifiers;

    public float CurrentMana { get; private set; }
    public float MaxHealth => GetFinalStat(EPlayerStatType.Health);
    public float MaxMana => GetFinalStat(EPlayerStatType.Mana);
    #endregion

    #region 이벤트
    public event Action<int> OnLevelUp;
    public event Action OnStatUpgraded;
    #endregion

    /// <summary>
    /// 세이프 파일이 없을 때 SO 데이터만 우선 연결해 주는 초기화 메서드
    /// </summary>
    /// <param name="baseData"></param>
    public void InitBaseData(CPlayerDataSO baseData)
    {
        _baseData = baseData;
        SetModifier(CurrentLevel);

        CurrentMana = MaxMana;
    }

    /// <summary>
    /// 세이브 데이터와 SO 데이터를 기반으로 현재 레벨과 경험치를 동기화하고 모디파이어를 계산
    /// </summary>
    /// <param name="data"></param>
    public void SyncWithSaveData(CPlayerDataSO baseData, CSaveData data)
    {
        _baseData = baseData;

        CurrentLevel = data.playerLevel;
        CurrentExp = data.playerExp;

        for (int i = 0; i < (int)EPlayerStatType.Count; i++)
        {
            _bonusModifiers[i] = data.GetStatBonus(i);
        }

        SetModifier(CurrentLevel);

        CurrentMana = data.currentMana;
    }

    /// <summary>
    /// 상점이나 이벤트에서 스탯을 증가시킬 때 호출하는 API
    /// </summary>
    /// <param name="statType"></param>
    /// <param name="amount"></param>
    public void PurchaseStatUpgrade(EPlayerStatType statType, float amount)
    {
        _bonusModifiers[(int)statType] += amount;

        OnStatUpgraded?.Invoke();
    }

    /// <summary>
    /// Temporary Buff Stat API
    /// </summary>
    /// <param name="statType"></param>
    /// <param name="amount"></param>
    /// <param name="duration"></param>
    public void AddTemporaryBuff(EPlayerStatType statType, float amount, float duration)
    {
        StartCoroutine(BuffRoutine(statType, amount, duration));
    }

    private IEnumerator BuffRoutine(EPlayerStatType statType, float amount, float duration)
    {
        _temporaryModifiers[(int)statType] += amount;
        
        OnStatUpgraded?.Invoke();

        yield return new WaitForSeconds(duration);

        _temporaryModifiers[(int)statType] -= amount;
        OnStatUpgraded?.Invoke();
    }

    /// <summary>
    /// 최종 스탯을 계산한 후 반환하는 메서드
    /// </summary>
    /// <param name="type"></param>
    /// <returns>float 최종 스탯</returns>
    public float GetFinalStat(EPlayerStatType type)
    {
        int index = (int)type;
        float baseValue = _baseData.GetStatInfo(type).BaseValue;

        return baseValue + _levelModifiers[index] + _bonusModifiers[index] + _temporaryModifiers[index];
    }

    public void AddExp(float amount)
    {
        float finalExp = amount * GetFinalStat(EPlayerStatType.ExpMultiplier);
        CurrentExp += finalExp;

        float requiredExp = GetRequiredExp(CurrentLevel);

        while (CurrentExp >= requiredExp)
        {
            CurrentExp -= requiredExp;
            CurrentLevel++;

            SetModifier(CurrentLevel);
            OnLevelUp?.Invoke(CurrentLevel);

            requiredExp = GetRequiredExp(CurrentLevel);
        }
    }

    public float GetRequiredExp(int currentLevel)
    {
        return 100f + (currentLevel * 100f);
    }

    /// <summary>
    /// 플레이어 레벨을 받아 모디파이어 세팅
    /// </summary>
    /// <param name="currentLevel">플레이어 레벨</param>
    private void SetModifier(int currentLevel)
    {
        float growthCount = currentLevel - 1;

        for (int i = 0; i < (int)EPlayerStatType.Count; i++)
        {
            EPlayerStatType type = (EPlayerStatType)i;
            float growthValue = _baseData.GetStatInfo(type).GrowthPerLevel;

            _levelModifiers[i] = growthCount * growthValue;
        }
    }

    #region 인터페이스 구현부
    public void RestoreMana(float amount)
    {
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);
        Debug.Log($"CPlayerStatManager : 마나 회복 (현재 : {CurrentMana}/{MaxMana}");
    }

    public bool ConsumeMana(float amount)
    {
        if (CurrentMana - amount < 0) return false;

        CurrentMana -= amount;
        Debug.Log($"CPlayerStatManager : 마나 사용 (사용량 : {amount}");

        return true;
    }
    #endregion
}
