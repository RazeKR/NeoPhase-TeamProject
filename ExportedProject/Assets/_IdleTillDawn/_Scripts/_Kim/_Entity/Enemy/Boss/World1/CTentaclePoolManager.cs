using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTentaclePoolManager : MonoBehaviour
{
    [SerializeField] private GameObject _tentaclePrefab;
    [SerializeField] private Transform _playerTr;

    private void OnEnable()
    {
        CWorldBossType1Controller.OnRequestSpawn += HandleSpawnRequest;
    }

    private void OnDisable()
    {
        CWorldBossType1Controller.OnRequestSpawn -= HandleSpawnRequest;
    }

    private void HandleSpawnRequest(string key, Vector2 position)
    {
        GameObject obj = Instantiate(_tentaclePrefab, position, Quaternion.identity);
        if (obj.TryGetComponent(out CTentacleController tentacle))
        {
            tentacle.SetTarget(_playerTr);
            tentacle.InitEnemy(1);
            tentacle.InitTentacle();
        }

        CDebug.Log($"[임시 스포너] {key} 소환 완료!");
    }
}
