using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CItemDropManager : MonoBehaviour
{
    #region 인스펙터
    [Header("참조")]
    [SerializeField] private CGenerateItem _itemGenerator;

    #endregion

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

        if (_itemGenerator == null)
        {
            _itemGenerator = GetComponent<CGenerateItem>();
        }
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
    }

    private void HandleEnemyDrop(CEnemyBase enemy)
    {
        enemy.OnDied -= HandleEnemyDrop;

        if (CGameManager.Instance == null) return;

        float finalDropChance = CGameManager.Instance.GetCurrentDropChance();

        float randomChance = Random.Range(0f, 100f);

        if (randomChance <= finalDropChance)
        {
            CDebug.Log($"{enemy.gameObject.name} : 아이템 획득, 현재 확률 {finalDropChance}");

            // 아이템 드롭 효과 추가

            _itemGenerator.GenerateRandomRankItem();
        }
    }
}
