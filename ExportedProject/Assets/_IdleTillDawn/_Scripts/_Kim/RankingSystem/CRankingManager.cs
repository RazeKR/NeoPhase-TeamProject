using System;
using System.Collections.Generic;
using UnityEngine;

public class CRankingManager : MonoBehaviour
{
    public static CRankingManager Instance { get; private set; }

    private List<CRankData> _cachedRankingData = new List<CRankData>();

    private float _lastFetchTime = -999f;
    private const float FETCH_COOLDOWN = 30f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 서버에 있는 랭킹 데이터를 읽어오는 메서드
    /// </summary>
    /// <param name="onComplete"></param>
    public void GetRankingData(Action<List<CRankData>> onComplete)
    {
        if (Time.time - _lastFetchTime < FETCH_COOLDOWN && _cachedRankingData.Count > 0)
        {
            onComplete?.Invoke(_cachedRankingData);
            return;
        }

        StartCoroutine(CRankingAPI.CoFetchRanking(
            onSuccess: (rankList) =>
            {
                _cachedRankingData = rankList.rankings;
                _lastFetchTime = Time.time;

                onComplete?.Invoke(_cachedRankingData);
            },
            onError: (error) =>
            {
                Debug.LogError($"가져오기 실패 : {error}");
                onComplete?.Invoke(_cachedRankingData); // 이전에 캐시된 데이터라도 띄움
            }
        ));
    }

    /// <summary>
    /// 로컬 세이브 데이터를 기반으로 서버에 랭킹을 등록하는 메서드
    /// </summary>
    /// <param name="myData"></param>
    public void SaveMyRanking(CSaveData localSaveData)
    {
        if (localSaveData == null)
        {
            Debug.LogWarning("CRankingManager : 로컬 세이브 데이터가 NULL");
            return;
        }

        CRankData rankDataToUpload = CRankData.FromSaveDataToRankData(localSaveData);

        StartCoroutine(CRankingAPI.CoSaveRanking(
            rankDataToUpload,
            onSuccess: () => Debug.Log("서버 전송 완료"),
            onError: (error) => Debug.LogError($"전송 실패 : {error}")
        ));
    }
}
