using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CUIInputHandler : MonoBehaviour
{
    #region 내부 변수
    private Coroutine _bindCo;
    
    private COptionUI _optionUI;
    private CGoldShopUI _goldShopUI;
    private CStageManager _stageManager;
    private CLeaderboardPanel _leaderboardPanel;
    #endregion

    private void OnEnable()
    {
        if (_bindCo != null)
        {
            StopCoroutine(_bindCo);
        }

        _optionUI = FindAnyObjectByType<COptionUI>();
        _goldShopUI = FindAnyObjectByType<CGoldShopUI>();
        _stageManager = FindAnyObjectByType<CStageManager>();
        _leaderboardPanel = FindAnyObjectByType<CLeaderboardPanel>(FindObjectsInactive.Include);

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
            CInputDispatcher.Instance.OnBossSummon -= HandleBossSummonInput;
            CInputDispatcher.Instance.OnOpenRanking -= HandleOpenRankingInput;
            CInputDispatcher.Instance.OnOpenPet -= HandleOpenPetInput;
            CInputDispatcher.Instance.OnOpenWeaponBox -= HandleOpenWeaponBoxInput;
        }
    }

    private IEnumerator CoBindDispatcher()
    {
        while (CInputDispatcher.Instance == null) yield return null;

        CInputDispatcher.Instance.OnInventory += HandleInventoryInput;
        CInputDispatcher.Instance.OnOption += HandleOptionInput;
        CInputDispatcher.Instance.OnShop += HandleShopInput;
        CInputDispatcher.Instance.OnSkillTree += HandleSkillTreeInput;
        CInputDispatcher.Instance.OnBossSummon += HandleBossSummonInput;
        CInputDispatcher.Instance.OnOpenRanking += HandleOpenRankingInput;
        CInputDispatcher.Instance.OnOpenPet += HandleOpenPetInput;
        CInputDispatcher.Instance.OnOpenWeaponBox += HandleOpenWeaponBoxInput;
    }

    private void HandleInventoryInput()
    {
        if (CInventoryUI.Instance != null)
        {
            CInventoryUI.Instance.OnOffInventoryUI();
        }
        else
        {
            CDebug.LogWarning("CUIInputHandler : CInventoryUI.Instance를 찾을 수 없음");
        }
    }

    private void HandleOptionInput()
    {
        // 인게임 ESC 메뉴가 있으면 해당 스크립트에 위임
        // CInGameEscMenu가 CInputDispatcher.OnOption을 직접 구독하므로 여기서 중복 처리하지 않음
        if (FindAnyObjectByType<CInGameEscMenu>() != null) return;

        // 메인메뉴 씬: COptionUI 직접 제어
        if (_optionUI != null)
        {
            if (_optionUI.IsOptionOpen) _optionUI.Hide();
            else                       _optionUI.Show();
        }
        else
        {
            CDebug.LogWarning("CUIInputHandler : 씬에 COptionUI 없음");
        }
    }

    private void HandleShopInput()
    {
        if (CShopUI.Instance != null)
        {
            if (CShopUI.Instance.IsOpen)
            {
                CShopUI.Instance.CloseShop();
            }
            else
            {
                CShopUI.Instance.OpenShop();

                if (_goldShopUI != null)
                {
                    _goldShopUI.OnShopOpened();
                }
            }
        }
        else
        {
            CDebug.LogWarning("CUIInputHandler : CShopUI.Instance를 찾을 수 없음");
        }
    }

    private void HandleSkillTreeInput()
    {
        if (CSkillUI.Instance != null)
        {
            CSkillUI.Instance.OnOffSkillWindow();
        }
        else
        {
            CDebug.LogWarning("CUIInputHandler : CSkillUI.Instance를 찾을 수 없음");
        }

    }

    private void HandleBossSummonInput()
    {
        if (_stageManager != null)
        {
            _stageManager.OnBossChallengeButtonPressed();
        }
        else
        {
            CDebug.LogWarning("CUIInputHandler : CStageManager를 찾을 수 없음");
        }
    }

    private void HandleOpenRankingInput()
    {
        if (_leaderboardPanel != null)
        {
            if (_leaderboardPanel.IsOpen)
            {
                _leaderboardPanel.Close();
            }
            else
            {
                _leaderboardPanel.Open();
            }
        }
        else
        {
            CDebug.LogWarning("CUIInputHandler : CLeaderboardPanel을 찾을 수 없음");
        }
    }

    private void HandleOpenPetInput()
    {
        if (CPetInventoryUI.Instance != null)
        {
            CPetInventoryUI.Instance.OnOffPetInventoryUI();
        }
        else
        {
            CDebug.LogWarning("CUIInputHandler : CPetInventoryUI를 찾을 수 없음");
        }
    }

    private void HandleOpenWeaponBoxInput()
    {
        CGenerateItem weaponBox = FindAnyObjectByType<CGenerateItem>();

        if (weaponBox != null)
        {
            weaponBox.GenerateRandomRankItem();
        }
        else
        {
            CDebug.LogWarning("CUIInputHandler : CGenerateItem을 찾을 수 없음");
        }
    }
}
