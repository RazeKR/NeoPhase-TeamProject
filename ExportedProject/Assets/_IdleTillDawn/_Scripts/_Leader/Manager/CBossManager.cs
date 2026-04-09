using System;
using UnityEngine;

/// <summary>
/// 보스의 스폰, 전투 생명주기, 결과 이벤트 발행을 전담하는 매니저
/// 보스 AI와 피격 처리는 CBoss 컴포넌트에 위임하고 이 클래스는 생명주기 관리에만 집중한다
/// CStageManager는 이 클래스의 이벤트를 구독하여 보스 결과에 반응한다
/// 보스는 고정 스폰 포인트 대신 플레이어 주변 랜덤 위치에 등장하여 방향 예측을 방지한다
/// </summary>
public class CBossManager : MonoBehaviour
{
    #region Events

    public event Action OnBossDefeated;   // 보스 처치 완료 — CStageManager가 구독하여 StageClear 전환
    public event Action OnPlayerDefeated; // 보스에 의한 플레이어 사망 — CStageManager가 구독하여 리스폰 처리

    #endregion

    #region Inspector Variables

    [Header("플레이어 참조")]
    private Transform _player; // 보스 스폰 반경의 기준이 되는 플레이어 Transform (CPlayerSpawner가 주입)

    [Header("전기벽")]
    [SerializeField] private CElectircWall _electricWall; // 보스 소환 시 활성화할 전기벽

    [Header("보스 스폰 반경")]
    [SerializeField] private float _spawnMinRadius = 8f;  // 보스 스폰 최소 반경 (플레이어 발 밑 방지)
    [SerializeField] private float _spawnMaxRadius = 12f; // 보스 스폰 최대 반경 (화면 밖 등장 연출)

    #endregion

    #region Private Variables

    private CBossBase    currentBoss;    // 현재 활성 보스 인스턴스 (null이면 전투 중 아님)
    private CBossDataSO  currentBossData; // 현재 보스의 SO 데이터 (골드 보상 참조용)

    #endregion

    #region Properties

    /// <summary>
    /// 현재 활성 보스 인스턴스를 읽기 전용으로 노출한다
    /// CPlayerController의 킬존 검사가 보스 존재 여부 및 거리 계산에 활용한다
    /// 보스가 없거나 전투 중이 아니면 null을 반환한다
    /// </summary>
    public CBossBase CurrentBoss => currentBoss; // 킬존 참조용 읽기 전용 노출

    #endregion

    #region Public Methods

    /// <summary>
    /// CPlayerSpawner가 플레이어 스폰 직후 호출하여 플레이어 Transform을 주입한다
    /// </summary>
    public void SetPlayerTarget(Transform playerTransform)
    {
        _player = playerTransform;
    }

    /// <summary>
    /// 스테이지 데이터를 기반으로 보스를 플레이어 주변 랜덤 위치에 스폰하고 스탯을 초기화한다
    /// 보스 프리팹을 CStageData에서 가져오므로 스테이지마다 다른 보스를 등장시킬 수 있다
    /// 중복 스폰 방지를 위해 currentBoss가 이미 존재하면 즉시 반환한다
    /// 플레이어 주변 링 영역에 무작위 각도로 등장시켜 항상 다른 방향에서 접근하도록 한다
    /// </summary>
    /// <param name="stageData">보스 ID 및 배율 정보를 담은 스테이지 데이터</param>
    public void SpawnBoss(CStageDataSO stageData)
    {
        if (currentBoss != null) return; // 중복 스폰 방지

        if (!stageData.HasBoss)
        {
            Debug.LogWarning("[CBossManager] 이 스테이지에는 보스가 없습니다.");
            return;
        }

        CBossDataSO bossData = CDataManager.Instance.GetBoss(stageData.BossId);
        if (bossData == null || bossData.Prefab == null)
        {
            Debug.LogError($"[CBossManager] BossId {stageData.BossId}에 해당하는 보스 데이터 또는 프리팹이 없음");
            return;
        }
        currentBossData = bossData; // 골드 보상 참조를 위해 캐싱

        Vector3 spawnPos   = GetRandomSpawnPosition();                              // 플레이어 주변 랜덤 위치 계산
        GameObject bossObj = Instantiate(bossData.Prefab, spawnPos, Quaternion.identity);
        currentBoss = bossObj.GetComponent<CBossBase>();

        if (currentBoss == null)
        {
            Debug.LogError($"[CBossManager] {bossData.Prefab.name} 프리팹 루트에 CBossBase 컴포넌트가 없음");
            Destroy(bossObj);
            return;
        }

        // StageData SO 배율 대신 CGameManager 누적 공식으로 계산
        // 보스도 동일한 stageIndex 기반으로 자동 계승 스케일링이 적용된다
        currentBoss.Initialize(
            CGameManager.Instance.GetBossHpMultiplier(),
            CGameManager.Instance.GetBossAtkMultiplier(),
            _player
        );

        // 보스 결과 이벤트 구독
        currentBoss.OnDefeated     += HandleBossDefeated;
        currentBoss.OnPlayerKilled += HandlePlayerDefeated;

        // 전기벽 활성화 — 플레이어 위치를 봉쇄 중심으로 사용
        if (_electricWall != null)
            _electricWall.Activate(_player.position);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 보스 처치 이벤트를 수신하여 정리 후 CStageManager로 전달한다
    /// CleanUpBoss를 먼저 호출하여 보스 오브젝트를 제거한 뒤 이벤트를 발행한다
    /// </summary>
    private void HandleBossDefeated()
    {
        _electricWall?.Deactivate();
        CGoldManager.Instance?.AddGold(currentBossData?.BossGoldReward ?? 0); // 보스 처치 골드 보상

        int expReward = currentBossData?.BossExpReward ?? 0;
        if (expReward > 0)
            _player?.GetComponent<CPlayerStatManager>()?.AddExp(expReward); // 보스 처치 경험치 보상

        if (currentBossData?.RewardItems != null)
        {
            foreach (CBossRewardItem reward in currentBossData.RewardItems)
                CInventorySystemJ.Instance?.AddItem(reward.ItemId, reward.Count); // 보스 처치 아이템 보상
        }

        CleanUpBoss();
        OnBossDefeated?.Invoke();
    }

    /// <summary>
    /// 플레이어 사망 이벤트를 수신하여 정리 후 CStageManager로 전달한다
    /// </summary>
    private void HandlePlayerDefeated()
    {
        _electricWall?.Deactivate();
        CleanUpBoss();
        OnPlayerDefeated?.Invoke();
    }

    /// <summary>
    /// 플레이어 주변 링(도넛) 영역 내 무작위 위치를 반환한다
    /// 최소 반경을 두어 플레이어 발 밑에 스폰되는 상황을 방지하고
    /// 최대 반경을 카메라 밖으로 설정하여 보스가 화면 밖에서 등장하는 연출을 만든다
    /// </summary>
    /// <returns>보스를 스폰할 월드 좌표</returns>
    private Vector3 GetRandomSpawnPosition()
    {
        float   angle  = UnityEngine.Random.Range(0f, Mathf.PI * 2f);                   // 무작위 각도 (라디안)
        float   radius = UnityEngine.Random.Range(_spawnMinRadius, _spawnMaxRadius);     // 링 내 랜덤 반경
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        return _player.position + (Vector3)offset;                           // 플레이어 기준 오프셋 적용
    }

    /// <summary>
    /// 보스 이벤트 구독을 해제하고 인스턴스를 파괴한다
    /// Destroy 전에 반드시 구독 해제하여 이미 파괴된 객체에서 이벤트가 발행되는 상황을 방지한다
    /// </summary>
    private void CleanUpBoss()
    {
        if (currentBoss == null) return;

        currentBoss.OnDefeated     -= HandleBossDefeated;
        currentBoss.OnPlayerKilled -= HandlePlayerDefeated;
        Destroy(currentBoss.gameObject);
        currentBoss     = null;
        currentBossData = null;
    }

    #endregion

    #region Gizmos

    /// <summary>
    /// 씬 뷰에서 보스 스폰 최소/최대 반경을 시각화한다
    /// 노란색(최소), 빨간색(최대) 원으로 스폰 링 영역을 확인할 수 있다
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_player.position, _spawnMinRadius); // 최소 반경
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_player.position, _spawnMaxRadius); // 최대 반경
    }

    #endregion
}
