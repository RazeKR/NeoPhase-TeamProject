using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 스킬과 관련된 정보를 제어합니다.
/// JsonManager와 교류하며 스킬포인트 추가, 스킬 장착, 스킬 강화, 스킬 사용을 수행합니다.
/// UseBindSkill(int slotIndex)를 불러와 스킬을 사용할 수 있습니다.
/// </summary>


public class CSkillSystem : MonoBehaviour
{
    #region SingleTon
    public static CSkillSystem Instance;
    #endregion

    #region Inspectors

    [Header("적 레이어")]
    [SerializeField] private LayerMask _enemyLayer;
    [Header("마나 사용 (디버깅용, false 시 0 사용)")]
    [SerializeField] private bool _manaUse = true;

    #endregion

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

    // 쿨타임 관리 딕셔너리
    private Dictionary<int, float> _cooldownDict = new Dictionary<int, float>();

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
        if (_saveData != null)
            return _saveData.GetSkillLevel(id);

        if (CJsonManager.Instance == null) return 0;

        if (CJsonManager.Instance.CurrentSaveData != null)
            return CJsonManager.Instance.CurrentSaveData.GetSkillLevel(id);

        return 0;
    }

    /// <summary>스킬 사용 가능 상태 반환</summary>
    public bool IsSkillReady(int skillId)
    {
        return !_cooldownDict.ContainsKey(skillId);
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
                Debug.Log("[CSkillSystem] 이미 로드된 데이터를 발견하여 즉시 복구합니다.");
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

        UpdateCooldowns();
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// 스킬 사용, 바인드된 스킬 SO 내부에 저장된 내용 수행
    /// </summary>
    public void UseBindSkill(int slotIndex)
    {
        // 슬롯 유효성 검사
        if (slotIndex < 0 || slotIndex >= _equippedSkills.Count) return;
        int skillId = _equippedSkills[slotIndex];
        if (skillId == 0) return;

        // 데이터 체크
        CSkillDataSO data = CDataManager.Instance.GetSkill(skillId);
        if (data == null || data.effectPrefab == null) return;
        if (!IsSkillReady(skillId)) return;

        // 플레이어 스탯 매니저 체크
        GameObject playerObj = FindObjectOfType<CPlayerStatManager>()?.gameObject;
        if (playerObj == null) return;

        IManaUser manaUser = playerObj.GetComponent<IManaUser>();
        CPlayerStatManager playerStatManager = playerObj.GetComponent<CPlayerStatManager>();

        if (manaUser != null && !manaUser.ConsumeMana(_manaUse ? data.requiredMana : 0))
        {
            Debug.Log($"마나 부족으로 스킬 사용 불가, 현재 마나 {playerStatManager.CurrentMana}");
            return;
        }

        Vector3 spawnPos = playerObj.transform.position;
        spawnPos.z = 0;

        Transform target = GetNearestEnemy();
        float baseAngle = 0;

        if (target != null)
        {
            Vector2 dir = (target.position - spawnPos).normalized;
            baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else
        {
            baseAngle = playerObj.transform.eulerAngles.z;
        }

        int count = data.projectileAmount > 0 ? data.projectileAmount : 1;
        float spread = data.spreadAngle;

        // 중앙 baseAngle 기준으로 대칭으로 발사
        float startAngle = baseAngle - (spread * (count - 1) / 2f);

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (spread * i);
            Quaternion rot = Quaternion.Euler(0, 0, currentAngle);

            GameObject go = Instantiate(data.effectPrefab, spawnPos, rot);

            var followScript = go.GetComponent<CSkillFollow>();
            if (followScript != null )
                followScript.SetTarget(playerObj.transform);

            // ISkill 상속한 모든 스킬 컴포넌트 Init
            ISkill[] effects = go.GetComponents<ISkill>();
            int skillLevel = GetSkillLevel(skillId);

            foreach (var effect in effects)
            {
                effect.Init(data.damage, skillLevel);
            }
        }   
        
        

        // 쿨타임
        StartCooldown(skillId, data.coolDown);

        OnSkillEquipped?.Invoke();
    }

    /// <summary>쿨타임 시작 (스킬 사용 시 호출)</summary>
    public void StartCooldown(int skillId, float dur)
    {
        if (dur <= 0) return;

        if (_cooldownDict.ContainsKey(skillId))
            _cooldownDict[skillId] = dur;

        else _cooldownDict.Add(skillId, dur);
    }

    /// <summary>남아있는 쿨타임 비율 반환 (UI에서 참조)</summary>
    public float GetCooldownNormalized(int skillId)
    {
        if (!_cooldownDict.ContainsKey(skillId)) return 0f;

        CSkillDataSO data = CDataManager.Instance.GetSkill(skillId);
        if (data == null) return 0f;

        return _cooldownDict[skillId] / data.coolDown;
    }

    public float GetRemainingCoolDown(int skillId)
    {
        if (!_cooldownDict.ContainsKey(skillId)) return 0f;

        return _cooldownDict[skillId];
    }

    /// <summary>스킬포인트 추가</summary>
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

    /// <summary>스킬 장착 후 JsonManager로 저장</summary>
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
        

    /// <summary>스킬 업그레이드 시도</summary>
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


    /// <summary>선행 스킬 판독</summary>
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

    /// <summary>모든 노드 리프레쉬</summary>
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


    /// <summary>스킬 언락 가능한가</summary>
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

    #region PrivateMethods

    /// <summary>스킬 쿨타임 업데이트</summary>
    private void UpdateCooldowns()
    {
        if (_cooldownDict.Count == 0) return;

        // 키 리스트
        List<int> keys = new List<int>(_cooldownDict.Keys);

        foreach (int id in keys)
        {
            _cooldownDict[id] -= Time.deltaTime;

            if (_cooldownDict[id] <= 0f)
            {
                _cooldownDict.Remove(id);

                OnSkillEquipped?.Invoke();
            }
        }
    }

    /// <summary>가장 가까운 적을 탐색하여 Transform 반환/// </summary>
    private Transform GetNearestEnemy()
    {             
        GameObject player = FindObjectOfType<CPlayerController>()?.gameObject;
        if (player ==  null) return null;

        Vector3 currentPos = player.transform.position;

        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(currentPos, 10f, _enemyLayer);

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D col in enemyColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(currentPos, col.transform.position);
                if (dist < minDistance)
                {
                    nearest = col.transform;
                    minDistance = dist;
                }
            }
        }
        return nearest;
    }

    /// <summary>세이브 정보 복구</summary>
    private void RestoreFromSaveData(CSaveData save)
    {
        _saveData = save;
        currentSkillPoints = save.skillPoints;

        if (_equippedSkills == null) _equippedSkills = new List<int> { 0, 0, 0 };

        for (int i = 0; i < 3; i++)
        {
            // _equippedSkills.Count보다 i가 크면 Add하고, 아니면 덮어씁니다.
            int skillId = save.GetEquippedSkill(i);

            if (i < _equippedSkills.Count)
                _equippedSkills[i] = skillId;
            else
                _equippedSkills.Add(skillId);
        }


        OnSkillEquipped?.Invoke();
        RefreshAllNodes();

        if (CSkillUI.Instance != null) CSkillUI.Instance.UpdateUIState();
    }

    #endregion
}
