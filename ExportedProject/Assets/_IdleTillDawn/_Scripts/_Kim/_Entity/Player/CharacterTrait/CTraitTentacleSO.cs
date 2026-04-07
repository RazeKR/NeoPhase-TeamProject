using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "IdleTillDawn/Trait/PlayerTrait/Hastur", fileName = "Trait_Tentacle")]
public class CTraitTentacleSO : CCharacterTraitSO
{
	#region 인스펙터
	[Header("촉수 소환 설정")]
	[SerializeField] private float _spawnInterval = 3f;
	[SerializeField] private int _maxTentacleCount = 5;
	[SerializeField] private float _spawnRadiusMin = 2f;
	[SerializeField] private float _spawnRadiusMax = 4f;

	[Header("촉수 풀 매니저")]
	[SerializeField] private GameObject _tentaclePoolManagerPrefab;
    #endregion

    public override void ApplyTrait(CPlayerController player)
    {
        if (CHasturTentaclePoolManager.Instance == null && _tentaclePoolManagerPrefab != null)
		{
			GameObject poolObj = Instantiate(_tentaclePoolManagerPrefab);

			DontDestroyOnLoad(poolObj);
		}

		player.StartCoroutine(CoSpawnTentacles(player));
    }

	private IEnumerator CoSpawnTentacles(CPlayerController player)
	{
		while (true)
		{
			yield return new WaitForSeconds(_spawnInterval);

			if (CHasturTentaclePoolManager.Instance != null && CHasturTentaclePoolManager.Instance.CurrentTentacleCount < _maxTentacleCount)
			{
				Vector2 spawnPos = GetRandomSpawnPos(player.transform.position);

				CHasturTentaclePoolManager.Instance.SpawnTentacle(spawnPos, Damage, player.TargetLayer);
			}
		}
	}

	private Vector2 GetRandomSpawnPos(Vector2 position)
	{
		float angle = Random.Range(0f, Mathf.PI * 2f);
		float dist = Random.Range(_spawnRadiusMin, _spawnRadiusMax);
		return position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
	}
}
