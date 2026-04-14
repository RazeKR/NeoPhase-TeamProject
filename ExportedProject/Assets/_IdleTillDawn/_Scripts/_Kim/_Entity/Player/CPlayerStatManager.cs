using System;
using System.Collections;
using UnityEngine;

public class CPlayerStatManager : MonoBehaviour, IManaUser
{
    #region 인스펙터
    [Header("디버그 설정")]
    [SerializeField] private bool _isPrintLog = false;
    #endregion

    #region 내부 변수
    private CPlayerController _player;
    private CPlayerDataSO _baseData;
    // 레벨업 모디파이어
    private float[] _levelModifiers = new float[(int)EPlayerStatType.Count];
    // 추가 스탯 모디파이어
    private float[] _bonusModifiers = new float[(int)EPlayerStatType.Count];
    // buff Stat Modifier
    private float[] _temporaryModifiers = new float[(int)EPlayerStatType.Count];
    // passive Stat Modifier
    private float[] _passiveModifiers = new float[(int)EPlayerStatType.Count];
    // pet Stat Modifier (장착 펫 버프 전용, 스킬/버프와 분리)
    private float[] _petModifiers = new float[(int)EPlayerStatType.Count];
    #endregion

    #region 프로퍼티
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; set; } = 0f;

    public float[] BonusModifiers => _bonusModifiers;

    public float CurrentMana { get; private set; }
    public float MaxHealth => GetFinalStat(EPlayerStatType.Health);
    public float MaxMana => GetFinalStat(EPlayerStatType.Mana);
    public bool IsInitialized { get; private set; } = false;
    #endregion

    #region 이벤트
    public event Action<int> OnLevelUp;
    public event Action OnStatUpgraded;
    public event Action<float, float> OnManaChanged;
    public event Action<float, float> OnExpChanged;
    #endregion

    private void Awake()
    {
        _player = GetComponent<CPlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4)) AddExp(50000f);
    }

    /// <summary>
    /// 세이프 파일이 없을 때 SO 데이터만 우선 연결해 주는 초기화 메서드
    /// </summary>
    /// <param name="baseData"></param>
    public void InitBaseData(CPlayerDataSO baseData)
    {
        _baseData = baseData;
        SetModifier(CurrentLevel);

        CurrentMana = MaxMana;
        IsInitialized = true;
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
        IsInitialized = true;
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
    /// Passive Buff Stat API (for Passive Skill)
    /// </summary>
    /// <param name="statType"></param>
    /// <param name="amount"></param>
    public void SetPassiveStatUpgrade(EPlayerStatType statType, float amount)
    {
        _passiveModifiers[(int)statType] = amount;

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

        return (baseValue + _levelModifiers[index] + _bonusModifiers[index] + _temporaryModifiers[index])
               * (1 + _passiveModifiers[index] + _petModifiers[index]);
    }

    /// <summary>
    /// 장착 펫 버프 전용 API. 스킬/일반 패시브와 독립된 슬롯을 사용합니다.
    /// 펫 해제 시 amount = 0 으로 호출합니다.
    /// </summary>
    public void SetPetStatUpgrade(EPlayerStatType statType, float amount)
    {
        _petModifiers[(int)statType] = amount;
        OnStatUpgraded?.Invoke();
    }

    public const int MaxLevel = 40;

    public void AddExp(float amount)
    {
        if (CurrentLevel >= MaxLevel) return;

        float finalExp = amount * GetFinalStat(EPlayerStatType.ExpMultiplier);
        CurrentExp += finalExp;

        float requiredExp = GetRequiredExp(CurrentLevel);

        while (CurrentExp >= requiredExp && CurrentLevel < MaxLevel)
        {
            CurrentExp -= requiredExp;

            ProcessLevelUp();

            requiredExp = GetRequiredExp(CurrentLevel);
        }

        if (CurrentLevel >= MaxLevel)
            CurrentExp = 0f;

        if (_isPrintLog)
        {
            CDebug.Log($"경험치 획득 {finalExp}, 현재 경험치 {CurrentExp}/{requiredExp}");
        }

        OnExpChanged?.Invoke(CurrentExp, requiredExp);
    }

    public void ProcessLevelUp()
    {
        CurrentLevel++;
        SetModifier(CurrentLevel);

        OnLevelUp?.Invoke(CurrentLevel);
        RestoreMana(MaxMana);

        if (_isPrintLog)
        {
            CDebug.Log($"레벨업 : 현재 레벨 {CurrentLevel}");
            CDebug.Log($"현재 공격력 {GetFinalStat(EPlayerStatType.Damage)}");
        }
    }

    /// <summary>
    /// 사망 패널티: 현재 경험치의 20%를 차감하고 UI 이벤트를 발생시킨다.
    /// </summary>
    public void ApplyDeathExpPenalty()
    {
        CurrentExp *= 0.8f;
        OnExpChanged?.Invoke(CurrentExp, GetRequiredExp(CurrentLevel));
    }

    public float GetRequiredExp(int currentLevel)
    {
        return 20000f + (currentLevel * 20000f);
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
        CDebug.Log($"CPlayerStatManager : 마나 회복 (현재 : {CurrentMana}/{MaxMana}");
        OnManaChanged?.Invoke(CurrentMana, MaxMana);
    }

    public bool ConsumeMana(float amount)
    {
        if (CurrentMana - amount < 0) return false;

        CurrentMana -= amount;
        CDebug.Log($"CPlayerStatManager : 마나 사용 (사용량 : {amount}");

        OnManaChanged?.Invoke(CurrentMana, MaxMana);
        return true;
    }
    #endregion
}
