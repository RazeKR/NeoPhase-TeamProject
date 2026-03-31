using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CWorldBossType1Controller : CBossBase
{
    #region 인스펙터
    [Header("패턴 설정")]
    [SerializeField] private float _dashForce = 10f;
    [SerializeField] private float _slidingTime = 1.5f;
    [SerializeField] private string _dashLayer = "Dash_Layer";

    [Header("촉수 소환 설정")]
    [SerializeField] string _tentaclePoolKey = "Tentacle";
    [SerializeField] int _maxTentacleCount = 15;
    [SerializeField] float _spawnInterval = 5f;
    [SerializeField] float _spawnRadiusMin = 3f;
    [SerializeField] float _spawnRadiusMax = 6f;
    #endregion

    #region 내부 변수
    private CNode _rootNode;
    private int _originLayer;
    private Animator _bossAnimator;

    private int _currentTentacleCount = 0;
    private float _lastTentacleSpawnTime = 0f;
    private Coroutine _spawnLoopTentacle;
    #endregion

    #region 이벤트
    public static event System.Action<string, Vector2> OnRequestSpawn;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        _bossAnimator = GetComponent<Animator>();
        CTentacleController.OnTentacleDestroyed += HandleTentacleDestroyed;
    }

    private void OnDestroy()
    {
        CTentacleController.OnTentacleDestroyed -= HandleTentacleDestroyed;
    }

    protected override void Start()
    {
        base.Start();
        _originLayer = gameObject.layer;
        ConstructBehaviourTree();
    }

    private void ConstructBehaviourTree()
    {
        CNode checkTentacle = new CheckTentacleConditionNode(this);
        CNode spawnTentcale = new SpawnTentacleNode(this);

        CNode checkDash = new CCheckDashConditionNode(this);
        CNode dashAction = new CDashActionNode
        (
            this,
            _bossAnimator,
            0f,
            _slidingTime,
            _dashForce,
            LayerMask.NameToLayer(_dashLayer),
            _originLayer
        );
        CNode chaseAction = new CChaseNode(this);

        CSequence tentacleSequence = new CSequence(new List<CNode> { checkTentacle, spawnTentcale });
        CSequence dashSequece = new CSequence(new List<CNode> { checkDash, dashAction });

        _rootNode = new CSelector(new List<CNode> { tentacleSequence, dashSequece, chaseAction });
    }

    protected override void HandleAttack()
    {

    }

    protected override void HandleMovement()
    {
        if (IsKnockBacked) return;

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


    private void HandleTentacleDestroyed()
    {
        _currentTentacleCount = Mathf.Max(0, _currentTentacleCount -1);
    }

    private Vector2 GetRandomSpawnPos()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(_spawnRadiusMin, _spawnRadiusMax);

        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        return (Vector2)CurrentTarget.position + offset;
    }

    private class CheckTentacleConditionNode : CNode
    {
        private CWorldBossType1Controller _boss;

        public CheckTentacleConditionNode(CWorldBossType1Controller boss)
        {
            _boss = boss;
        }

        public override ENodeState Evaluate()
        {
            if (_boss.IsAttacking) return ENodeState.Failure;

            bool isCooltimeReady = Time.time >= _boss._lastTentacleSpawnTime + _boss._spawnInterval;
            bool isCountAvailable = _boss._currentTentacleCount < _boss._maxTentacleCount;
            
            if (isCooltimeReady && isCountAvailable)
            {
                State = ENodeState.Success;
                return State;
            }

            State = ENodeState.Failure;
            return State;
        }
    }

    private class SpawnTentacleNode : CNode
    {
        private CWorldBossType1Controller _boss;

        public SpawnTentacleNode(CWorldBossType1Controller boss)
        {
            _boss = boss;
        }

        public override ENodeState Evaluate()
        {
            Vector2 spawnPos = _boss.GetRandomSpawnPos();
            OnRequestSpawn?.Invoke(_boss._tentaclePoolKey, spawnPos);

            _boss._currentTentacleCount++;
            _boss._lastTentacleSpawnTime = Time.time;

            State = ENodeState.Success;
            return State;
        }
    }
}
