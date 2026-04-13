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

    public static IEnumerator CoDeleteRanking(CRankData data, Action onSuccess, Action<string> onError)
    {
        string jsonToUpload = $"{{\"action\":\"delete\", \"uid\":\"{data.uid}\", \"nickname\":\"{data.nickname}\"}}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonToUpload);

        using (UnityWebRequest request = new UnityWebRequest(WEB_APP_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.timeout = 30;

            yield return request.SendWebRequest();

            string resp = request.downloadHandler?.text ?? "(응답 없음)";
            Debug.Log($"[CoDeleteRanking] uid='{data.uid}' | HTTP={request.responseCode} | 응답={resp}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(GetKoreanErrorMessage(request));
                yield break;
            }

            // HTTP 200이어도 GAS가 error를 반환할 수 있으므로 body의 status 확인
            GASResponse gasResp = JsonUtility.FromJson<GASResponse>(resp);
            if (gasResp != null && gasResp.status == "success")
            {
                onSuccess?.Invoke();
            }
            else
            {
                string reason = gasResp?.message ?? resp;
                Debug.LogError($"[CoDeleteRanking] 서버 삭제 실패 — {reason}");
                onError?.Invoke($"서버 삭제 실패: {reason}");
            }
        }
    }

    /// <summary>
    /// 서버의 모든 랭킹 데이터를 삭제하는 코루틴 메서드.
    /// 1) deleteAll 단일 요청 → 즉시 검증
    /// 2) deleteAll 실패 시에만 잔존 항목 개별 삭제 (최대 2라운드)
    ///    - uid 있는 항목: uid+nickname으로 삭제
    ///    - uid 없는 항목: nickname만으로 삭제 시도
    /// </summary>
    public static IEnumerator CoDeleteAllRankings(Action<int> onSuccess, Action<string> onError)
    {
        // Step 1: deleteAll 단일 요청
        Debug.Log("[CoDeleteAllRankings] deleteAll 요청 전송...");
        {
            byte[] body = Encoding.UTF8.GetBytes("{\"action\":\"deleteAll\"}");
            using (UnityWebRequest req = new UnityWebRequest(WEB_APP_URL, "POST"))
            {
                req.uploadHandler   = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 30;
                yield return req.SendWebRequest();
                Debug.Log($"[CoDeleteAllRankings] deleteAll 응답: {req.downloadHandler?.text}");
            }
        }
        yield return new WaitForSeconds(1f);

        // Step 2: deleteAll 직후 즉시 검증 — 성공했으면 개별 루프 없이 완료
        RankDataList afterDeleteAll = null;
        using (UnityWebRequest request = UnityWebRequest.Get(WEB_APP_URL))
        {
            request.timeout = 30;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                afterDeleteAll = JsonUtility.FromJson<RankDataList>(request.downloadHandler.text);
            else
            {
                onError?.Invoke(GetKoreanErrorMessage(request));
                yield break;
            }
        }

        int remaining = afterDeleteAll?.rankings?.Count ?? 0;
        if (remaining == 0)
        {
            Debug.Log("[CoDeleteAllRankings] deleteAll 성공 — 서버 데이터 전체 삭제 완료");
            onSuccess?.Invoke(0);
            yield break;
        }

        // Step 3: deleteAll로 지워지지 않은 잔존 항목 개별 삭제
        Debug.LogWarning($"[CoDeleteAllRankings] deleteAll 후 {remaining}개 잔존 — 개별 삭제 시작");

        int totalDeleted = 0;
        const int MAX_ROUNDS = 2;
        // 이미 삭제 성공한 키 추적 (uid 또는 nickname) — 서버 캐시로 재조회되어도 중복 시도 방지
        var deletedKeys = new HashSet<string>();

        for (int round = 1; round <= MAX_ROUNDS; round++)
        {
            // 현재 서버 잔존 목록 조회
            RankDataList rankList = null;
            using (UnityWebRequest request = UnityWebRequest.Get(WEB_APP_URL))
            {
                request.timeout = 30;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    rankList = JsonUtility.FromJson<RankDataList>(request.downloadHandler.text);
                else
                {
                    onError?.Invoke(GetKoreanErrorMessage(request));
                    yield break;
                }
            }

            if (rankList == null || rankList.rankings == null || rankList.rankings.Count == 0)
            {
                Debug.Log($"[CoDeleteAllRankings] Round {round}: 잔존 없음 — 완료");
                break;
            }

            // 이미 삭제 성공한 항목 제외
            var toDelete = rankList.rankings.FindAll(d =>
            {
                string key = string.IsNullOrEmpty(d.uid) ? $"nick:{d.nickname}" : $"uid:{d.uid}";
                return !deletedKeys.Contains(key);
            });

            if (toDelete.Count == 0)
            {
                Debug.Log($"[CoDeleteAllRankings] Round {round}: 조회된 모든 항목이 기삭제됨 — 완료");
                break;
            }

            Debug.Log($"[CoDeleteAllRankings] Round {round}: {toDelete.Count}개 개별 삭제 시작");

            foreach (CRankData data in toDelete)
            {
                string uid  = data.uid      ?? "";
                string nick = data.nickname ?? "";

                string json;
                string trackingKey;

                if (!string.IsNullOrEmpty(uid))
                {
                    // uid 있는 항목: uid+nickname으로 삭제
                    json        = $"{{\"action\":\"delete\", \"uid\":\"{uid}\", \"nickname\":\"{nick}\"}}";
                    trackingKey = $"uid:{uid}";
                }
                else if (!string.IsNullOrEmpty(nick))
                {
                    // uid 없는 구버전 항목: nickname만으로 삭제 시도
                    json        = $"{{\"action\":\"deleteByNickname\", \"nickname\":\"{nick}\"}}";
                    trackingKey = $"nick:{nick}";
                    Debug.LogWarning($"[CoDeleteAllRankings] uid 없는 항목 발견 — nickname으로만 삭제 시도: nickname='{nick}'");
                }
                else
                {
                    // uid도 nickname도 없으면 클라이언트에서 삭제 불가
                    Debug.LogError($"[CoDeleteAllRankings] uid와 nickname 모두 없는 항목 — GAS 시트에서 직접 삭제 필요");
                    continue;
                }

                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                using (UnityWebRequest request = new UnityWebRequest(WEB_APP_URL, "POST"))
                {
                    request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = 30;
                    yield return request.SendWebRequest();

                    string resp = request.downloadHandler?.text ?? "(응답 없음)";
                    Debug.Log($"[CoDeleteAllRankings] uid='{uid}' nick='{nick}' | HTTP={request.responseCode} | 응답={resp}");

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        totalDeleted++;
                        deletedKeys.Add(trackingKey);
                    }
                }

                yield return new WaitForSeconds(0.3f);
            }

            if (round < MAX_ROUNDS)
                yield return new WaitForSeconds(1f);
        }

        // 최종 검증
        RankDataList finalCheck = null;
        using (UnityWebRequest request = UnityWebRequest.Get(WEB_APP_URL))
        {
            request.timeout = 30;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                finalCheck = JsonUtility.FromJson<RankDataList>(request.downloadHandler.text);
        }

        int finalRemaining = finalCheck?.rankings?.Count ?? 0;
        if (finalRemaining > 0)
        {
            Debug.LogWarning($"[CoDeleteAllRankings] 최종 검증: {finalRemaining}개 잔존 (uid 없는 레거시 데이터 또는 GAS 서버 미지원 — 시트에서 직접 삭제 필요)");
            finalCheck.rankings?.ForEach(d => Debug.LogWarning($"  uid='{d.uid}' nickname='{d.nickname}'"));
            onError?.Invoke($"삭제 후 {finalRemaining}개 잔존 — GAS 시트에서 직접 삭제 필요");
        }
        else
        {
            Debug.Log($"[CoDeleteAllRankings] 전체 삭제 완료. 개별 삭제 {totalDeleted}개");
            onSuccess?.Invoke(totalDeleted);
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
