using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSkillManager : MonoBehaviour
{
    public static CSkillManager Instance;

    #region Events

    /// <summary>스킬 내용이 변경될 때마다 발생합니다. UI 갱신 구독에 사용합니다./// </summary>
    public event System.Action OnSkillChanged;

    /// <summary>장착 스킬이 변경되었을 때 발생합니다./// </summary>
    public event System.Action OnSkillEquipped;

    #endregion


    #region PrivateVariables

    private CSaveData _saveData;    // 세이브 데이터

    // 습득 스킬 레벨 정보 저장
    private Dictionary<string, int> _acquiredSkills = new Dictionary<string, int>();


    #endregion

    #region publicVariables

    public int currentSkillPoints;
    public CSkillDataSO CurrentlyDraggingSkill; // 드래그 도중 저장 SO
    public GameObject DragIconVisual;           // 드래그 아이콘    
    public List<int> _equippedSkills = new List<int> { 0, 0, 0 };      // 장착 스킬 정보 저장

    #endregion

    #region Properties


    public int GetSkillLevel(int id)
    {
        if (_saveData == null)
        {
            if (CJsonManager.Instance.CurrentSaveData != null)
                return CJsonManager.Instance.CurrentSaveData.GetSkillLevel(id);

            return 0;
        }            

        return _saveData.GetSkillLevel(id);
    }

    #endregion

    #region UnityMethods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;          
    }

    private void Start()
    {
        if (CJsonManager.Instance != null)
        {
            CJsonManager.Instance.OnLoadCompleted += RestoreFromSaveData;

            if (CJsonManager.Instance.CurrentSaveData != null)
            {
                Debug.Log("[CSkillManager] 이미 로드된 데이터를 발견하여 즉시 복구합니다.");
                RestoreFromSaveData(CJsonManager.Instance.CurrentSaveData);
            }
        }
    }

    private void OnDestroy()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted -= RestoreFromSaveData;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddSkillPoint(5);
        }
    }

    #endregion

    #region PublicMethods

    // 스킬포인트 추가
    public void AddSkillPoint(int amount)
    {
        currentSkillPoints += amount;

        var save = CJsonManager.Instance.CurrentSaveData;
        if (save != null)
        {
            save.skillPoints = currentSkillPoints;
            CJsonManager.Instance.Save(save);
        }

        if (CSkillUI.Instance != null)
        {
            CSkillUI.Instance.TextSet(currentSkillPoints);
        }

        RefreshAllNodes();
    }

    // 스킬 장착 후 JsonManager로 저장
    public bool EquipSkill(CSkillDataSO data, int slotIndex)
    {
        if (GetSkillLevel(data.Id) <= 0) return false;   // 레벨이 없을 시 false 반환

        if (data.skillType != ESkillType.Active) return false;  // 액티브가 아닐 시 false 반환

        while (_equippedSkills.Count <= slotIndex)
        {
            _equippedSkills.Add(0);
        }

        for (int i = 0; i < _equippedSkills.Count; i++)         // 중복 방지
        {
            if (_equippedSkills[i] == data.Id)
            {
                _equippedSkills[i] = 0;
            }
        }

        _equippedSkills[slotIndex] = data.Id;

        CJsonManager.Instance.SaveEquippedSkill(_equippedSkills);

        OnSkillEquipped?.Invoke();
        return true;
    }


    private void RestoreFromSaveData(CSaveData save)
    {
        _saveData = save;
        currentSkillPoints = save.skillPoints;

        // [핵심] 리스트가 어떤 이유로든 줄어들지 않도록 강제로 3칸을 보장합니다.
        if (_equippedSkills == null) _equippedSkills = new List<int> { 0, 0, 0 };

        // 리스트의 내용물을 하나씩 안전하게 옮깁니다.
        for (int i = 0; i < 3; i++)
        {
            // _equippedSkills.Count보다 i가 크면 Add하고, 아니면 덮어씁니다.
            int skillId = save.GetEquippedSkill(i); // 헬퍼 메서드 사용 (인덱스 안전)

            if (i < _equippedSkills.Count)
                _equippedSkills[i] = skillId;
            else
                _equippedSkills.Add(skillId);
        }

        // 데이터 복구 후 강제 로그 확인 (여기서 Count가 무조건 3이어야 합니다)
        Debug.Log($"[복구확인] 포인트: {currentSkillPoints}, 슬롯크기: {_equippedSkills.Count}");

        OnSkillEquipped?.Invoke();
        RefreshAllNodes();

        if (CSkillUI.Instance != null) CSkillUI.Instance.UpdateUIState();
    }

    // 스킬 업그레이드 시도
    public bool TryUpgradeSkill(CSkillNode node)
    {
        CSkillDataSO data = node.SkillData;
        int currentLevel = GetSkillLevel(data.Id);

        if (!CanUnlock(node)) return false;

        currentSkillPoints -= data.requiredPoints;
        CJsonManager.Instance.CurrentSaveData.skillPoints = currentSkillPoints;

        CJsonManager.Instance.SaveSkillLevel(data.Id, currentLevel + 1);

        CSkillUI.Instance.TextSet(currentSkillPoints);
        RefreshAllNodes();

        Debug.Log($"{data.skillName} 레벨 상승, 남은 포인트 : {currentSkillPoints}");

        return true;
    }


    // 선행 스킬 판독
    public bool CheckPrerequisites(CSkillDataSO data)
    {
        // 선행 스킬 없을 경우 통과
        if (data.prerequisiteSkills == null || data.prerequisiteSkills.Count == 0) return true;

        // 선행 스킬 리스트 순회 후 미충족 시 false 반환
        foreach (var p in data.prerequisiteSkills)
        {
            if (GetSkillLevel(p.Id) <= 0) return false;
        }

        return true;
    }



    // 모든 노드 리프레쉬
    public void RefreshAllNodes()
    {
        CSkillNode[] allNodes = FindObjectsOfType<CSkillNode>();

        foreach (var node in allNodes)
        {
            node.UpdateUI();
        }

        CNodeConnector[] cons = FindObjectsOfType<CNodeConnector>();

        foreach (var c in cons)
        {
            c.UpdateLineColor();
        }
    }


    // 스킬 언락 가능한가
    public bool CanUnlock(CSkillNode node)
    {
        CSkillDataSO data = node.SkillData;
        int currentLevel = GetSkillLevel(data.Id);

        if (currentSkillPoints < data.requiredPoints) return false;    // 포인트 부족
        if (currentLevel >= data.maxLevel) return false;                // 만렙
        if (!CheckPrerequisites(data)) return false;                    // 선행 스킬 미달

        return true;
    }

    #endregion
}
