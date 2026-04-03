using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRankingTestScript : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Debug.Log("TEST 랭킹 데이터 가져오기");

            CRankingManager.Instance.GetRankingData((rankList) =>
            {
                if (rankList == null && rankList.Count == 0)
                {
                    Debug.Log("랭킹 데이터 비어있음");
                    return;
                }

                Debug.Log("랭킹 데이터 가져오기 성공");

                for (int i = 0; i < rankList.Count; i++)
                {
                    CRankData data = rankList[i];
                    Debug.Log($"{i + 1}등 | 닉네임 : {data.nickname} | 스테이지 : {data.highestStageIdx} | 킬 : {data.totalKills}");
                }
            });
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Debug.Log("TEST 랭킹 데이터 저장");

            CRankData myData = CRankData.FromSaveDataToRankData(CJsonManager.Instance.CurrentSaveData);

            CRankingManager.Instance.SaveMyRanking(myData);
        }
    }
}
