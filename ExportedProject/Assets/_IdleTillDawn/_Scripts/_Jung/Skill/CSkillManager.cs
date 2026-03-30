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

    

    // 습득 스킬 레벨 정보 저장
    private Dictionary<string, int> _acquiredSkills = new Dictionary<string, int>();
        

    #endregion

    #region publicVariables

    public int currentSkillPoints;
    public CSkillDataSO CurrentlyDraggingSkill; // 드래그 도중 저장 SO
    public GameObject DragIconVisual;           // 드래그 아이콘    
    public CSkillDataSO[] _equippedSkills;      // 장착 스킬 정보 저장

    #endregion

    #region Properties

    public int GetSkillLevel(string skillName) => _acquiredSkills.ContainsKey(skillName) ? _acquiredSkills[skillName] : 0; // 스킬 레벨 가져오기

    private void SetSkillLevel(string skillName, int level) => _acquiredSkills[skillName] = level; // 스킬 레벨 저장

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

        _equippedSkills = new CSkillDataSO[3];
                
    }

    private void Start()
    {
        if (CJsonManager.Instance != null)
            CJsonManager.Instance.OnLoadCompleted += RestoreFromSaveData;

        RefreshAllNodes();
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
        CSkillUI.Instance.TextSet(currentSkillPoints);
        RefreshAllNodes();
    }

    // 스킬 장착
    public bool EquipSkill(CSkillDataSO data, int slotIndex)
    {
        if (GetSkillLevel(data.skillName) <= 0) return false;   // 레벨이 없을 시 false 반환

        if (data.skillType != ESkillType.Active) return false;  // 액티브가 아닐 시 false 반환

        for (int i = 0; i < _equippedSkills.Length; i++)        // 중복 방지
        {
            if (_equippedSkills[i] == data) _equippedSkills[i] = null;
        }

        _equippedSkills[slotIndex] = data;



        return true;
    }


    private void RestoreFromSaveData(CSaveData save)
    {
        currentSkillPoints = save.skillPoints;

        _acquiredSkills.Clear();


        for (int i = 0; i < save.skillIds.Count; i++)
        {
            int id = save.skillIds[i];
            int level = save.GetSkillLevel(id);

            CSkillDataSO so = CDataManager.Instance.GetSkill(id);
            
            if (so != null)
            {
                _acquiredSkills.Add(so.skillName, level);
            }
        }

        for (int i = 0; i < save.equippedSkillIds.Count; i++)
        {
            int eqId = save.equippedSkillIds[i];

            if (eqId == 0) _equippedSkills[i] = null;

            else _equippedSkills[i] = CDataManager.Instance.GetSkill(eqId);
        }
        
        RefreshAllNodes();
    }

    // 스킬 업그레이드 시도
    public bool TryUpgradeSkill(CSkillNode node)
    {
        CSkillDataSO data = node.SkillData;
        int currentLevel = GetSkillLevel(data.skillName);

        if (!CanUnlock(node)) return false;

        currentSkillPoints -= data.requiredPoints;
        SetSkillLevel(data.skillName, currentLevel + 1);

        Debug.Log($"{data.skillName} 레벨 상승, 남은 포인트 : {currentSkillPoints}");

        CSkillUI.Instance.TextSet(currentSkillPoints);

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
            if (GetSkillLevel(p.skillName) <= 0) return false;
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
        int currentLevel = GetSkillLevel(data.skillName);

        if (currentSkillPoints < data.requiredPoints) return false;    // 포인트 부족
        if (currentLevel >= data.maxLevel) return false;                // 만렙
        if (!CheckPrerequisites(data)) return false;                    // 선행 스킬 미달

        return true;
    }

    #endregion
}
