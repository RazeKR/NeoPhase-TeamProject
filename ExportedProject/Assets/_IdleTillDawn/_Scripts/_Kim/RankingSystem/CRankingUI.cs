using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CRankingUI : MonoBehaviour
{
    #region 인스펙터
    [Header("UI 연결")]
    [SerializeField] private Transform _contentPanel;
    [SerializeField] private GameObject _textPrefab;
    #endregion

    public void DrawRankingBoard(List<CRankData> rankList)
    {
        foreach (Transform child in _contentPanel)
        {
            Destroy(child.gameObject);
        }

        if (rankList == null || rankList.Count == 0)
        {
            GameObject emptyGo = Instantiate(_textPrefab, _contentPanel);
            emptyGo.GetComponent<TextMeshProUGUI>().text = "등록된 랭킹이 없습니다.";
            return;
        }

        for (int i = 0; i < rankList.Count; i++)
        {
            CRankData data = rankList[i];

            GameObject slotGo = Instantiate(_textPrefab, _contentPanel);

            TextMeshProUGUI textComponent = slotGo.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"{i + 1}위 - {data.nickname} [Stage:{data.highestStageIdx} | {data.totalKills}Kills]";
            }
        }
    }

    public void ShowLoadingMessage()
    {
        foreach (Transform child in _contentPanel)
        {
            Destroy(child.gameObject);
        }

        GameObject loadingGo = Instantiate(_textPrefab, _contentPanel);

        TextMeshProUGUI textComponent = loadingGo.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = "Data Loading..";
        }
    }
}
