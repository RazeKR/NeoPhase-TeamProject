using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMiddleBossController : CBossBase
{
    #region 인스펙터
    [Header("중간 보스 패턴 설정")]
    [SerializeField] private float _dashForce = 10f;
    [SerializeField] private float _prepareTime = 1.5f;
    [SerializeField] private float _slidingTime = 0.8f;
    [SerializeField] private float _dashHitRange = 1.5f;
    [SerializeField] private string _dashLayer = "Dash_Layer";
    #endregion

    #region 내부 변수
    private CNode _rootNode;
    private int _originLayer;
    private Animator _bossAnimator;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        _bossAnimator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();
        _originLayer = gameObject.layer;
        ConstructBehaviourTree();
    }

    private void ConstructBehaviourTree()
    {
        CNode checkDash = new CCheckDashConditionNode(this);
        CNode dashAction = new CDashActionNode
        (
            this,
            _bossAnimator,
            _prepareTime,
            _slidingTime,
            _dashForce,
            _dashHitRange,
            LayerMask.NameToLayer(_dashLayer),
            _originLayer
        );
        CNode chaseAction = new CChaseNode(this);

        CSequence dashSequence = new CSequence(new List<CNode> { checkDash, dashAction });

        _rootNode = new CSelector(new List<CNode> { dashSequence, chaseAction });
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

        float speed = Rb.velocity.magnitude;
        _bossAnimator.SetFloat("aSpeed", speed);
        FlipCharacter(Rb.velocity.x);
    }

    protected override IEnumerator CoProcessPattern()
    {
        yield break;
    }
}
