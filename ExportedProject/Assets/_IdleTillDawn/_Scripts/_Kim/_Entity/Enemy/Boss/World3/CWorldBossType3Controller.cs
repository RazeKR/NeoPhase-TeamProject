using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CWorldBossType3Controller : CBossBase
{
    #region 인스펙터
    [Header("공격 설정")]
    [SerializeField] private CParabolaProjectile _projectilePrefab;
    [SerializeField] private float _bombardmentCooldown = 5f;
    [SerializeField] private int _projectileCount = 5;
    [SerializeField] private float _bombardmentRadius = 2f;
    #endregion

    #region 내부 변수
    private CNode _rootNode;

    private float _lastBombardmentTime = 0f;
    private bool _isBombarding = false;
    #endregion

    protected override void Start()
    {
        base.Start();
        ConstructBehaviourTree();
    }

    protected override void HandleMovement()
    {
        if (HasStatus(EStatusEffect.Knockback)) return;

        if (CurrentTarget != null && _rootNode != null)
        {
            _rootNode.Evaluate();
        }
        else
        {
            Rb.velocity = Vector2.zero;
        }
    }

    protected override void ExecuteAttack() { }
    protected override IEnumerator CoProcessPattern() { yield break; }

    private void ConstructBehaviourTree()
    {
        CNode checkBombardment = new CConditionDelegateNode(() =>
        {
            return Time.time >= _lastBombardmentTime + _bombardmentCooldown && !_isBombarding;
        });

        CNode fireBombardment = new CActionDelegateNode(EvaluateBombardment);

        CNode chaseAction = new CChaseNode(this);

        CSequence bombardmentSequence = new CSequence(new List<CNode> { checkBombardment, fireBombardment });
        _rootNode = new CSelector(new List<CNode> { bombardmentSequence, chaseAction });
    }

    private ENodeState EvaluateBombardment()
    {
        if (!_isBombarding)
        {
            StartCoroutine(CoBombardmentPattern());
            Rb.velocity = Vector2.zero;
            return ENodeState.Running;
        }

        if (_isBombarding)
        {
            Rb.velocity = Vector2.zero;
            return ENodeState.Running;
        }

        return ENodeState.Success;
    }

    public IEnumerator CoBombardmentPattern()
    {
        _isBombarding = true;
        _lastBombardmentTime = Time.time;

        Vector3 originScale = transform.localScale;
        Vector3 strechScale = new Vector3(originScale.x * 1.3f, originScale.y * 0.6f, originScale.z);

        float chargeDuration = 0.5f;
        float timer = 0f;

        while (timer < chargeDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originScale, strechScale, timer / chargeDuration);
            yield return null;
        }

        transform.localScale = strechScale;

        float recoveryDuration = 0.05f;
        timer = 0f;

        while (timer < recoveryDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(strechScale, originScale, timer / recoveryDuration);
            yield return null;
        }

        transform.localScale = originScale;

        for (int i = 0; i < _projectileCount; i++)
        {
            Vector2 randomTargetOffset = Random.insideUnitCircle * _bombardmentRadius;
            Vector2 targetPosition = (Vector2)CurrentTarget.transform.position + randomTargetOffset;

            CParabolaProjectile projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            projectile.Fire(transform.position, targetPosition, AttackDamage);
        }

        yield return new WaitForSeconds(0.5f);

        _isBombarding = false;
    }
}
