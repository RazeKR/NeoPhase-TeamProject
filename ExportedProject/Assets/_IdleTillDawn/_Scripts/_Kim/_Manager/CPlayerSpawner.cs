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
    }
}
