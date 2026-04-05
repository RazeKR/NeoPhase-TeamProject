// ============================================================
// CSoundUsageExamples.cs
// 이 파일은 실제 게임에 포함되지 않습니다. 사용 방법 참고용입니다.
// ============================================================
//
// 아래 코드는 각 오브젝트에서 CAudioManager.Play()를 호출하는
// 표준 패턴을 보여줍니다. 실제 코드에서는 해당 클래스에 붙여넣으세요.
//
// ============================================================

#if false // 이 블록은 컴파일되지 않습니다 (참고용)

// ── 1. 무기 발사 시 ──────────────────────────────────────────

// 무기 발사 로직이 있는 클래스에서 (예: CWeaponController, Projectile 생성 직전)
// _weaponData 는 CWeaponDataSO 레퍼런스

void Fire()
{
    // ... 발사 로직 ...
    CAudioManager.Instance.Play(_weaponData.FireSFX, transform.position);
}


// ── 2. 스킬 시전 시 ──────────────────────────────────────────

// 스킬 시전 로직이 있는 클래스에서 (예: CPlayerStateMachine, ISkill.Activate())
// _skillData 는 CSkillDataSO 레퍼런스

void CastSkill(CSkillDataSO skillData)
{
    // ... 스킬 이펙트 스폰 로직 ...
    CAudioManager.Instance.Play(skillData.CastSFX, transform.position);
}


// ── 3. 일반 몬스터 피격 시 ────────────────────────────────────

// CEnemyBase 또는 피격 처리 메서드에서
// _enemyData 는 CEnemyDataSO 레퍼런스

void OnHit(float damage)
{
    // ... HP 감소, 히트 이펙트 로직 ...
    CAudioManager.Instance.Play(_enemyData.HitSFX, transform.position);
}


// ── 4. 보스 특수 공격 시전 시 ─────────────────────────────────

// CBossBase 또는 보스 패턴 노드에서
// _bossData 는 CBossDataSO 레퍼런스

void OnSpecialAttack()
{
    // ... 특수 공격 로직 ...
    CAudioManager.Instance.Play(_bossData.AttackSFX, transform.position);
}

// 보스 피격 시 (CEnemyDataSO 상속으로 HitSFX 공유)
void OnBossHit(float damage)
{
    // ... HP 감소 로직 ...
    CAudioManager.Instance.Play(_bossData.HitSFX, transform.position);
}


// ── 5. 플레이어 피격 시 ──────────────────────────────────────

// CPlayerStateMachine 또는 IDamageable 구현 메서드에서
// _playerData 는 CPlayerDataSO 레퍼런스

void OnPlayerHit(float damage)
{
    // ... HP 감소, 무적 프레임 로직 ...
    CAudioManager.Instance.Play(_playerData.HitSFX, transform.position);
}


// ── 6. 2D UI 사운드 예시 (position 무관) ──────────────────────

// CButtonManager 에서 호버/클릭 사운드 (position 생략 = Vector3.zero = 2D 처리)
// CSoundData.Is3D = false, SpatialBlend = 0 으로 설정해야 합니다

void OnButtonHover(CSoundData hoverSound)
{
    CAudioManager.Instance.Play(hoverSound); // position 생략 → 2D 재생
}

#endif
