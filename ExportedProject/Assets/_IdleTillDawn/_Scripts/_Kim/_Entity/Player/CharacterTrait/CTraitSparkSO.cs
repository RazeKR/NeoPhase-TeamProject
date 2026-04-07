using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "IdleTillDawn/Trait/PlayerTrait/Spark", fileName = "Trait_Spark")]
public class CTraitSparkSO : CCharacterTraitSO
{
	#region 인스펙터
	[Header("번개 설정")]
	[SerializeField, Range(0f, 1f)] private float _attackChance;
    [SerializeField] private float _detectionRadius = 7.0f;

    [Header("번개 풀 매니저")]
    [SerializeField] private GameObject _thunderPoolManagerPrefab;
    #endregion

    public override void ApplyTrait(CPlayerController player)
    {
        if (CThunderPoolManager.Instance == null)
        {
            GameObject poolObj = Instantiate(_thunderPoolManagerPrefab);

            DontDestroyOnLoad(poolObj);
        }

        player.OnPlayerAttack += () => TryDropThunder(player);
    }

    private void TryDropThunder(CPlayerController player)
    {
        if (Random.value > _attackChance) return;

        Collider2D[] targetsInRange = Physics2D.OverlapCircleAll(
            player.transform.position,
            _detectionRadius,
            player.TargetLayer
        );

        if (targetsInRange.Length == 0 ) return;

        int randomIndex = Random.Range(0, targetsInRange.Length);
        Transform randomTarget = targetsInRange[randomIndex].transform;

        CThunderPoolManager.Instance.ShowThunder(randomTarget.position, Damage, player.TargetLayer);
    }
}
