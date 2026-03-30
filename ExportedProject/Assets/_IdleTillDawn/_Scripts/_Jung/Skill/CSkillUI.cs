using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSkillUI : MonoBehaviour
{
    public static CSkillUI Instance { get; private set; }

    [SerializeField] private GameObject _skillWindowUI;
    [SerializeField] private Text _pointsText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
               

        _skillWindowUI.SetActive(false);
    }


    private void Start()
    {
        CSkillManager.Instance.RefreshAllNodes();

        _pointsText.text = CSkillManager.Instance.currentSkillPoints.ToString();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    

    public void TextSet(int amount)
    {
        if (_pointsText != null)
        {
            _pointsText.text = amount.ToString();
        }
    }


    // 창 On/Off
    // 버튼에 연결하여 사용
    public void OnOffSkillWindow()
    {
        if (_skillWindowUI == null) return;

        bool willBeActive = !_skillWindowUI.activeSelf;
        _skillWindowUI.SetActive(willBeActive);

        if (willBeActive)
        {
            UpdateUIState();
        }
    }

    public void UpdateUIState()
    {
        if (CSkillManager.Instance == null) return;

        TextSet(CSkillManager.Instance.currentSkillPoints);

        CSkillManager.Instance.RefreshAllNodes();


    }
}
