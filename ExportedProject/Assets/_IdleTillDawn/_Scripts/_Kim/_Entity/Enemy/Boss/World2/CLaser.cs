using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLaser : MonoBehaviour
{
	#region 인스펙터
	[Header("보스 참조")]
	[SerializeField] private CBossBase _boss;

	[Header("데미지 설정")]
	[SerializeField] private float _damageTickRate = 0.5f;
	#endregion

	#region 내부 변수
	private float _lastDamageTime = 0f;
    #endregion

    private void OnEnable()
    {
		_lastDamageTime = 0f;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
		if (collision.GetComponentInParent<CEnemyBase>() != null) return;
		Debug.Log($"{gameObject.name} : {collision.gameObject.name} 감지");

		IDamageable target = collision.GetComponent<IDamageable>();
		Debug.Log($"{collision.gameObject.name}의 IDamageable 여부 {target != null}");

		if (Time.time >= _lastDamageTime + _damageTickRate)
		{
			
			if (target != null)
			{
				target.TakeDamage(_boss.AttackDamage);

				_lastDamageTime = Time.time;
			}
		}
    }
}
