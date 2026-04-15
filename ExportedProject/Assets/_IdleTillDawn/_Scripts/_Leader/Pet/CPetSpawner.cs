using System.Collections;
using UnityEngine;

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

    #endregion

    #region Unity Methods

    private void Start()
    {
        // 런타임 장착/해제 이벤트 구독 (Start 기준이면 모든 Awake 완료가 보장됨)
        if (CPetInventorySystem.Instance != null)
        {
            CPetInventorySystem.Instance.OnPetEquipped   += OnPetEquipped;
            CPetInventorySystem.Instance.OnPetUnequipped += DespawnPet;
        }

        // 게임 시작 시 장착 펫 스폰: 플레이어 등록을 기다린 후 실행
        StartCoroutine(Co_WaitAndSpawn());
    }

    private void OnDestroy()
    {
        if (CPetInventorySystem.Instance != null)
        {
            CPetInventorySystem.Instance.OnPetEquipped   -= OnPetEquipped;
            CPetInventorySystem.Instance.OnPetUnequipped -= DespawnPet;
        }

        DespawnPet();
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
    /// </summary>
    private IEnumerator Co_WaitAndSpawn()
    {
        // CGameManager.CachedStatManager가 null이 아닐 때까지 대기
        // = CPlayerController.Start()에서 RegisterPlayer()가 호출될 때까지 대기
        yield return new WaitUntil(() => CGameManager.Instance?.CachedStatManager != null);

        _playerTransform = CGameManager.Instance.CachedStatManager.transform;

        // 이 시점에 RestoreFromSaveData 완료가 보장됨
        CPetInstance equipped = CPetInventorySystem.Instance?.EquippedPet;
        if (equipped != null)
            SpawnPet(equipped);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 런타임 중 펫이 장착될 때 호출됩니다.
    /// playerTransform이 없으면 CachedStatManager에서 다시 참조합니다.
    /// </summary>
    private void OnPetEquipped(CPetInstance petInstance)
    {
        if (_playerTransform == null)
            _playerTransform = CGameManager.Instance?.CachedStatManager?.transform;

        SpawnPet(petInstance);
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
