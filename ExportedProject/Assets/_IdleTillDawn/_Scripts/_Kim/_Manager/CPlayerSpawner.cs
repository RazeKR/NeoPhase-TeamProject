using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CPlayerSpawner : MonoBehaviour
{
    #region Inspector Variables

    [Header("플레이어 참조 수신 대상")]
    [SerializeField] private _CPlayerCameraController _cameraController;
    [SerializeField] private CSpawnManager            _spawnManager;
    [SerializeField] private CWorldShift              _worldShift;           // 기본 스테이지용 (상하좌우)
    [SerializeField] private CWorldShiftHorizontal    _worldShiftHorizontal; // Stage3 전용 (좌우만)
    [SerializeField] private CWeaponEquip             _weaponEquip;
    [SerializeField] private CBossManager             _bossManager;

    [Header("무기 오브젝트 설정")]
    [Tooltip("플레이어 프리팹 내 무기 스프라이트 자식 오브젝트 이름")]
    [SerializeField] private string _weaponChildName = "WeaponTarget";

    [Header("기본 지급 무기")]
    [Tooltip("인벤토리에 무기가 하나도 없을 때 자동 지급할 기본 무기 ID (Revolver = 6)")]
    [SerializeField] private int _defaultWeaponId = 6;

    #endregion

    void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPoint = transform.position;

        // CJsonManager.Instance가 null일 수 있음 (씬 직접 실행 또는 Script Execution Order 문제)
        CSaveData saveData = CJsonManager.Instance != null ? CJsonManager.Instance.Load() : null;

        // ── 캐릭터 ID 결정 우선순위 ──────────────────────────────────────
        // 1순위: CGameManager 메모리 (_selectedPlayerId) — 이번 세션 선택 또는 PlayerPrefs 복원
        // 2순위: JSON 파일의 playerStatId — 이전 세션에서 저장한 값
        // 3순위: 0 (기본값 폴백)
        // ────────────────────────────────────────────────────────────────
        int playerId = 0;
        int managerSelectedId = CGameManager.Instance != null ? CGameManager.Instance.SelectedPlayerId : -1;

        if (managerSelectedId >= 0 && CDataManager.Instance != null && CDataManager.Instance.GetPlayerData(managerSelectedId) != null)
        {
            // 1순위: 메모리 (이번 세션 선택 or PlayerPrefs 복원)
            playerId = managerSelectedId;
        }
        else if (saveData != null && CDataManager.Instance != null && CDataManager.Instance.GetPlayerData(saveData.playerStatId) != null)
        {
            // 2순위: JSON 파일
            playerId = saveData.playerStatId;
        }
        else
        {
            // 3순위: 기본값 0 (어떤 저장 데이터도 유효하지 않을 때)
            playerId = 0;
            CDebug.LogWarning("[CPlayerSpawner] 유효한 캐릭터 ID를 찾지 못해 기본값(0)으로 폴백합니다.");
        }

        if (CDataManager.Instance == null)
        {
            CDebug.LogError("[CPlayerSpawner] CDataManager.Instance가 null입니다. 플레이어를 스폰할 수 없습니다.");
            return;
        }

        CPlayerDataSO playerData = CDataManager.Instance.GetPlayerData(playerId);

        // null 체크를 Instantiate 호출 전에 수행
        if (playerData == null || playerData.Prefab == null)
        {
            CDebug.LogError($"CPlayerSpawner : {playerId}의 데이터 또는 프리팹 없음");
            return;
        }

        GameObject playerObj = Instantiate(playerData.Prefab, spawnPoint, Quaternion.identity);

        if (playerObj.TryGetComponent(out CPlayerController playerController))
        {
            if (playerData.UniqueTrait != null)
            {
                playerData.UniqueTrait.ApplyTrait(playerController);
            }
        }

        CDebug.Log($"[CPlayerSpawner] 캐릭터 스폰: {playerData.CharacterName} (id={playerId})");

        SetPlayerTarget(playerObj.transform);

        // 항상 실행 — Co_GrantDefaultWeapon 내부에서 인벤토리 상태를 보고 필요할 때만 지급
        StartCoroutine(Co_GrantDefaultWeapon(_defaultWeaponId));
    }

    /// <summary>
    /// RestoreFromSaveData 완료 후 인벤토리에 무기가 없으면 기본 무기를 지급합니다.
    /// 이미 무기가 있으면 아무것도 하지 않습니다.
    /// </summary>
    private IEnumerator Co_GrantDefaultWeapon(int defaultWeaponId)
    {
        // CInventorySystemJ.Start() 및 RestoreFromSaveData가 완료될 때까지 1프레임 대기
        yield return null;

        if (CInventorySystemJ.Instance == null)
        {
            CDebug.LogWarning("[CPlayerSpawner] CInventorySystemJ 인스턴스 없음 — 기본 무기 지급 실패");
            yield break;
        }

        // 인벤토리에 무기가 이미 있으면 지급 불필요
        bool hasAnyWeapon = CInventorySystemJ.Instance.Inventory
            .OfType<CWeaponInstance>()
            .Any();

        if (hasAnyWeapon)
        {
            // 장착된 무기가 없으면 첫 번째 무기를 장착
            bool hasEquipped = CInventorySystemJ.Instance.Inventory
                .OfType<CWeaponInstance>()
                .Any(w => w._isEquipped);

            if (!hasEquipped)
            {
                CWeaponInstance first = CInventorySystemJ.Instance.Inventory
                    .OfType<CWeaponInstance>()
                    .First();
                CInventorySystemJ.Instance.EquipWeapon(first._instanceID);
            }

            CDebug.Log("[CPlayerSpawner] 인벤토리에 무기 존재 — 기본 무기 지급 생략");
            yield break;
        }

        // 인벤토리에 무기가 없으면 기본 무기 지급
        CInventorySystemJ.Instance.AddItem(defaultWeaponId, 1);

        CWeaponInstance weapon = CInventorySystemJ.Instance.Inventory
            .OfType<CWeaponInstance>()
            .FirstOrDefault(w => w._itemData.Id == defaultWeaponId);

        if (weapon != null)
        {
            CInventorySystemJ.Instance.EquipWeapon(weapon._instanceID);
            CDebug.Log($"[CPlayerSpawner] 기본 무기 지급 완료: id={defaultWeaponId}");
        }
        else
        {
            CDebug.LogError($"[CPlayerSpawner] 기본 무기 지급 실패: id={defaultWeaponId}. " +
                           $"CDataManager._weaponList에 해당 SO가 등록되어 있는지 확인하세요.");
        }
    }

    /// <summary>
    /// 스폰된 플레이어 Transform을 등록된 컴포넌트들에 주입합니다.
    /// </summary>
    private void SetPlayerTarget(Transform playerTransform)
    {
        if (_cameraController != null)
            _cameraController.SetTarget(playerTransform);
        else
            CDebug.LogWarning("CPlayerSpawner : CameraController 미연결");

        if (_spawnManager != null)
            _spawnManager.SetPlayerTarget(playerTransform);
        else
            CDebug.LogWarning("CPlayerSpawner : SpawnManager 미연결");

        if (_worldShift != null)
            _worldShift.SetPlayerTarget(playerTransform);
        else if (_worldShiftHorizontal != null)
            _worldShiftHorizontal.SetPlayerTarget(playerTransform);
        else
            CDebug.LogWarning("CPlayerSpawner : WorldShift 미연결");

        if (_weaponEquip != null)
        {
            if (playerTransform.TryGetComponent<CPlayerController>(out CPlayerController playerController))
                _weaponEquip.SetPlayerController(playerController);
            else
                CDebug.LogWarning("CPlayerSpawner : 플레이어에서 CPlayerController를 찾을 수 없음");

            Transform weaponChild = FindDeepChild(playerTransform, _weaponChildName);
            if (weaponChild != null)
                _weaponEquip.SetTargetObject(weaponChild.gameObject);
            else
                CDebug.LogWarning($"CPlayerSpawner : 플레이어 하위에서 '{_weaponChildName}' 오브젝트를 찾을 수 없음");
        }
        else
        {
            CDebug.LogWarning("CPlayerSpawner : WeaponEquip 미연결");
        }

        if (_bossManager != null)
            _bossManager.SetPlayerTarget(playerTransform);
        else
            CDebug.LogWarning("CPlayerSpawner : BossManager 미연결");
    }

    /// <summary>
    /// 이름으로 자식 Transform을 재귀적으로 탐색합니다.
    /// </summary>
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform found = FindDeepChild(child, childName);
            if (found != null) return found;
        }
        return null;
    }
}
