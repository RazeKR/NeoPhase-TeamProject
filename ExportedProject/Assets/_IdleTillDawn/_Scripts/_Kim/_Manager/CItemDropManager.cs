using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CItemDropManager : MonoBehaviour
{
    #region 내부 변수
    public static CItemDropManager Instance { get; private set; }
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 적이 생성되거나 풀에서 나올 때 이벤트 등록
    /// </summary>
    /// <param name="enemy"></param>
    public void RegisterEnemy(CEnemyBase enemy)
    {
        enemy.OnDied += HandleEnemyDrop;

        //CDebug.Log($"[CItemDropManager] {enemy.gameObject.name}의 OnDied 이벤트 구독 완료!");
    }

    private void HandleEnemyDrop(CEnemyBase enemy)
    {
        //CDebug.Log($"[CItemDropManager] {enemy.gameObject.name} 사망 이벤트 수신 완료!");

        enemy.OnDied -= HandleEnemyDrop;

        if (CGameManager.Instance == null) return;

        float baseDropChance = CGameManager.Instance.GetCurrentDropChance();

        float finalDropChance = baseDropChance * 0.05f;

        CDebug.Log($"현재 드랍 확률 {finalDropChance}");

        float randomChance = Random.Range(0f, 100f);

        if (randomChance <= finalDropChance)
        {
            if (CJsonManager.Instance != null)
            {
                var data = CJsonManager.Instance.GetOrCreateSaveData();
                data.weaponBoxCount++;

                CGoldShopUI.TriggerWeaponBoxCountChanged(data.weaponBoxCount);

                CDebug.Log($"{enemy.gameObject.name} : 아이템 획득, 현재 확률 {finalDropChance}");
            }

        }
    }
}
