using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 펫 GameObject에 부착되어 궤도 회전·공격·스프라이트 애니메이션을 처리합니다.
///
/// [타입별 동작]
///   ProjectileBoost / AttackSpeedBoost
///     - 플레이어 기준 원형 궤도로 회전
///     - Idle / Attack 스프라이트 배열 교대 재생
///     - 가장 가까운 적을 감지하여 FireInterval 마다 투사체 발사
///
///   AttackPowerBoost
///     - 플레이어 기준 원형 궤도로 회전하며 스프라이트 자체도 함께 회전
///     - CircleCollider2D(isTrigger) 로 적과 접촉 시 쿨다운마다 근접 데미지 적용
///
/// CPetSpawner가 Init()을 호출하여 초기화합니다.
/// </summary>
public class CPetOrbitController : MonoBehaviour
{
    #region Constants

    /// <summary>스프라이트 한 프레임 표시 시간(초). 10 fps 기준.</summary>
    private const float SpriteFrameInterval = 0.1f;

    /// <summary>AttackPowerBoost 타입의 접촉 데미지 쿨다운(초).</summary>
    private const float MeleeDamageCooldown = 0.5f;

    #endregion

    #region Private Variables

    private CPetInstance  _petInstance;
    private CPetDataSO    _data;
    private Transform     _playerTransform;

    // 궤도
    private float _orbitAngle;

    // 스프라이트 렌더러 (CPetSpawner가 생성 후 할당)
    private SpriteRenderer _spriteRenderer;

    // 스프라이트 애니메이션
    private float _spriteTimer;
    private int   _spriteIndex;
    private bool  _isAttacking;
    private Coroutine _attackAnimCoroutine;

    // 투사체 발사 타이머
    private float _fireTimer;

    // AttackPowerBoost 자전 각도 (궤도 각도와 독립)
    private float _selfRotationAngle;

    // 근접 데미지 쿨다운 (적 instanceID → 다음 데미지 허용 시각)
    private readonly Dictionary<int, float> _meleeCooldownMap = new Dictionary<int, float>();

    #endregion

    #region Initialization

    /// <summary>
    /// CPetSpawner가 GameObject 생성 직후 호출합니다.
    /// </summary>
    /// <param name="petInstance">장착된 펫 인스턴스</param>
    /// <param name="playerTransform">플레이어 Transform</param>
    /// <param name="spriteRenderer">펫 이미지를 표시할 SpriteRenderer</param>
    /// <param name="initialOrbitAngle">초기 궤도 각도(도). 여러 펫이 있을 때 분산 배치에 사용.</param>
    public void Init(CPetInstance petInstance, Transform playerTransform,
                     SpriteRenderer spriteRenderer, float initialOrbitAngle = 0f)
    {
        _petInstance     = petInstance;
        _data            = petInstance._data;
        _playerTransform = playerTransform;
        _spriteRenderer  = spriteRenderer;
        _orbitAngle      = initialOrbitAngle;
        _fireTimer       = _data.FireInterval; // 스폰 즉시 발사 가능하도록 채워둠

        // 초기 스프라이트 설정
        RefreshIdleSprite();
    }

    #endregion

    #region Unity Methods

    private void Update()
    {
        if (_playerTransform == null || _data == null) return;

        UpdateOrbit();

        switch (_data.PetType)
        {
            case EPetType.ProjectileBoost:
            case EPetType.AttackSpeedBoost:
                UpdateSpriteAnimation();
                UpdateProjectileFire();
                break;

            case EPetType.AttackPowerBoost:
                UpdateMeleeSpriteRotation();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_data == null || _data.PetType != EPetType.AttackPowerBoost) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        int id = other.gameObject.GetInstanceID();
        if (_meleeCooldownMap.TryGetValue(id, out float nextTime) && Time.time < nextTime) return;

        _meleeCooldownMap[id] = Time.time + MeleeDamageCooldown;

        Vector2 hitDir = (other.transform.position - transform.position).normalized;
        damageable.TakeDamage(_petInstance.GetPetAttackPower(), hitDir);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 지속 접촉 중에도 쿨다운 기반 데미지 적용
        OnTriggerEnter2D(other);
    }

    #endregion

    #region Orbit

    private void UpdateOrbit()
    {
        _orbitAngle += _data.OrbitSpeed * Time.deltaTime;

        float rad    = _orbitAngle * Mathf.Deg2Rad;
        float radius = _data.OrbitRadius;

        transform.position = _playerTransform.position
                             + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
    }

    #endregion

    #region AttackPowerBoost - Melee Rotation

    /// <summary>
    /// AttackPowerBoost 펫: 궤도 공전 + 스프라이트 자체 자전을 합산하여 회전합니다.
    /// _orbitAngle  → 플레이어 기준 공전 각도
    /// _selfRotationAngle → 스프라이트 자체 자전 각도 (SelfRotationSpeed 도/초)
    /// </summary>
    private void UpdateMeleeSpriteRotation()
    {
        _selfRotationAngle += _data.SelfRotationSpeed * Time.deltaTime;
        transform.rotation  = Quaternion.Euler(0f, 0f, _orbitAngle + _selfRotationAngle);
    }

    #endregion

    #region ProjectileBoost / AttackSpeedBoost - Sprite Animation

    private void UpdateSpriteAnimation()
    {
        if (_spriteRenderer == null) return;

        Sprite[] frames = _isAttacking ? _data.AttackSprites : _data.IdleSprites;

        if (frames == null || frames.Length == 0) return;

        _spriteTimer += Time.deltaTime;

        if (_spriteTimer >= SpriteFrameInterval)
        {
            _spriteTimer -= SpriteFrameInterval;
            _spriteIndex  = (_spriteIndex + 1) % frames.Length;
            _spriteRenderer.sprite = frames[_spriteIndex];
        }
    }

    private void RefreshIdleSprite()
    {
        if (_spriteRenderer == null || _data == null) return;

        switch (_data.PetType)
        {
            case EPetType.AttackPowerBoost:
                _spriteRenderer.sprite = _data.MeleeSprite;
                break;

            case EPetType.ProjectileBoost:
            case EPetType.AttackSpeedBoost:
                if (_data.IdleSprites != null && _data.IdleSprites.Length > 0)
                    _spriteRenderer.sprite = _data.IdleSprites[0];
                break;
        }
    }

    #endregion

    #region ProjectileBoost / AttackSpeedBoost - Projectile Fire

    private void UpdateProjectileFire()
    {
        _fireTimer += Time.deltaTime;
        if (_fireTimer < _data.FireInterval) return;

        Transform nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        _fireTimer = 0f;
        FireProjectileAt(nearestEnemy.position);

        // 공격 애니메이션 전환
        if (_attackAnimCoroutine != null)
            StopCoroutine(_attackAnimCoroutine);
        _attackAnimCoroutine = StartCoroutine(Co_PlayAttackAnim());
    }

    /// <summary>
    /// 씬에 활성화된 CEnemyBase 중 가장 가까운 적의 Transform을 반환합니다.
    /// </summary>
    private Transform FindNearestEnemy()
    {
        CEnemyBase[] enemies = FindObjectsByType<CEnemyBase>(FindObjectsSortMode.None);

        Transform nearest = null;
        float minDist     = float.MaxValue;

        foreach (CEnemyBase enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist  = dist;
                nearest  = enemy.transform;
            }
        }

        return nearest;
    }

    /// <summary>
    /// BulletConfig의 BulletPrefab을 펫 위치에서 대상 방향으로 발사합니다.
    /// ProjectileCount가 2 이상이면 ProjectileSpreadAngle 간격으로 부채꼴 방향으로 나갑니다.
    ///   예) Count=3, Spread=15° → -15°, 0°, +15° 세 방향
    /// </summary>
    private void FireProjectileAt(Vector3 targetPos)
    {
        if (_data.BulletConfig == null || _data.BulletConfig.BulletPrefab == null)
        {
            CDebug.LogWarning($"[CPetOrbitController] BulletConfig 또는 BulletPrefab이 없습니다. ({_data.name})");
            return;
        }

        int   count  = _data.ProjectileCount;
        float spread = _data.ProjectileSpreadAngle;
        float baseAngle = Mathf.Atan2(
            targetPos.y - transform.position.y,
            targetPos.x - transform.position.x) * Mathf.Rad2Deg;

        // count가 홀수면 0°를 중심으로, 짝수면 ±spread/2를 중심으로 균등 분배
        float totalSpread = spread * (count - 1);
        float startOffset = -totalSpread * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angleOffset = startOffset + spread * i;
            float finalAngle  = baseAngle + angleOffset;
            float rad         = finalAngle * Mathf.Deg2Rad;
            Vector2 dir       = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            GameObject bulletObj = Instantiate(
                _data.BulletConfig.BulletPrefab,
                transform.position,
                Quaternion.Euler(0f, 0f, finalAngle));

            flanne.Projectile proj = bulletObj.GetComponent<flanne.Projectile>();
            if (proj != null)
            {
                proj.owner  = _playerTransform != null ? _playerTransform.gameObject : gameObject;
                proj.damage = _petInstance.GetPetAttackPower();
                proj.vector = dir * _data.BulletConfig.ProjectileSpeed;
            }

            // 화상 상태이상 옵션
            if (_data.ApplyBurnOnHit)
            {
                CPetBurnApplier burn = bulletObj.AddComponent<CPetBurnApplier>();
                burn.Init(_data.BurnDuration, _data.BurnTickDamage, _data.BurnTickInterval,
                          _playerTransform != null ? _playerTransform.gameObject : gameObject);
            }

            Destroy(bulletObj, _data.BulletConfig.LifeTime);
        }
    }

    /// <summary>
    /// 공격 스프라이트를 AttackSprites 전체 한 사이클 재생 후 Idle로 복귀합니다.
    /// </summary>
    private IEnumerator Co_PlayAttackAnim()
    {
        _isAttacking  = true;
        _spriteIndex  = 0;
        _spriteTimer  = 0f;

        // AttackSprites 배열 길이 × 프레임 간격 만큼 대기
        int frameCount = _data.AttackSprites != null ? _data.AttackSprites.Length : 0;
        if (frameCount > 0)
            yield return new WaitForSeconds(frameCount * SpriteFrameInterval);

        _isAttacking = false;
        _spriteIndex = 0;
        _spriteTimer = 0f;
    }

    #endregion
}
