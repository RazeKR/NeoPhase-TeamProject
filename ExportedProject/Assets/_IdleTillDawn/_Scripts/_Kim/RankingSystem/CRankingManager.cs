using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRankingManager : MonoBehaviour
{
    public static CRankingManager Instance { get; private set; }

    private List<CRankData> _cachedRankingData = new List<CRankData>();

    // 전체 삭제 진행 중 여부 — 이 플래그가 true이면 SaveMyRanking 차단
    private bool _isDeletingAll = false;
    // 저장 중복 요청 방지
    private bool _isSaving = false;
    // 저장 중 새 요청이 들어왔을 때 대기시킬 최신 데이터 (null이면 대기 없음)
    private CSaveData _pendingSaveData = null;

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
    /// Del 키로 전체 랭킹 초기화 (개발용 — 모든 씬에서 동작)
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (_isDeletingAll)
            {
                CDebug.LogWarning("[CRankingManager] 이미 전체 삭제 진행 중입니다.");
                return;
            }
            CDebug.Log("[CRankingManager] Del 키 감지 — 전체 랭킹 초기화 시작...");
            DeleteAllRankings(
                onSuccess: (count) => CDebug.Log($"[CRankingManager] 전체 랭킹 삭제 완료 : {count}개"),
                onError:   (error)  => CDebug.LogError($"[CRankingManager] 전체 랭킹 삭제 실패 : {error}")
            );
        }

        // P키 처리는 CGameManager.ResetAllData()에서 일괄 처리 (CJsonManager.DeleteSave → DeleteMyRanking 호출)
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
                // 닉네임이 없는 유효하지 않은 항목 제거 후 캐시
                rankList.rankings?.RemoveAll(d => string.IsNullOrEmpty(d.nickname));

                // 같은 UID 중복 항목 제거 (GAS insert 중복 방어)
                var seen = new HashSet<string>();
                rankList.rankings?.RemoveAll(d =>
                {
                    if (string.IsNullOrEmpty(d.uid) || seen.Add(d.uid)) return false;
                    return true; // 이미 seen에 있으면 중복 제거
                });

                _cachedRankingData = rankList.rankings ?? new List<CRankData>();
                _lastFetchTime = Time.time;

                onComplete?.Invoke(_cachedRankingData);
            },
            onError: (error) =>
            {
                CDebug.LogError($"가져오기 실패 : {error}");
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
            CDebug.LogWarning("CRankingManager : 로컬 세이브 데이터가 비어있음");
            return;
        }

        // 전체 삭제 진행 중에는 새 데이터 저장 차단
        if (_isDeletingAll)
        {
            CDebug.LogWarning("CRankingManager : 전체 삭제 진행 중 — 랭킹 저장 차단됨");
            return;
        }

        // UID 또는 닉네임이 없으면 서버에 저장하지 않음
        if (string.IsNullOrEmpty(localSaveData.uid) ||
            string.IsNullOrEmpty(localSaveData.nickname) ||
            localSaveData.nickname == "이름 없는 플레이어")
        {
            CDebug.LogWarning("CRankingManager : UID 또는 닉네임이 없어 랭킹 저장 생략");
            return;
        }

        // 이미 저장 요청이 진행 중이면 최신 데이터를 대기열에 보관 (드롭하지 않음)
        if (_isSaving)
        {
            CDebug.LogWarning("CRankingManager : 이미 저장 중 — 완료 후 재전송 예약");
            _pendingSaveData = localSaveData;
            return;
        }

        StartCoroutine(CoSaveMyRankingWithRetry(localSaveData));
    }

    private IEnumerator CoSaveMyRankingWithRetry(CSaveData localSaveData)
    {
        _isSaving = true;
        CRankData rankData = CRankData.FromSaveDataToRankData(localSaveData);

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            bool success = false;
            bool done    = false;

            StartCoroutine(CRankingAPI.CoSaveRanking(
                rankData,
                onSuccess: () => { success = true; done = true; },
                onError:   (e) => { CDebug.LogError($"서버 전송 실패 (시도 {attempt}/3) : {e}"); done = true; }
            ));

            yield return new WaitUntil(() => done);

            if (success)
            {
                CDebug.Log($"서버 전송 완료 (시도 {attempt}/3) | playerLevel={rankData.playerLevel}");
                _isSaving = false;
                // 저장 성공 시 캐시를 무효화해 다음 랭킹 조회 시 최신 데이터를 서버에서 받아오게 함
                _cachedRankingData.Clear();
                _lastFetchTime = -999f;

                // 저장 완료 후 대기 중인 요청이 있으면 즉시 전송
                if (_pendingSaveData != null)
                {
                    CSaveData pending = _pendingSaveData;
                    _pendingSaveData = null;
                    CDebug.Log($"[CRankingManager] 대기 중이던 랭킹 데이터 전송 | playerLevel={pending.playerLevel}");
                    SaveMyRanking(pending);
                }
                yield break;
            }

            if (attempt < 3)
                yield return new WaitForSeconds(2f);
        }

        CDebug.LogError("CRankingManager : 3회 재시도 후 랭킹 저장 실패");
        _isSaving = false;
    }

    public void DeleteMyRanking(CSaveData localSaveData)
    {
        if (localSaveData == null)
        {
            CDebug.LogWarning("CRankingManager : 삭제하려는 데이터가 비어있음");
            return;
        }

        CRankData saveDataToRankData = CRankData.FromSaveDataToRankData(localSaveData);

        StartCoroutine(CRankingAPI.CoDeleteRanking(
            saveDataToRankData,
            onSuccess: () =>
            {
                CDebug.Log($"서버 랭킹 데이터 삭제 완료 (UID : {saveDataToRankData.uid})");
                _cachedRankingData.Clear();
                _lastFetchTime = -999f;

                // 로컬 닉네임만 초기화 — uid는 유지해서 재등록 가능하게 함
                // 단, 현재 세션의 세이브 데이터와 동일한 경우에만 저장
                // (리셋 후 새 세션이 시작됐다면 이전 세이브 데이터를 덮어쓰면 안 됨)
                if (CJsonManager.Instance != null &&
                    object.ReferenceEquals(CJsonManager.Instance.CurrentSaveData, localSaveData))
                {
                    localSaveData.nickname = "";
                    CJsonManager.Instance.Save(localSaveData);
                }
                CDebug.Log("[CRankingManager] 서버 랭킹 삭제 완료 — 로컬 닉네임 초기화");
            },
            onError: (error) => CDebug.LogError($"서버 랭킹 데이터 삭제 실패 : {error}")
        ));
    }

    /// <summary>
    /// 서버에 저장된 모든 플레이어의 랭킹 데이터를 일괄 삭제합니다.
    /// </summary>
    public void DeleteAllRankings(Action<int> onSuccess = null, Action<string> onError = null)
    {
        if (_isDeletingAll)
        {
            CDebug.LogWarning("CRankingManager : 이미 전체 삭제 진행 중");
            return;
        }
        StartCoroutine(CRankingAPI.CoDeleteAllRankings(
            onSuccess: (count) =>
            {
                CDebug.Log($"전체 랭킹 초기화 완료 ({count}개 삭제)");
                _cachedRankingData.Clear();
                _lastFetchTime = -999f;
                _isDeletingAll = false;
                onSuccess?.Invoke(count);
            },
            onError: (error) =>
            {
                CDebug.LogError($"전체 랭킹 초기화 실패 : {error}");
                _isDeletingAll = false;
                onError?.Invoke(error);
            }
        ));
        _isDeletingAll = true;
    }
}
