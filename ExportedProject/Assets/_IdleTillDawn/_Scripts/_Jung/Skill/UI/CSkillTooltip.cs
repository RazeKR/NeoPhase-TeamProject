using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CSkillTooltip : MonoBehaviour
{
    public static CSkillTooltip Instance;

    [Header("UI References")]
    [SerializeField] private GameObject _window;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _typeText;
    [SerializeField] private Text _descriptionText;

    [Header("Dynamic Table Area")]
    [SerializeField] private Transform _verticalLayout;
    [SerializeField] private GameObject _tableRowPrefab;

    [Header("ToolTipWindowOffset")]
    [SerializeField] private float _xOffset = 40;
    [SerializeField] private float _yOffset = 40;

    private List<GameObject> _activeRows = new List<GameObject>();

    private CSkillDataSO _data;
    private int _currentLevel;

    private void Awake() => Instance = this;

    private void Start()
    {
        CSkillSystem.Instance.OnSkillChanged += RefreshTooltip;
        Hide();
    }

    private void LateUpdate()
    {
        if (!_window.activeSelf) return;

        Vector2 mousePos = Input.mousePosition;

        RectTransform rect = GetComponent<RectTransform>();
        float width = rect.rect.width;
        float height = rect.rect.height;

        if (mousePos.x + width + _xOffset > Screen.width) mousePos.x -= (width + _xOffset);
        else mousePos.x += _xOffset;

        if (mousePos.y - height - _yOffset < 0) mousePos.y += (height + _yOffset);
        else mousePos.y -= _yOffset;

        transform.position = mousePos;
    }

    private void RefreshTooltip()
    {
        if (_data == null || !_window.activeSelf) return;

        _currentLevel = CSkillSystem.Instance.GetSkillLevel(_data.Id);

        ClearRows();

        _nameText.text = _data.skillName;

        string type = "";
        switch (_data.skillType)
        {
            case ESkillType.Active or ESkillType.Buff: type = "[Active]"; break;
            case ESkillType.Passive: type = "[Passive]"; break;
        }
        _typeText.text = type;

        _descriptionText.text = _data.flavourText;

        // 현재 레벨 인덱스 (리스트는 0부터 시작하므로 level - 1)
        int lvIdx = Mathf.Max(0, _currentLevel - 1);

        // 액티브 스킬 데이터가 있을 때 (Mana, CoolTime, Damage 등)
        if (_data.ActiveLevelDatas.Count > lvIdx)
        {
            var active = _data.ActiveLevelDatas[lvIdx];

            // 값이 0이 아닐 때만 유동적으로 추가
            if (active.coolDown > 0) AddStatRow("CoolTime", $"{active.coolDown}s");
            if (active.requiredMana > 0) AddStatRow("Mana", active.requiredMana.ToString());
            if (active.damage > 0) AddStatRow("Damage", active.damage.ToString());
            if (active.projectileAmount > 1) AddStatRow("Amount", active.projectileAmount.ToString());
        }

        // 버프 데이터가 있을 때
        if (_data.BuffLevelsDatas != null && _data.BuffLevelsDatas.Count > lvIdx)
        {
            var buff = _data.BuffLevelsDatas[lvIdx];
            AddStatRow(buff.statType.ToString(), $"+{buff.buffAmount}");
            AddStatRow("Duration", $"{buff.duration}s");
        }

        // 패시브 데이터가 있을 때
        if (_data.PassiveLevelDatas.Count > lvIdx)
        {
            var passive = _data.PassiveLevelDatas[lvIdx];
            AddStatRow("Bonus", $"+{passive.statAmount * 100}%");
        }

        int playerPoints = CSkillSystem.Instance.currentSkillPoints;

        if (_currentLevel != _data.maxLevel && playerPoints < _data.requiredPoints)
            AddColorRow("포인트 부족", $"{_data.requiredPoints}", Color.yellow);

        else if (_currentLevel == _data.maxLevel) return;

        else AddStatRow("포인트 요구량", $"{_data.requiredPoints}");
    }

    public void ShowTooltip(CSkillDataSO data, int currentLevel)
    {
        _window.SetActive(true);
        _data = data;
        _currentLevel = currentLevel;

        RefreshTooltip();
    }

    private void AddStatRow(string table, string value)
    {
        GameObject row = Instantiate(_tableRowPrefab, _verticalLayout);
        Text[] txs = row.GetComponentsInChildren<Text>();
        txs[0].text = table;
        txs[1].text = value;
        _activeRows.Add(row);
    }

    private void AddColorRow(string message, string value, Color color)
    {
        GameObject row = Instantiate(_tableRowPrefab, _verticalLayout);
        Text[] txs = row.GetComponentsInChildren<Text>();
        txs[0].text = message;
        txs[0].color = color;
        txs[0].fontStyle = FontStyle.Bold;
        txs[1].text = value;
        txs[0].color = color;
        txs[0].fontStyle = FontStyle.Bold;
        _activeRows.Add(row);
    }

    private void ClearRows()
    {
        foreach(var row in _activeRows) Destroy(row);
        _activeRows.Clear();
    }

    public void Hide() => _window.SetActive(false);
}
