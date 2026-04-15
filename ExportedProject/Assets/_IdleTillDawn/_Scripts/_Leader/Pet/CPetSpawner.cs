using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 펫 장착/해제 이벤트를 감지하여 씬에 펫 GameObject를 동적으로 스폰/삭제합니다.
///
/// [부착 위치]
///   항상 활성화된 오브젝트에 부착해야 합니다. (Player, GameManager 등)
///
/// [초기 스폰 보장 전략]
///   Co_WaitAndSpawn 코루틴이 플레이어 등록을 기다립니다.
///   CPlayerController.Start()에서 RegisterPlayer()가 호출되는 시점(Frame 2)이면
///   CJsonManager.Load()와 CPetInventorySystem.RestoreFromSaveData()가
///   모두 완료된 것이 보장됩니다.
///   → 타이밍 경쟁 없이 EquippedPet과 playerTransform을 동시에 확보합니다.
///
/// [씬 전환 대응]
///   DontDestroyOnLoad 오브젝트에 부착된 경우 Start()가 최초 1회만 실행됩니다.
///   SceneManager.sceneLoaded를 구독하여 씬 전환 시마다 새 CPetInventorySystem에
///   재구독하고 Co_WaitAndSpawn을 재시작하여 펫 스폰·버프를 보장합니다.
/// </summary>
public class CPetSpawner : MonoBehaviour
{
    #region Inspector

    [Tooltip("펫 스프라이트의 Sorting Layer 이름. 비워두면 'Default'를 사용합니다.")]
    [SerializeField] private string _sortingLayerName = "Default";

    [Tooltip("펫 스프라이트의 Order in Layer.")]
    [SerializeField] private int _sortingOrder = 5;

    #endregion

    #region Private Variables

    private GameObject _activePetObject;
    private Transform  _playerTransform;
    private Coroutine  _spawnCoroutine;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // SceneManager.sceneLoaded는 Awake/OnEnable 이후, Start() 이전에 발생합니다.
        // DontDestroyOnLoad 오브젝트가 씬 전환 후 새 CPetInventorySystem에 재구독하도록 등록합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // 오브젝트가 씬 전용(Player 등)인 경우 Start()가 씬마다 실행되므로
        // 여기서도 ReInitialize를 호출합니다.
        // DontDestroyOnLoad 오브젝트의 경우 OnSceneLoaded가 Start() 전에 실행되므로
        // ReInitialize가 두 번 호출될 수 있지만 멱등(idempotent)하게 설계되어 안전합니다.
        ReInitialize();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (CGameManager.Instance != null)
            CGameManager.Instance.OnPlayerRegistered -= OnPlayerRegistered;

        if (CPetInventorySystem.Instance != null)
        {
            CPetInventorySystem.Instance.OnPetEquipped   -= OnPetEquipped;
            CPetInventorySystem.Instance.OnPetUnequipped -= DespawnPet;
        }

        DespawnPet();
    }

    /// <summary>
    /// 씬 전환 완료 시 호출됩니다 (Awake/OnEnable 이후, Start() 이전).
    /// 새 씬의 CPetInventorySystem에 재구독하고 스폰 코루틴을 재시작합니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReInitialize();
    }

    #endregion

    #region Reinitialization

    /// <summary>
    /// 씬 전환 또는 최초 초기화 시 공통으로 호출됩니다.
    /// - 이전 펫 오브젝트 제거 및 플레이어 참조 초기화
    /// - CGameManager.OnPlayerRegistered 재구독 (씬 전환 후 새 플레이어 등록 감지)
    /// - 새 씬의 CPetInventorySystem에 재구독 (중복 방지 포함)
    /// - 스폰 코루틴 재시작
    /// </summary>
    private void ReInitialize()
    {
        // 이전 펫·플레이어 참조 초기화
        DespawnPet();
        _playerTransform = null;

        // CGameManager.OnPlayerRegistered 구독 (DontDestroyOnLoad이므로 항상 존재)
        // 플레이어가 씬에 등록될 때마다 스폰을 보장합니다.
        if (CGameManager.Instance != null)
        {
            CGameManager.Instance.OnPlayerRegistered -= OnPlayerRegistered;
            CGameManager.Instance.OnPlayerRegistered += OnPlayerRegistered;
        }

        // 새 씬의 CPetInventorySystem에 재구독 (-=로 중복 방지 후 +=)
        if (CPetInventorySystem.Instance != null)
        {
            CPetInventorySystem.Instance.OnPetEquipped   -= OnPetEquipped;
            CPetInventorySystem.Instance.OnPetUnequipped -= DespawnPet;
            CPetInventorySystem.Instance.OnPetEquipped   += OnPetEquipped;
            CPetInventorySystem.Instance.OnPetUnequipped += DespawnPet;
        }

        // 기존 코루틴 중단 후 재시작 (중복 실행 방지)
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
        _spawnCoroutine = StartCoroutine(Co_WaitAndSpawn());
    }

    #endregion

    #region Initial Spawn Coroutine

    /// <summary>
    /// 플레이어가 등록될 때까지 기다렸다가 장착 펫을 스폰합니다.
    ///
    /// CPlayerController.Start()가 실행되는 시점(Frame 2 이상)은
    /// CPlayerSpawner.Start()가 호출한 CJsonManager.Load()와
    /// CPetInventorySystem.Start()의 RestoreFromSaveData()가
    /// 모두 완료된 이후임이 보장됩니다.
    ///
    /// 씬 전환 후에는 이전 씬 플레이어가 파괴돼 CachedStatManager가 Unity null이 되므로
    /// 새 플레이어가 RegisterPlayer()를 호출할 때까지 올바르게 대기합니다.
    /// </summary>
    private IEnumerator Co_WaitAndSpawn()
    {
        // CGameManager.CachedStatManager가 null이 아닐 때까지 대기
        // = CPlayerController.Start()에서 RegisterPlayer()가 호출될 때까지 대기
        // 씬 전환 직후에는 이전 플레이어가 파괴돼 Unity null check에 의해 false가 되므로
        // 새 플레이어 등록 시까지 올바르게 대기합니다.
        yield return new WaitUntil(() => CGameManager.Instance?.CachedStatManager != null);

        _playerTransform = CGameManager.Instance.CachedStatManager.transform;

        // 이 시점에 RestoreFromSaveData 완료가 보장됨
        CPetInstance equipped = CPetInventorySystem.Instance?.EquippedPet;
        if (equipped != null)
            SpawnPet(equipped);

        _spawnCoroutine = null;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 런타임 중 펫이 장착될 때 호출됩니다.
    /// playerTransform이 없으면 CachedStatManager에서 다시 참조합니다.
    /// 플레이어가 아직 등록되지 않은 경우(씬 전환 초기 Awake 단계 등) Co_WaitAndSpawn을 재시작합니다.
    /// </summary>
    private void OnPetEquipped(CPetInstance petInstance)
    {
        if (_playerTransform == null)
            _playerTransform = CGameManager.Instance?.CachedStatManager?.transform;

        // playerTransform이 여전히 null이면 플레이어가 아직 등록되지 않은 상태
        // → OnPlayerRegistered 이벤트 또는 Co_WaitAndSpawn 코루틴이 이후 스폰을 처리합니다.
        if (_playerTransform == null)
        {
            CDebug.Log("[CPetSpawner] OnPetEquipped: 플레이어 미등록 상태 — Co_WaitAndSpawn으로 스폰을 위임합니다.");
            if (_spawnCoroutine == null)
                _spawnCoroutine = StartCoroutine(Co_WaitAndSpawn());
            return;
        }

        SpawnPet(petInstance);
    }

    /// <summary>
    /// CGameManager.OnPlayerRegistered 이벤트 핸들러.
    /// 씬 전환 후 플레이어가 씬에 등록될 때마다 호출됩니다.
    /// CPetBuffApplier와 동일한 방식으로 스폰을 보장합니다.
    /// </summary>
    private void OnPlayerRegistered(CPlayerStatManager statManager)
    {
        _playerTransform = statManager != null ? statManager.transform : null;

        // 진행 중인 코루틴이 있으면 중단 (조건이 이미 충족됐으므로 코루틴 불필요)
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        // 이미 활성 펫이 있으면 재스폰 불필요
        if (_activePetObject != null) return;

        if (_playerTransform == null)
        {
            CDebug.LogWarning("[CPetSpawner] OnPlayerRegistered: statManager.transform이 null입니다.");
            return;
        }

        CPetInstance equipped = CPetInventorySystem.Instance?.EquippedPet;
        if (equipped != null)
            SpawnPet(equipped);
    }

    #endregion

    #region Spawn / Despawn

    private void SpawnPet(CPetInstance petInstance)
    {
        DespawnPet();

        if (petInstance?._data == null) return;
        if (_playerTransform == null)
        {
            CDebug.LogWarning("[CPetSpawner] playerTransform이 null입니다. 펫 스폰을 건너뜁니다.");
            return;
        }

        CPetDataSO data = petInstance._data;

        // ─── GameObject 생성 ───────────────────────────────────────────────
        _activePetObject = new GameObject($"Pet_{data.name}");

        // ─── SpriteRenderer ────────────────────────────────────────────────
        SpriteRenderer sr = _activePetObject.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = string.IsNullOrEmpty(_sortingLayerName) ? "Default" : _sortingLayerName;
        sr.sortingOrder     = _sortingOrder;

        switch (data.PetType)
        {
            case EPetType.AttackPowerBoost:
                sr.sprite = data.MeleeSprite;
                break;
            case EPetType.ProjectileBoost:
            case EPetType.AttackSpeedBoost:
                if (data.IdleSprites != null && data.IdleSprites.Length > 0)
                    sr.sprite = data.IdleSprites[0];
                break;
        }

        // ─── AttackPowerBoost 전용: 물리 컴포넌트 ─────────────────────────
        if (data.PetType == EPetType.AttackPowerBoost)
        {
            Rigidbody2D rb            = _activePetObject.AddComponent<Rigidbody2D>();
            rb.bodyType               = RigidbodyType2D.Kinematic;
            rb.gravityScale           = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D col = _activePetObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius    = 0.35f;
        }

        // ─── CPetOrbitController 초기화 ────────────────────────────────────
        CPetOrbitController controller = _activePetObject.AddComponent<CPetOrbitController>();
        controller.Init(petInstance, _playerTransform, sr, initialOrbitAngle: 0f);
    }

    private void DespawnPet()
    {
        if (_activePetObject != null)
        {
            Destroy(_activePetObject);
            _activePetObject = null;
        }
    }

    #endregion
}
