using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CHelpUI : MonoBehaviour
{
    public static CHelpUI Instance { get; private set; }

    #region Inspector

    [Header("패널 참조")]
    [SerializeField] private GameObject _helpUIPanel;

    #endregion

    #region PrivateVariables
    private CanvasGroup _canvasGroup;
    #endregion

    #region Properties

    public bool IsOpen => _canvasGroup != null && _canvasGroup.alpha > 0f;

    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_helpUIPanel != null)
        {
            _canvasGroup = _helpUIPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = _helpUIPanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        if (_helpUIPanel != null && _canvasGroup != null)
        {
            _helpUIPanel.SetActive(true);

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void OnOffHelpUIPanel()
    {
        if (_canvasGroup == null) return;

        if (IsOpen)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        else
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
