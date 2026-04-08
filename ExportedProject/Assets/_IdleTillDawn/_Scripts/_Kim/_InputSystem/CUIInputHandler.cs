using System.Collections;
using UnityEngine;

public class CUIInputHandler : MonoBehaviour
{
    #region 내부 변수
    private Coroutine _bindCo;
    #endregion

    private void OnEnable()
    {
        if (_bindCo != null)
        {
            StopCoroutine(_bindCo);
        }

        _bindCo = StartCoroutine(CoBindDispatcher());
    }

    private void OnDisable()
    {
        if (_bindCo != null)
        {
            StopCoroutine(_bindCo);
        }
        _bindCo = null;

        if (CInputDispatcher.Instance != null)
        {
            CInputDispatcher.Instance.OnInventory -= HandleInventoryInput;
            CInputDispatcher.Instance.OnOption -= HandleOptionInput;
            CInputDispatcher.Instance.OnShop -= HandleShopInput;
            CInputDispatcher.Instance.OnSkillTree -= HandleSkillTreeInput;
        }
    }

    private IEnumerator CoBindDispatcher()
    {
        while (CInputDispatcher.Instance == null) yield return null;

        CInputDispatcher.Instance.OnInventory += HandleInventoryInput;
        CInputDispatcher.Instance.OnOption += HandleOptionInput;
        CInputDispatcher.Instance.OnShop += HandleShopInput;
        CInputDispatcher.Instance.OnSkillTree += HandleSkillTreeInput;
    }

    private void HandleInventoryInput()
    {
        if (CInventoryUI.Instance != null)
        {
            CInventoryUI.Instance.OnOffInventoryUI();
        }
        else
        {
            Debug.LogWarning("CUIInputHandler : CInventoryUI.Instance를 찾을 수 없음");
        }
    }

    private void HandleOptionInput()
    {

    }

    private void HandleShopInput()
    {

    }

    private void HandleSkillTreeInput()
    {
        if (CSkillUI.Instance != null)
        {
            CSkillUI.Instance.OnOffSkillWindow();
        }
        else
        {
            Debug.LogWarning("CUIInputHandler : CSkillUI.Instance를 찾을 수 없음");
        }

    }
}
