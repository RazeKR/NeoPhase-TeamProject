using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRankingTestScript : MonoBehaviour
{
    public GameObject UiCanvas;
    public CRankingUI uiScript;
    private bool _isActived = false;

    private void Start()
    {
        uiScript = FindObjectOfType<CRankingUI>();

        CRankingUI ui = FindFirstObjectByType<CRankingUI>();

        UiCanvas = ui.gameObject;

        UiCanvas.SetActive(_isActived);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            CDebug.Log("TEST 랭킹 데이터 가져오기");

            CRankingManager.Instance.GetRankingData((rankList) =>
            {
                if (rankList == null && rankList.Count == 0)
                {
                    CDebug.Log("랭킹 데이터 비어있음");
                    return;
                }

                CDebug.Log("랭킹 데이터 가져오기 성공");

                for (int i = 0; i < rankList.Count; i++)
                {
                    CRankData data = rankList[i];
                    CDebug.Log($"{i + 1}등 | 닉네임 : {data.nickname} | 스테이지 : {data.highestStageIdx} | 킬 : {data.totalKills}");
                }
            });
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            CDebug.Log("TEST 랭킹 데이터 저장");

            CRankingManager.Instance.SaveMyRanking(CJsonManager.Instance.CurrentSaveData);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            _isActived = !_isActived;
            UiCanvas.SetActive(_isActived);

            if (_isActived)
            {
                if (uiScript != null) uiScript.ShowLoadingMessage();

                CRankingManager.Instance.GetRankingData((rankList) =>
                {
                    if (uiScript != null) uiScript.DrawRankingBoard(rankList);
                });
            }
        }

        // Delete : 전체 랭킹 초기화는 CRankingManager.Update()에서 처리
    }
}
