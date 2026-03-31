using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerSpawner : MonoBehaviour
{
    #region Inspector Variables

    [Header("플레이어 참조 수신 대상")]
    [SerializeField] private _CPlayerCameraController _cameraController;
    [SerializeField] private CSpawnManager            _spawnManager;
    [SerializeField] private CWorldShift              _worldShift;
    [SerializeField] private CWeaponEquip             _weaponEquip;

    [Header("무기 오브젝트 설정")]
    [Tooltip("플레이어 프리팹 내 무기 스프라이트 자식 오브젝트 이름")]
    [SerializeField] private string _weaponChildName = "WeaponTarget";

    #endregion

    void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPoint = transform.position;

        CSaveData saveData = CJsonManager.Instance.GetOrCreateSaveData();
        int playerId = (saveData != null && saveData.playerStatId >= 0) ? saveData.playerStatId : 0;

        CPlayerDataSO playerData = CDataManager.Instance.GetPlayerData(playerId);

        if (playerData == null || playerData.Prefab == null)
        {
            Debug.LogError($"CPlayerSpawner : {playerId}의 데이터 또는 프리팹 없음");
            return;
        }

        GameObject playerObj = Instantiate(playerData.Prefab, spawnPoint, Quaternion.identity);

        SetPlayerTarget(playerObj.transform);
    }

    /// <summary>
    /// 스폰된 플레이어 Transform을 등록된 컴포넌트들에 주입합니다.
    /// </summary>
    private void SetPlayerTarget(Transform playerTransform)
    {
        if (_cameraController != null)
            _cameraController.SetTarget(playerTransform);
        else
            Debug.LogWarning("CPlayerSpawner : CameraController 미연결");

        if (_spawnManager != null)
            _spawnManager.SetPlayerTarget(playerTransform);
        else
            Debug.LogWarning("CPlayerSpawner : SpawnManager 미연결");

        if (_worldShift != null)
            _worldShift.SetPlayerTarget(playerTransform);
        else
            Debug.LogWarning("CPlayerSpawner : WorldShift 미연결");

        if (_weaponEquip != null)
        {
            Transform weaponChild = FindDeepChild(playerTransform, _weaponChildName);
            if (weaponChild != null)
                _weaponEquip.SetTargetObject(weaponChild.gameObject);
            else
                Debug.LogWarning($"CPlayerSpawner : 플레이어 하위에서 '{_weaponChildName}' 오브젝트를 찾을 수 없음");
        }
        else
        {
            Debug.LogWarning("CPlayerSpawner : WeaponEquip 미연결");
        }
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
