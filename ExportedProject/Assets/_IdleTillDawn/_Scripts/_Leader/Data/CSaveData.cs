using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

/// <summary>
/// JSON으로 직렬화되는 플레이어의 모든 동적 진행 데이터를 담는 클래스입니다.
/// Unity JsonUtility 호환을 위해 Dictionary 대신 List 쌍을 사용합니다.
/// CJsonManager가 직렬화·역직렬화를 전담하며, 다른 클래스는 직접 수정하지 않습니다.
/// </summary>
[Serializable]
public class CSaveData
{
    // ── 유저 고유 정보 ────────────────────────────────────────────────────
    public string uid = "";
    public string nickname = "";
    public EPlayerType characterType;

    // ── 플레이어 기본 ─────────────────────────────────────────────────────
    public int   playerStatId    = 0;     // 사용 중인 CPlayerStatDataSO ID
    public int   playerLevel     = 1;     // 현재 플레이어 레벨
    public float playerExp       = 0f;    // 현재 경험치
    public float currentHp       = 100f;  // 현재 체력
    public float currentMana     = 100f;  // 현재 마나
    public int   gold            = 0;     // 보유 골드
    public int   diamond         = 0;     // 보유 다이아 (상점에서 획득, 골드 구매에 사용)

    // ── 스테이지 진행 ─────────────────────────────────────────────────────
    public int currentStageId   = 1;  // 현재 스테이지 ID (CStageDataSO.Id 기준)
    public int highestStageId   = 1;  // 도달한 최고 스테이지 ID
    public int currentKillCount = 0;  // 현재 스테이지 처치 수 (세션 내 진행도)
    public int totalKills       = 0;  // 누적 킬 수

    // ── 장비 ──────────────────────────────────────────────────────────────
    public int equippedWeaponId = 0;  // 장착 무기 ID (0 = 없음)

    // ── 인벤토리 (ID-Count 쌍, JsonUtility Dictionary 미지원으로 List 사용) ──
    
    public List<int> inventoryIds    = new(); // 인벤토리 아이템 ID 목록
    public List<int> inventoryCounts = new(); // 각 아이템 수량 (inventoryIds와 1:1 대응)
    /// <summary>
    /// 보유한 각 아이템의 세이브 데이터 묶음
    /// CInventorySaveData는 CItemSaveData의 묶음 리스트
    /// CItemSaveData는 SO 식별자, 인스턴스Id(무기 고유 Id), 등급, 장착여부, 강화, 수량, 종류 정보를 가짐
    /// → 각 아이템이 가지는 고유 정보가 다르기 때문에 구분을 위해서 별도로 데이터화
    /// </summary>
    public CInventorySaveData inventorySaveData = new();

    // ── 스킬 ──────────────────────────────────────────────────────────────
    public int       skillPoints          = 0;              // 보유 스킬 포인트
    public List<int> skillIds             = new();          // 해금된 스킬 ID 목록
    public List<int> skillLevelValues     = new();          // 각 스킬 레벨 (skillIds와 1:1 대응)
    public List<int> equippedSkillIds     = new() { 0, 0, 0 }; // 장착 스킬 슬롯 ID (3칸, 0 = 비어있음)


    // ── 스탯 업그레이드 (상점 구매 보너스) ────────────────────────────────
    public List<int>   statTypeIds   = new(); // EPlayerStatType을 int로 저장
    public List<float> statBonuses   = new(); // 각 스탯 보너스 수치 (statTypeIds와 1:1 대응)

    // ── 메타데이터 ────────────────────────────────────────────────────────
    public string lastSavedTime = string.Empty; // 마지막 저장 시각 (ISO 8601)
    public int    saveVersion   = 1;             // 세이브 포맷 버전 (마이그레이션 대비)

    // ── 헬퍼 메서드 (직렬화 대상 아님) ──────────────────────────────────

    /// <summary>스킬 ID로 현재 레벨을 조회합니다. 해금 전이면 0을 반환합니다.</summary>
    public int GetSkillLevel(int skillId)
    {
        int index = skillIds.IndexOf(skillId);
        if (index < 0) return 0;
        return skillLevelValues[index];
    }

    /// <summary>스킬 레벨을 설정합니다. 없으면 새로 추가합니다.</summary>
    public void SetSkillLevel(int skillId, int level)
    {
        int index = skillIds.IndexOf(skillId);
        if (index >= 0)
        {
            skillLevelValues[index] = level;
            return;
        }

        skillIds.Add(skillId);
        skillLevelValues.Add(level);
    }

    /// <summary>인덱스로 해당 인덱스에 저장된 id를 가져옵니다. 인덱스 범위 밖이면 0을 반환합니다./// </summary>
    public int GetEquippedSkill(int Index)
    {
        if (equippedSkillIds == null || Index < 0 || Index >= equippedSkillIds.Count)
            return 0;

        return equippedSkillIds[Index];
    }

    /// <summary>장착 스킬 리스트를 저장합니다. 길이가 다를 경우 취소합니다./// </summary>
    public void SetEquippedSkill(List<int> skillIds)
    {
        if (skillIds.Count != equippedSkillIds.Count) return;

        for (int i = 0; i < skillIds.Count; i++)
        {
            equippedSkillIds[i] = skillIds[i];
        }        
    }

    /// <summary>아이템 ID로 현재 보유 수량을 반환합니다. 없으면 0을 반환합니다.</summary>
    public int GetItemCount(int itemId)
    {
        int index = inventoryIds.IndexOf(itemId);
        if (index < 0) return 0;
        return inventoryCounts[index];
    }

    /// <summary>아이템 수량을 delta만큼 변경합니다. 수량이 0 이하가 되면 목록에서 제거합니다.</summary>
    public void ChangeItemCount(int itemId, int delta)
    {
        int index = inventoryIds.IndexOf(itemId);
        if (index >= 0)
        {
            inventoryCounts[index] += delta;
            if (inventoryCounts[index] <= 0)
            {
                inventoryIds.RemoveAt(index);
                inventoryCounts.RemoveAt(index);
            }
            return;
        }

        if (delta <= 0) return;

        inventoryIds.Add(itemId);
        inventoryCounts.Add(delta);
    }

    /// <summary>EPlayerStatType(int)에 해당하는 보너스 수치를 반환합니다. 없으면 0을 반환합니다.</summary>
    public float GetStatBonus(int statTypeId)
    {
        int index = statTypeIds.IndexOf(statTypeId);
        if (index < 0) return 0f;
        return statBonuses[index];
    }

    /// <summary>EPlayerStatType(int)의 보너스 수치를 설정합니다.</summary>
    public void SetStatBonus(int statTypeId, float bonus)
    {
        int index = statTypeIds.IndexOf(statTypeId);
        if (index >= 0)
        {
            statBonuses[index] = bonus;
            return;
        }

        statTypeIds.Add(statTypeId);
        statBonuses.Add(bonus);
    }

    /// <summary>
    /// 디스크 저장 없이 메모리상의 진행도 데이터만 한 번에 갱신합니다.
    /// (마지막에 CJsonManager.Save()를 한 번만 호출하기 위한 최적화 용도)
    /// </summary>
    public void UpdateProgress(int level, float exp, float hp, float mana, int stageId)
    {
        playerLevel = level;
        playerExp = exp;
        currentHp = hp;
        currentMana = mana;
        currentStageId = stageId;

        if (stageId > highestStageId)
        {
            highestStageId = stageId;
        }
    }
}
