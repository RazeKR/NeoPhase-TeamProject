using UnityEngine;

public class CDashActionNode : CNode
{
    private enum DashState
    {
        Telegraph,
        Dashing
    }
    private DashState _currentState = DashState.Telegraph;
    private float _timer;

    private CBossBase _boss;
    private Animator _animator;

    private float _prepareTime;
    private float _slidingTime;
    private float _dashForce;
    private float _dashHitRange;
    private int _dashLayer;
    private int _originLayer;

    private Vector2 _dashDirection;

    public CDashActionNode(CBossBase boss, Animator animator, float prepareTime, float slidingTime, float dashForce, float dashHitRange, int dashLayer, int originLayer)
    {
        _boss = boss;
        _animator = animator;
        _prepareTime = prepareTime;
        _slidingTime = slidingTime;
        _dashForce = dashForce;
        _dashHitRange = dashHitRange;
        _dashLayer = dashLayer;
        _originLayer = originLayer;
    }

    public override ENodeState Evaluate()
    {
        // 대시 진행 중 빙결 감지 → 즉시 모션캔슬 후 패턴 리셋
        if (_boss.HasStatus(EStatusEffect.Freeze) && _boss.IsAttacking)
        {
            CancelDash();
            State = ENodeState.Failure;
            return State;
        }

        if (_currentState == DashState.Telegraph)
        {
            if (_timer == 0f)
            {
                _animator.SetTrigger("tAttack");
                _boss.IsAttacking = true;
                _boss.DashDamageDealt = false;
            }

            _boss.Rb.velocity = Vector2.zero;
            _timer += Time.fixedDeltaTime;

            if (_timer >= _prepareTime)
            {
                _currentState = DashState.Dashing;
                _timer = 0f;

                SetLayerRecursively(_boss.gameObject, _dashLayer);

                if (_boss.BossEnemyData is CBossDataSO bd && bd.AttackSFX != null)
                    CAudioManager.Instance?.Play(bd.AttackSFX, _boss.transform.position);

                _dashDirection = (_boss.CurrentTarget.position - _boss.transform.position).normalized;
                _boss.Rb.velocity = _dashDirection * _dashForce;
            }
            State = ENodeState.Running;
            return State;
        }
        else if (_currentState == DashState.Dashing)
        {
            // 투사체 충돌 등으로 velocity가 변경되지 않도록 매 프레임 대시 방향으로 고정
            _boss.Rb.velocity = _dashDirection * _dashForce;

            // 거리 기반 피격 판정 (이 게임은 물리 충돌 콜백 대신 거리 체크 방식 사용)
            if (!_boss.DashDamageDealt && _boss.CurrentTarget != null)
            {
                float dist = Vector2.Distance(_boss.transform.position, _boss.CurrentTarget.position);
                if (dist <= _dashHitRange)
                {
                    IDamageable damageable = _boss.CurrentTarget.GetComponent<IDamageable>();
                    damageable?.TakeDamage(_boss.AttackDamage);
                    _boss.DashDamageDealt = true;
                }
            }

            _timer += Time.fixedDeltaTime;

            if (_timer >= _slidingTime)
            {
                _boss.Rb.velocity = Vector2.zero;
                SetLayerRecursively(_boss.gameObject, _originLayer);

                _boss.LastAttackTime = Time.time;
                _boss.IsAttacking = false;

                _currentState = DashState.Telegraph;
                _timer = 0f;

                State = ENodeState.Success;
                return State;
            }
            State = ENodeState.Running;
            return State;
        }

        State = ENodeState.Failure;
        return State;
    }

    /// <summary>
    /// 빙결 등 외부 인터럽트로 대시를 즉시 중단하고 원래 패턴으로 복구
    /// </summary>
    private void CancelDash()
    {
        _boss.Rb.velocity = Vector2.zero;
        SetLayerRecursively(_boss.gameObject, _originLayer);
        _boss.LastAttackTime = Time.time;
        _boss.IsAttacking = false;
        _currentState = DashState.Telegraph;
        _timer = 0f;
    }

    /// <summary>
    /// 자신을 포함한 모든 자식의 레이어를 바꾸는 재귀함수
    /// </summary>
    /// <param name="go"></param>
    /// <param name="newLayer"></param>
    public void SetLayerRecursively(GameObject go, int newLayer)
    {
        go.layer = newLayer;

        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
