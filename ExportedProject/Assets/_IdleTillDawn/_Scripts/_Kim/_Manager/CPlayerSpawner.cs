using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerSpawner : MonoBehaviour
{
    void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPoint = transform.position;

        CSaveData saveData = CJsonManager.Instance.GetOrCreateSaveData();
        int playerId = (saveData  != null && saveData.playerStatId >= 0) ? saveData.playerStatId : 0;

        CPlayerDataSO playerData = CDataManager.Instance.GetPlayerData(playerId);

        if (playerData == null || playerData.Prefab == null)
        {
            Debug.LogError($"CPlayerSpawner : {playerId}의 데이터 또는 프리팹 없음");
            return;
        }

        GameObject playerObj = Instantiate(playerData.Prefab, spawnPoint, Quaternion.identity);
    }
}
