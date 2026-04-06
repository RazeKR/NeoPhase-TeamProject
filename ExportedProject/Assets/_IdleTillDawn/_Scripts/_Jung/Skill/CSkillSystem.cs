using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ��ų�� ���õ� ������ �����մϴ�.
/// JsonManager�� �����ϸ� ��ų����Ʈ �߰�, ��ų ����, ��ų ��ȭ, ��ų ����� �����մϴ�.
/// UseBindSkill(int slotIndex)�� �ҷ��� ��ų�� ����� �� �ֽ��ϴ�.
/// </summary>


public class CSkillSystem : MonoBehaviour
{
    #region SingleTon
    public static CSkillSystem Instance;
    #endregion

    #region Inspectors

    [Header("�� ���̾�")]
    [SerializeField] private LayerMask _enemyLayer;
    [Header("���� ��� (������, false �� 0 ���)")]
    [SerializeField] private bool _manaUse = true;

    #endregion

    #region Events

    /// <summary>��ų ������ ����� ������ �߻��մϴ�. UI ���� ������ ����մϴ�./// </summary>
    public event System.Action OnSkillChanged;

    /// <summary>���� ��ų�� ����Ǿ��� �� �߻��մϴ�./// </summary>
    public event System.Action OnSkillEquipped;

    #endregion

    #region PrivateVariables

    private CSaveData _saveData;    // ���̺� ������

    // ���� ��ų ���� ���� ����
    private Dictionary<string, int> _acquiredSkills = new Dictionary<string, int>();

    // ��Ÿ�� ���� ��ųʸ�
    private Dictionary<int, float> _cooldownDict = new Dictionary<int, float>();

    private GameObject GlareGO;     // PassiveSkill - 'Glare' GameObject

    private GameObject EOStormGO;   // PassiveSkill - 'Eye of The Storm' GameObject

    #endregion

    #region publicVariables

    public int currentSkillPoints;
    public CSkillDataSO CurrentlyDraggingSkill; // �巡�� ���� ���� SO
    public GameObject DragIconVisual;           // �巡�� ������    
    public List<int> _equippedSkills = new List<int> { 0, 0, 0 };      // ���� ��ų ���� ����

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

    /// <summary>��ų ��� ���� ���� ��ȯ</summary>
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
        OnSkillChanged += RefreshPassiveSkill;
        RefreshPassiveSkill();

        if (CJsonManager.Instance != null)
        {
            CJsonManager.Instance.OnLoadCompleted += RestoreFromSaveData;

            if (CJsonManager.Instance.CurrentSaveData != null)
            {
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

        OnSkillChanged -= RefreshPassiveSkill;
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

    public void RefreshPassiveSkill()
    {
        GameObject playerObj = FindObjectOfType<CPlayerStatManager>()?.gameObject;

        // 플레이어가 아직 씬에 없으면(메인메뉴, 초기화 타이밍 등) 조용히 종료
        if (playerObj == null) return;

        CPlayerController ctr = playerObj.GetComponent<CPlayerController>();
        CPlayerStatManager smg = playerObj.GetComponent<CPlayerStatManager>();
        Vector3 spawnPos = playerObj.transform.position;
        float finalScaleMult = 1.0f;

        if (GetSkillLevel(12) != 0) // Glare
        {
            CFogLightSource fog = playerObj.GetComponent<CFogLightSource>();
            CSkillDataSO data = CDataManager.Instance.GetSkill(12);

            int level = GetSkillLevel(12);

            if (fog != null)
            {
                fog.SetOuterRadius(5f + 5f * data.PassiveLevelDatas[level -1].statAmount);
            }                        
            if (GlareGO != null)
            {
                Destroy(GlareGO);
                GlareGO = null;
            }
            GlareGO = Instantiate(data.effectPrefab, spawnPos, Quaternion.identity);

            var followScript = GlareGO.GetComponent<CSkillFollow>();
            if (followScript != null)
                followScript.SetTarget(playerObj.transform);

            ISkill[] effects = GlareGO.GetComponents<ISkill>();

            foreach (var effect in effects)
            {
                effect.Init(data, level);
            }
        }

        if (GetSkillLevel(11) != 0) // Giant
        {
            finalScaleMult += (0.1f * GetSkillLevel(11));

            CSkillDataSO data = CDataManager.Instance.GetSkill(11);

            int level = GetSkillLevel(11);

            smg.SetPassiveStatUpgrade(EPlayerStatType.Health, data.PassiveLevelDatas[level - 1].statAmount);
            smg.SetPassiveStatUpgrade(EPlayerStatType.MoveSpeed, -0.016f * GetSkillLevel(11));
        }

        if (GetSkillLevel(3) != 0)  // Big Shot
        {
            CSkillDataSO data = CDataManager.Instance.GetSkill(3);

            int level = GetSkillLevel(3);

            if (ctr != null)
            {
                ctr.BulletScaleBonus = 0.4f * level;
            }            

            smg.SetPassiveStatUpgrade(EPlayerStatType.Damage, data.PassiveLevelDatas[level - 1].statAmount);
        }

        if (GetSkillLevel(2) != 0)  // Anger Point
        {
            ctr.OnDamaged -= AngerPoint;
            ctr.OnDamaged += AngerPoint;
        }
        else ctr.OnDamaged -= AngerPoint;

        if (GetSkillLevel(8) != 0)  // Fan Fire
        {
            ctr.OnShot -= FanFire;
            ctr.OnShot += FanFire;
        }
        else ctr.OnShot -= FanFire;

        if (GetSkillLevel(7) != 0)  // Eye of the Storm
        {
            CSkillDataSO data = CDataManager.Instance.GetSkill(7);

            int level = GetSkillLevel(7);

            if (EOStormGO != null)
            {
                Destroy(EOStormGO);
                EOStormGO = null;
            }
            EOStormGO = Instantiate(data.effectPrefab, spawnPos, Quaternion.identity);

            var followScript = EOStormGO.GetComponent<CSkillFollow>();
            if (followScript != null)
                followScript.SetTarget(playerObj.transform);

            ISkill[] effects = EOStormGO.GetComponents<ISkill>();

            foreach (var effect in effects)
            {
                effect.Init(data, level - 1);
            }

            // CoolDown
            StartCooldown(7, data.ActiveLevelDatas[level - 1].coolDown);
        }

        ctr.PlayerLocalScale = Vector3.one * finalScaleMult;
    }

    public void FanFire()
    {
        if (!IsSkillReady(8)) return;

        GameObject playerObj = FindObjectOfType<CPlayerStatManager>()?.gameObject;

        Vector3 spawnPos = playerObj.transform.position;

        CSkillDataSO data = CDataManager.Instance.GetSkill(8);

        spawnPos.z = 0;

        int levelIndex = GetSkillLevel(8) - 1;

        Transform target = GetNearestEnemy();
        float baseAngle = playerObj.transform.eulerAngles.z;

        float startAngle;
        float step = 36f;
        startAngle = baseAngle;

        for (int i = 0; i < 10; i++)
        {
            float currentAngle = startAngle + (step * i);
            Quaternion rot = Quaternion.Euler(0, 0, currentAngle);

            if (data.skillType == ESkillType.Buff) rot = Quaternion.identity;

            GameObject go = Instantiate(data.effectPrefab, spawnPos, rot);

            var followScript = go.GetComponent<CSkillFollow>();
            if (followScript != null)
                followScript.SetTarget(playerObj.transform);

            // ISkill Init
            ISkill[] effects = go.GetComponents<ISkill>();

            foreach (var effect in effects)
            {
                effect.Init(data, levelIndex);
            }
        }

        // CoolDown
        StartCooldown(8, data.ActiveLevelDatas[GetSkillLevel(8) - 1].coolDown);
    }

    public void AngerPoint()
    {
        if (!IsSkillReady(2)) return;

        GameObject playerObj = FindObjectOfType<CPlayerStatManager>()?.gameObject;

        CSkillDataSO data = CDataManager.Instance.GetSkill(2);

        if (data != null && playerObj != null)
        {
            CPlayerStatManager pStat = playerObj.GetComponent<CPlayerStatManager>();
            if (pStat == null) return;

            BuffLevelData datas = data.BuffLevelsDatas[GetSkillLevel(2) - 1];

            pStat.AddTemporaryBuff(datas.statType, datas.buffAmount, datas.duration);

            Debug.Log("AngerPoint On");

            // CoolDown
            StartCooldown(2, data.ActiveLevelDatas[GetSkillLevel(2) - 1].coolDown);
        }
    }

    /// <summary>
    /// Use Skill by Index
    /// </summary>
    public void UseBindSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedSkills.Count) return;
        int skillId = _equippedSkills[slotIndex];
        if (skillId == 0) return;

        CSkillDataSO data = CDataManager.Instance.GetSkill(skillId);
        if (data == null || data.effectPrefab == null) return;
        if (!IsSkillReady(skillId)) return;

        GameObject playerObj = FindObjectOfType<CPlayerStatManager>()?.gameObject;
        if (playerObj == null) return;

        IManaUser manaUser = playerObj.GetComponent<IManaUser>();
        CPlayerStatManager playerStatManager = playerObj.GetComponent<CPlayerStatManager>();

        int levelIndex = GetSkillLevel(skillId) - 1;

        int requiredMana = data.ActiveLevelDatas[levelIndex].requiredMana;

        if (manaUser != null && !manaUser.ConsumeMana(_manaUse ? requiredMana : 0))
        {
            Debug.Log($"Mana Use : {playerStatManager.CurrentMana}");
            return;
        }

        if (data.skillType == ESkillType.Buff)
        {
            CPlayerStatManager pStat = playerObj.GetComponent<CPlayerStatManager>();
            if (pStat == null) return;

            BuffLevelData datas = data.BuffLevelsDatas[levelIndex];

            pStat.AddTemporaryBuff(datas.statType, datas.buffAmount, datas.duration);

            Debug.Log("Buff On");
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

        int count = data.ActiveLevelDatas[levelIndex].projectileAmount > 0 ? data.ActiveLevelDatas[levelIndex].projectileAmount : 1;

        float startAngle;
        float step;

        if (data.circularSpread)
        {
            // 360도 원형 발사: 플레이어 기준 균등 분산
            step = 360f / count;
            startAngle = baseAngle;
        }
        else
        {
            // 부채꼴 발사: 가장 가까운 적 방향 기준 좌우 대칭
            step = data.spreadAngle;
            startAngle = baseAngle - (step * (count - 1) / 2f);
        }

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (step * i);
            Quaternion rot = Quaternion.Euler(0, 0, currentAngle);

            if (data.skillType == ESkillType.Buff) rot = Quaternion.identity;

            GameObject go = Instantiate(data.effectPrefab, spawnPos, rot);

            var followScript = go.GetComponent<CSkillFollow>();
            if (followScript != null )
                followScript.SetTarget(playerObj.transform);

            // ISkill Init
            ISkill[] effects = go.GetComponents<ISkill>();

            foreach (var effect in effects)
            {
                effect.Init(data, GetSkillLevel(skillId));
            }
        }   
        
        

        // 범위 넉백 (발동 즉시 주변 전체 적용)
        if (data.areaKnockbackRadius > 0)
            ApplyAreaKnockback(spawnPos, data.areaKnockbackRadius, data.areaKnockbackForce, data.areaKnockbackDuration);

        // CoolDown
        StartCooldown(skillId, data.ActiveLevelDatas[levelIndex].coolDown);

        OnSkillEquipped?.Invoke();
    }

    /// <summary>��Ÿ�� ���� (��ų ��� �� ȣ��)</summary>
    public void StartCooldown(int skillId, float dur)
    {
        if (dur <= 0) return;

        if (_cooldownDict.ContainsKey(skillId))
            _cooldownDict[skillId] = dur;

        else _cooldownDict.Add(skillId, dur);
    }

    /// <summary>�����ִ� ��Ÿ�� ���� ��ȯ (UI���� ����)</summary>
    public float GetCooldownNormalized(int skillId)
    {
        if (!_cooldownDict.ContainsKey(skillId)) return 0f;

        CSkillDataSO data = CDataManager.Instance.GetSkill(skillId);
        if (data == null) return 0f;

        int levelIndex = GetSkillLevel(skillId) - 1;

        return _cooldownDict[skillId] / data.ActiveLevelDatas[levelIndex].coolDown;
    }

    public float GetRemainingCoolDown(int skillId)
    {
        if (!_cooldownDict.ContainsKey(skillId)) return 0f;

        return _cooldownDict[skillId];
    }

    /// <summary>��ų����Ʈ �߰�</summary>
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

    /// <summary>��ų ���� �� JsonManager�� ����</summary>
    public bool EquipSkill(CSkillDataSO data, int slotIndex)
    {
        if (GetSkillLevel(data.Id) <= 0) return false;   // ������ ���� �� false ��ȯ

        if (data.skillType == ESkillType.Passive) return false;  // ��Ƽ�갡 �ƴ� �� false ��ȯ

        while (_equippedSkills.Count <= slotIndex)
        {
            _equippedSkills.Add(0);
        }

        for (int i = 0; i < _equippedSkills.Count; i++)         // �ߺ� ����
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
        

    /// <summary>��ų ���׷��̵� �õ�</summary>
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

        Debug.Log($"{data.skillName} : {currentSkillPoints}");

        return true;
    }


    /// <summary>���� ��ų �ǵ�</summary>
    public bool CheckPrerequisites(CSkillDataSO data)
    {
        // ���� ��ų ���� ��� ���
        if (data.prerequisiteSkills == null || data.prerequisiteSkills.Count == 0) return true;

        // ���� ��ų ����Ʈ ��ȸ �� ������ �� false ��ȯ
        foreach (var p in data.prerequisiteSkills)
        {
            if (GetSkillLevel(p.Id) <= 0) return false;
        }

        return true;
    }    

    /// <summary>��� ��� ��������</summary>
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

        OnSkillChanged?.Invoke();
    }


    /// <summary>��ų ��� �����Ѱ�</summary>
    public bool CanUnlock(CSkillNode node)
    {
        CSkillDataSO data = node.SkillData;
        int currentLevel = GetSkillLevel(data.Id);

        if (currentSkillPoints < data.requiredPoints) return false;    // ����Ʈ ����
        if (currentLevel >= data.maxLevel) return false;                // ����
        if (!CheckPrerequisites(data)) return false;                    // ���� ��ų �̴�

        return true;
    }

    #endregion

    #region PrivateMethods

    /// <summary>��ų ��Ÿ�� ������Ʈ</summary>
    private void UpdateCooldowns()
    {
        if (_cooldownDict.Count == 0) return;

        // Ű ����Ʈ
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

    /// <summary>스킬 발동 위치 기준 범위 내 모든 적에게 넉백 적용</summary>
    private void ApplyAreaKnockback(Vector3 origin, float radius, float force, float duration)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, _enemyLayer);

        foreach (Collider2D col in hits)
        {
            CEntityBase entity = col.GetComponentInParent<CEntityBase>();
            if (entity == null) continue;

            Vector2 dir = (col.transform.position - origin).normalized;
            entity.ApplyKnockback(dir * force, duration);
        }
    }

    /// <summary>���� ����� ���� Ž���Ͽ� Transform ��ȯ/// </summary>
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

    /// <summary>���̺� ���� ����</summary>
    private void RestoreFromSaveData(CSaveData save)
    {
        _saveData = save;
        currentSkillPoints = save.skillPoints;

        if (_equippedSkills == null) _equippedSkills = new List<int> { 0, 0, 0 };

        for (int i = 0; i < 3; i++)
        {
            // _equippedSkills.Count���� i�� ũ�� Add�ϰ�, �ƴϸ� ����ϴ�.
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
