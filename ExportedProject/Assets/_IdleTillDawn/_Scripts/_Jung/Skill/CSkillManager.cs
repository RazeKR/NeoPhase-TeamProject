using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSkillManager : MonoBehaviour
{
    [SerializeField] private bool _showDebug = false;

    public static CSkillManager Instance;

    // 스킬 레벨 가져오기
    public int GetSkillLevel(string skillName) => _acquiredSkills.ContainsKey(skillName) ? _acquiredSkills[skillName] : 0;
    // 스킬 레벨 저장
    private void SetSkillLevel(string skillName, int level) => _acquiredSkills[skillName] = level;

    // 스킬 포인트, 외부에서 자유롭게 접근하여 수정
    public int currentSkillPoints = 10;

    // 노드 커넥터에서 확인하는 라인 부모
    public RectTransform lineParent;


    public List<CSkillDataSO> allSkillDataCache;

    // 습득 스킬 레벨 정보 저장
    private Dictionary<string, int> _acquiredSkills = new Dictionary<string, int>();

    // 장착 스킬 정보 저장
    private CSkillDataSO[] _equippedSkills;

    // Application.persistentDataPath : OS별로 데이터 저장이 허용된 안전 경로 탐색. 게임 삭제해도 데이터 유지
    private string SavePath => Path.Combine(Application.persistentDataPath, "skillSave.json");


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _equippedSkills = new CSkillDataSO[3];

        PreloadSkillData();
        LoadSkill();
        RefreshAllNodes();
    }


    private void OnDestroy()
    {
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

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetAllSkills();
        }
    }


    // 스킬포인트 추가
    public void AddSkillPoint(int amount)
    {
        currentSkillPoints += amount;
        CSkillUI.Instance.TextSet(currentSkillPoints);
        RefreshAllNodes();
    }


    // 무기 장착
    public bool EquipSkill(CSkillDataSO data, int slotIndex)
    {
        if (GetSkillLevel(data.skillName) <= 0) return false;   // 레벨이 없을 시 false 반환

        if (data.skillType != ESkillType.Active) return false;  // 액티브가 아닐 시 false 반환

        for (int i = 0; i < _equippedSkills.Length; i++)        // 중복 방지
        {
            if (_equippedSkills[i] == data) _equippedSkills[i] = null;
        }

        _equippedSkills[slotIndex] = data;
        if (_showDebug) Debug.Log($"{slotIndex}번 슬롯 : {data.skillName} 장착");

        return true;
    }


    // 스킬 저장 (정보 : 스킬명 -> 스킬 레벨)
    public void SaveSkill()
    {
        CSkillSaveData save = new CSkillSaveData();
        save.remainingPoints = currentSkillPoints;

        foreach (var s in _acquiredSkills)
        {
            save.skillList.Add(new CSkillInstance { skillName = s.Key, level = s.Value });

            string json = JsonUtility.ToJson(save, true);
            File.WriteAllText(SavePath, json);            
        }

        if (_showDebug) Debug.Log($"스킬 저장 완료, 저장 경로 : {SavePath}");
    }



    private void PreloadSkillData()
    {
        CSkillDataSO[] allItems = Resources.LoadAll<CSkillDataSO>("SO/Skills");

        foreach (var item in allItems)
        {
            allSkillDataCache.Add(item);
        }
        if (_showDebug) Debug.Log("스킬 캐시 완료");
    }


    public void LoadSkill()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        CSkillSaveData save = JsonUtility.FromJson<CSkillSaveData>(json);

        currentSkillPoints = save.remainingPoints;

        _acquiredSkills.Clear();

        // 배운 스킬 정보 불러오기
        foreach(var s in save.skillList)
        {
            _acquiredSkills.Add(s.skillName, s.level);
        }

        // 장착한 스킬 정보 불러오기
        for (int i = 0; i < save.equippedSkillName.Length; i++)
        {
            string saveName = save.equippedSkillName[i];
            if (!string.IsNullOrEmpty(saveName))
            {
                _equippedSkills[i] = allSkillDataCache.Find(s => s.skillName == saveName);
            }
        }
        
        RefreshAllNodes();
        if (_showDebug) Debug.Log("스킬 불러오기 완료");
    }

    // 스킬 업그레이드 시도
    public bool TryUpgradeSkill(CSkillNode node)
    {
        CSkillDataSO data = node.SkillData;
        int currentLevel = GetSkillLevel(data.skillName);

        if (!CanUnlock(node)) return false;

        currentSkillPoints -= data.requiredPoints;
        SetSkillLevel(data.skillName, currentLevel + 1);

        if (_showDebug) Debug.Log($"{data.skillName} 레벨 상승, 남은 포인트 : {currentSkillPoints}");

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

        SaveSkill();
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


    public void ResetAllSkills()
    {
        _acquiredSkills.Clear();

        currentSkillPoints = 10;

        for (int i = 0; i < _equippedSkills.Length; i++)
        {
            _equippedSkills[i] = null;
        }

        RefreshAllNodes();
        CSkillUI.Instance.TextSet(currentSkillPoints);
        SaveSkill();

        if (_showDebug) Debug.Log("스킬 정보 초기화");
    }
}
