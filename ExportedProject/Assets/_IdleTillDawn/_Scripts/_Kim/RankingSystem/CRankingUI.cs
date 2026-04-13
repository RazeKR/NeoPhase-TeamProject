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

        // 1순위: 최고 스테이지 내림차순 / 2순위: 플레이어 레벨 내림차순 / 3순위: 총 킬 수 내림차순
        rankList.Sort((a, b) =>
        {
            int stageCmp = b.highestStageIdx.CompareTo(a.highestStageIdx);
            if (stageCmp != 0) return stageCmp;

            int levelCmp = b.playerLevel.CompareTo(a.playerLevel);
            if (levelCmp != 0) return levelCmp;

            return b.totalKills.CompareTo(a.totalKills);
        });

        for (int i = 0; i < rankList.Count; i++)
        {
            CRankData data = rankList[i];

            GameObject slotGo = Instantiate(_textPrefab, _contentPanel);

            TextMeshProUGUI textComponent = slotGo.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"{i + 1}위  {data.nickname}  [{data.characterType}]  Lv.{data.playerLevel}  Stage {data.highestStageIdx + 1}";
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
