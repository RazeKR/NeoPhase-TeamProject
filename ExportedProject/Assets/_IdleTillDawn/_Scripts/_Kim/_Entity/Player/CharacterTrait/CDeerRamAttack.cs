using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDeerRamAttack : MonoBehaviour
{
	#region 프로퍼티
	public float Damage { get; set; }
	public LayerMask EnemyLayer { get; set; }
    #endregion

    private void Awake()
    {
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.05f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTouchAttack(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
		TryTouchAttack(collision.gameObject);
    }

    private void TryTouchAttack(GameObject targetObj)
	{
        if (((1 << targetObj.layer) & EnemyLayer.value) == 0) return;

        CEnemyBase target = targetObj.GetComponentInParent<CEnemyBase>();

        if (target != null)
        {
            Vector2 hitDir = (target.transform.position - transform.position).normalized;

            if (target is CBossBase)
            {
                target.TakeDamage(Damage, hitDir);
                CFogFlashSource.SpawnImpact(target.transform.position, outerRadius: 4f, peakIntensity: 1f);
                Debug.Log($"[사슴 박치기] 보스 '{target.EntityName}'에게 {Damage} 데미지!");
            }
            else
            {
                target.TakeDamage(Damage * 10000f, hitDir);
                CFogFlashSource.SpawnImpact(target.transform.position, outerRadius: 2f, peakIntensity: 0.5f);
                Debug.Log($"[사슴 박치기] 일반 몬스터 '{target.EntityName}' 즉사!");
            }
        }
    }
}
