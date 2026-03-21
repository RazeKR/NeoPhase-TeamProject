using UnityEngine;

/// <summary>
/// 플레이어 무기 투사체
/// Init() 호출 시 방향/데미지/속도/수명이 주입된다
/// CEnemy(구 풀 시스템)와 IDamageable(Kim 신 시스템) 모두 피격 처리한다
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CBullet : MonoBehaviour
{
    #region 내부 변수

    private float _damage;
    private float _lifeTime;
    private float _spawnTime;

    #endregion

    #region Public Methods

    /// <summary>
    /// 투사체 초기화 — 스폰 직후 반드시 호출해야 한다
    /// </summary>
    /// <param name="direction">발사 방향 (정규화 불필요, 내부에서 normalized 처리)</param>
    /// <param name="damage">적에게 입힐 데미지</param>
    /// <param name="speed">이동 속도 (Unity 유닛/초)</param>
    /// <param name="lifeTime">자동 소멸까지의 시간 (초)</param>
    public void Init(Vector2 direction, float damage, float speed, float lifeTime)
    {
        _damage    = damage;
        _lifeTime  = lifeTime;
        _spawnTime = Time.time;

        Vector2 dir = direction.normalized;
        GetComponent<Rigidbody2D>().velocity = dir * speed;

        // 스프라이트 기본 방향(오른쪽) 기준으로 투사체 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    #endregion

    #region Unity Methods

    private void Update()
    {
        if (Time.time - _spawnTime >= _lifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 자신에게는 피격하지 않음
        if (other.GetComponent<CPlayerController>() != null) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }

    #endregion
}
