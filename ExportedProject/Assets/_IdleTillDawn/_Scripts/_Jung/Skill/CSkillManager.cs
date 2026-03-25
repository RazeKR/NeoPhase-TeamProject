using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSkillManager : MonoBehaviour
{
    public static CSkillManager Instance;

    // 스킬 레벨 가져오기
    public int GetSkillLevel(string skillName) => _acquiredSkills.ContainsKey(skillName) ? _acquiredSkills[skillName] : 0;
    // 스킬 레벨 저장
    private void SetSkillLevel(string skillName, int level) => _acquiredSkills[skillName] = level;

    // 스킬 포인트, 외부에서 자유롭게 접근하여 수정
    public int _currentSkillPoints = 10;

    // 노드 커넥터에서 확인하는 라인 부모
    public RectTransform _lineParent;
        
    // 습득 스킬 레벨 정보 저장
    private Dictionary<string, int> _acquiredSkills = new Dictionary<string, int>();


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


    // 스킬 저장 (정보 : 스킬명 -> 스킬 레벨)
    public void SaveSkill()
    {
        CSkillSaveData save = new CSkillSaveData();
        save.remainingPoints = _currentSkillPoints;

        foreach (var s in _acquiredSkills)
        {
            save.skillList.Add(new CSkillInstance { skillName = s.Key, level = s.Value });

            string json = JsonUtility.ToJson(save, true);
            File.WriteAllText(SavePath, json);            
        }

        Debug.Log($"스킬 저장 완료, 저장 경로 : {SavePath}");
    }


    public void LoadSkill()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        CSkillSaveData save = JsonUtility.FromJson<CSkillSaveData>(json);

        _currentSkillPoints = save.remainingPoints;

        _acquiredSkills.Clear();

        foreach(var s in save.skillList)
        {
            _acquiredSkills.Add(s.skillName, s.level);
        }

        RefreshAllNodes();
        Debug.Log("스킬 불러오기 완료");
    }

    // 스킬 업그레이드 시도
    public bool TryUpgradeSkill(CSkillNode node)
    {
        CSkillDataSO data = node.SkillData;
        int currentLevel = GetSkillLevel(data.skillName);

        if (!CanUnlock(node)) return false;

        _currentSkillPoints -= data.requiredPoints;
        SetSkillLevel(data.skillName, currentLevel + 1);

        Debug.Log($"{data.skillName} 레벨 상승, 남은 포인트 : {_currentSkillPoints}");

        CSkillUI.Instance.TextSet(_currentSkillPoints);

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

        SaveSkill();
    }


    // 스킬 언락 가능한가
    public bool CanUnlock(CSkillNode node)
    {
        CSkillDataSO data = node.SkillData;
        int currentLevel = GetSkillLevel(data.skillName);

        if (_currentSkillPoints < data.requiredPoints) return false;    // 포인트 부족
        if (currentLevel >= data.maxLevel) return false;                // 만렙
        if (!CheckPrerequisites(data)) return false;                    // 선행 스킬 미달

        return true;
    }
}
