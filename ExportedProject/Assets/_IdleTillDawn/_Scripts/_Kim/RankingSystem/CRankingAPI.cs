using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class CRankingAPI
{
    private const string WEB_APP_URL = "https://script.google.com/macros/s/AKfycbwVZIX9ZRkvIQtU3_i0jhuEAgfTENbUvvM8TXKkuqU4arHK34BhL_f4Sepy10YAd5IT/exec";

    /// <summary>
    /// 서버에서 랭킹을 읽어오는 코루틴 메서드
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <returns></returns>
    public static IEnumerator CoFetchRanking(Action<RankDataList> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(WEB_APP_URL))
        {
            request.timeout = 30; // 30초 동안 응답이 없으면 에러 처리하고 종료

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                RankDataList rankList = JsonUtility.FromJson<RankDataList>(request.downloadHandler.text);
                onSuccess?.Invoke(rankList);
            }
            else
            {
                string koreanError = GetKoreanErrorMessage(request);
                onError?.Invoke(koreanError);
            }
        }
    }

    /// <summary>
    /// 서버에 랭킹 데이터를 저장하는 코루틴 메서드
    /// </summary>
    /// <param name="data"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <returns></returns>
    public static IEnumerator CoSaveRanking(CRankData data, Action onSuccess, Action<string> onError)
    {
        string jsonToUpload = JsonUtility.ToJson(data);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonToUpload);

        using (UnityWebRequest request = new UnityWebRequest(WEB_APP_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.timeout = 30; // 30초 동안 응답이 없으면 에러 처리하고 종료

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                string koreanError = GetKoreanErrorMessage(request);
                onError?.Invoke(koreanError);
            }
        }
    }

    /// <summary>
    /// 에러 메세지를 한글로 변경하는 메서드
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private static string GetKoreanErrorMessage(UnityWebRequest request)
    {
        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                return "인터넷 연결 상태가 불안정합니다. 와이파이나 데이터를 확인해 주세요.";
            case UnityWebRequest.Result.DataProcessingError:
                return "데이터를 처리하는 중 문제가 발생했습니다.";
            case UnityWebRequest.Result.ProtocolError:
                if (request.responseCode >= 500) // 서버 쪽 HTTP 에러 코드
                {
                    return "서버 점검 중이거나 일시적인 장애가 발생했습니다.";
                }
                else
                {
                    return $"통신 오류가 발생했습니다. (코드: {request.responseCode})";
                }
            default:
                if (request.error != null && request.error.Contains("timeout"))
                {
                    return "서버 응답이 너무 오래 걸립니다. 잠시 후 다시 시도해 주세요.";
                }
                return "알 수 없는 오류가 발생했습니다.";
        }
    }
}
