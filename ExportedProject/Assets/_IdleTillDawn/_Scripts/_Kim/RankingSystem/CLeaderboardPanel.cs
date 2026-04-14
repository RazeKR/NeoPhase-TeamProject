using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임씬의 리더보드 패널을 제어하는 컴포넌트입니다.
/// 랭킹 버튼 onClick에 Open()을 연결하여 사용합니다.
///
/// [패널 구조 예시]
/// LeaderboardPanel (GameObject — 시작 시 비활성)
///  ├─ Header Row (순위 / 닉네임 / 캐릭터 / 레벨 / 스테이지)
///  ├─ ScrollView
///  │   └─ Content (여기에 CLeaderboardRow 프리팹이 생성됨)
///  ├─ StatusText (로딩/에러 메시지용 TMP)
///  └─ CloseButton
///
/// [의존 관계]
/// - CRankingManager.Instance : 서버 데이터 조회
/// - CJsonManager.Instance    : 내 UID 조회 (내 행 강조용)
/// </summary>
public class CLeaderboardPanel : MonoBehaviour
{
    #region Inspector

    [Header("패널 루트")]
    [SerializeField] private GameObject _panelRoot;

    [Header("리더보드 행")]
    [Tooltip("CLeaderboardRow 컴포넌트가 부착된 행 프리팹")]
    [SerializeField] private GameObject _rowPrefab;

    [Tooltip("ScrollView의 Content Transform — 여기에 행이 생성됨")]
    [SerializeField] private Transform _contentParent;

    [Header("상태 텍스트")]
    [Tooltip("로딩 / 에러 메시지를 표시할 TMP 텍스트 (선택)")]
    [SerializeField] private TextMeshProUGUI _statusText;

    [Header("닫기 버튼")]
    [SerializeField] private Button _closeButton;

    [Header("랭킹 버튼 (선택)")]
    [Tooltip("랭킹 버튼 — 여기에 연결하면 자동으로 Open() onClick 등록")]
    [SerializeField] private Button _rankingButton;

    [Header("내 순위 표시 (선택)")]
    [Tooltip("내 순위를 별도로 표시할 텍스트 (예: '내 순위: 3위')")]
    [SerializeField] private TextMeshProUGUI _myRankText;

    #endregion

    #region Properties

    public bool IsOpen => _panelRoot != null && _panelRoot.activeSelf;

    #endregion

    #region Private

    private List<GameObject> _spawnedRows = new List<GameObject>();
    private bool _isFetching = false;

    #endregion

    #region Unity

    private void Awake()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);

        if (_rankingButton != null)
            _rankingButton.onClick.AddListener(Open);
    }

    #endregion

    #region Public API

    /// <summary>리더보드 패널을 열고 데이터를 새로 불러옵니다.</summary>
    public void Open()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(true);

        FetchAndDisplay();
    }

    /// <summary>리더보드 패널을 닫습니다.</summary>
    public void Close()
    {
        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }

    #endregion

    #region Private

    private void FetchAndDisplay()
    {
        if (_isFetching) return;
        _isFetching = true;

        ClearRows();
        ShowStatus("순위 데이터를 불러오는 중...");

        if (CRankingManager.Instance == null)
        {
            ShowStatus("랭킹 매니저를 찾을 수 없습니다.");
            _isFetching = false;
            return;
        }

        CRankingManager.Instance.GetRankingData(OnDataReceived);
    }

    private void OnDataReceived(List<CRankData> rankList)
    {
        _isFetching = false;

        if (rankList == null || rankList.Count == 0)
        {
            ShowStatus("등록된 랭킹이 없습니다.");
            return;
        }

        // 닉네임이 없는 유효하지 않은 항목 제거
        rankList.RemoveAll(d => string.IsNullOrEmpty(d.nickname));

        if (rankList.Count == 0)
        {
            ShowStatus("등록된 랭킹이 없습니다.");
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

        HideStatus();

        string myUid = GetMyUid();
        int myRank   = -1;

        for (int i = 0; i < rankList.Count; i++)
        {
            CRankData data  = rankList[i];
            int rank        = i + 1;
            bool isMyRow    = !string.IsNullOrEmpty(myUid) && data.uid == myUid;

            if (isMyRow) myRank = rank;

            SpawnRow(rank, data, isMyRow);
        }

        UpdateMyRankText(myRank, rankList.Count);
    }

    private void SpawnRow(int rank, CRankData data, bool isMyRow)
    {
        if (_rowPrefab == null || _contentParent == null) return;

        GameObject go = Instantiate(_rowPrefab, _contentParent);
        _spawnedRows.Add(go);

        CLeaderboardRow row = go.GetComponent<CLeaderboardRow>();
        if (row != null)
            row.SetData(rank, data, isMyRow);
    }

    private void ClearRows()
    {
        foreach (GameObject go in _spawnedRows)
        {
            if (go != null) Destroy(go);
        }
        _spawnedRows.Clear();
    }

    private void ShowStatus(string message)
    {
        if (_statusText == null) return;
        _statusText.text = message;
        _statusText.gameObject.SetActive(true);
    }

    private void HideStatus()
    {
        if (_statusText != null)
            _statusText.gameObject.SetActive(false);
    }

    private void UpdateMyRankText(int myRank, int total)
    {
        if (_myRankText == null) return;

        _myRankText.text = myRank > 0
            ? $"내 순위: {myRank}위 / {total}명"
            : "내 순위: 집계 중";
    }

    private string GetMyUid()
    {
        if (CJsonManager.Instance == null) return string.Empty;
        CSaveData data = CJsonManager.Instance.CurrentSaveData;
        return data != null ? data.uid : string.Empty;
    }

    #endregion
}
