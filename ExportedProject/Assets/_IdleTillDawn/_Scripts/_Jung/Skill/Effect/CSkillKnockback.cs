using flanne;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSkillKnockback : MonoBehaviour
{
    [Header("넉백 설정")]
    public float knockbackForce = 10f; // 밀쳐내는 힘 (Rigidbody 질량에 따라 조절)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Rigidbody2D enemyRb = other.GetComponentInParent<Rigidbody2D>();

            if (enemyRb != null)
            {
                Vector2 pushDir = (other.transform.position - transform.position).normalized;

                // 1. 기존 속도를 무시하고 넉백 방향으로 강제 속도 부여
                // 100은 너무 클 수 있으니 10~20부터 테스트해보세요.
                //enemyRb.velocity = pushDir * knockbackForce;

                other.transform.position += (Vector3)pushDir * knockbackForce * Time.deltaTime;

                Debug.Log($"[넉백] {other.name}에게 {knockbackForce} 속도 부여됨");
            }
        }
    }
}
