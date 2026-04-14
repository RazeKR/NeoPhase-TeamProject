using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CUpgradePopUp : MonoBehaviour
{
    public static CUpgradePopUp Instance;

    [SerializeField] private GameObject _window;
    [SerializeField] private Animator _upgradeEft;
    [SerializeField] private Text _resultText;
    [SerializeField] private Text _subText;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _rankImage;
    [SerializeField] private Sprite[] _rankSprites;
    [SerializeField] private CSoundData _successSound;
    [SerializeField] private CSoundData _failSound;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_window.activeSelf)
            {
                RectTransform rectTransform = _window.GetComponent<RectTransform>();
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localMousePos);

                if (!rectTransform.rect.Contains(localMousePos))
                {
                    Hide();
                }
            }
        }
    }

    public void Show(bool isSuccess, Sprite itemSprite, int rank, string messsage = "")
    {
        _window.SetActive(true);

        _itemImage.sprite = itemSprite;
        _rankImage.sprite = _rankSprites[rank];

        if (isSuccess)
        {
            _resultText.text = "SUCCESS!";
            _subText.text = messsage;
            _upgradeEft.Play("Upgrade", 0, 0f);
            if (CAudioManager.Instance != null)
                CAudioManager.Instance.Play(_successSound);
        }
        else
        {
            _resultText.text = "Fail..";
            _subText.text = messsage;
            _upgradeEft.Play("Fail", 0, 0f);
            if (CAudioManager.Instance != null)
                CAudioManager.Instance.Play(_failSound);
        }
    }

    public void Hide()
    {
        _window.SetActive(false);
    }
}
