using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CThunderPoolManager : MonoBehaviour
{
	#region 인스펙터
	[Header("풀 설정")]
	[SerializeField] private CThunder _prefab;
	[SerializeField] private int _poolSize = 10;
	#endregion

	#region 내부 변수
	private Queue<CThunder> _pool;
	public static CThunderPoolManager Instance { get; private set; }
    #endregion

    private void Awake()
    {
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		InitPool();
    }

	private void InitPool()
	{
		_pool = new Queue<CThunder>(_poolSize);
		for (int i = 0; i < _poolSize; i++)
		{
			CThunder obj = Instantiate(_prefab, transform);
			obj.gameObject.SetActive(false);
			_pool.Enqueue(obj);
		}
	}

	/// <summary>
	/// 번개를 소환할 때 호출할 메서드
	/// </summary>
	/// <param name="position"></param>
	/// <param name="damage"></param>
	/// <param name="enemyLayer"></param>
	public void ShowThunder(Vector3 position, float damage, LayerMask enemyLayer)
	{
		CThunder thunder = GetFromPool();
		thunder.transform.position = position;
		thunder.gameObject.SetActive(true);
		thunder.Init(damage, enemyLayer);
	}

	/// <summary>
	/// 번개를 풀로 되돌릴 때 호출할 메서드
	/// </summary>
	/// <param name="thunder"></param>
	public void Return(CThunder thunder)
	{
		thunder.gameObject.SetActive(false);
		_pool.Enqueue(thunder);
	}

	private CThunder GetFromPool()
	{
		if (_pool.Count > 0)
		{
			return _pool.Dequeue();
		}
		return Instantiate(_prefab, transform);
	}
}
