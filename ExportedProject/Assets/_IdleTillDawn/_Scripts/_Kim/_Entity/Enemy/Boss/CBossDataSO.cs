using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "2D(SO)/Data/Boss Data", fileName = "BossDataSO_")]
public class CBossDataSO : CEnemyDataSO
{
	#region 인스펙터
	[Header("패턴 설정")]
	[SerializeField] private float _specialAttackCooldown = 5f;
	[SerializeField] private GameObject _specialAttackEffect;
	#endregion

	#region 프로퍼티
	public float SpecialAttackCooldown => _specialAttackCooldown;
	public GameObject SpecialAttackEffect => _specialAttackEffect;
	#endregion
}
