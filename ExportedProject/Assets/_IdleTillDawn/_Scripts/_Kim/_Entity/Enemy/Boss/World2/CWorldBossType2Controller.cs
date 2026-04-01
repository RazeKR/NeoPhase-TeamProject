using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CWorldBossType2Controller : CBossBase, ILaserCaster
{
	#region 인스펙터
	[Header("레이저 패턴 설정")]
	[SerializeField] private float _laserCooldown = 8f;
	[SerializeField] private float _laserDuration = 4f;
	[SerializeField] private float _laserRotationSpeed = 90f;

	[Header("레이저 오브젝트")]
	[SerializeField] private GameObject _laserObject;

	[Header("레이저 애니메이션 옵션")]
	[SerializeField] private string _fireParam = "tFire";
	[SerializeField] private string _disappearParam = "tDisappear";
	[SerializeField] private float _readyTime = 1.0f;
	[SerializeField] private float _disappearTime = 0.5f;
	#endregion

	#region 내부 변수
	CNode _rootNode;
	private float _lastFiringTime = 0f;

	private Animator _laserAnimator;
	#endregion

	#region 프로퍼티
	public bool IsFiringLaser { get; private set; } = false;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

		if (_laserObject != null)
		{
			_laserObject.SetActive(false);
			_laserAnimator = _laserObject.GetComponent<Animator>();
		}

		ConstructBehaviourTree();
    }

    protected override void ExecuteAttack()
    {
        
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

    protected override IEnumerator CoProcessPattern()
    {
		yield break;
    }

	public void FireSpinLaser(float duration)
	{
		StartCoroutine(CoFireSpinLaser(duration));
	}

	public IEnumerator CoFireSpinLaser(float duration)
	{
		IsFiringLaser = true;
		_lastFiringTime = Time.time;
		Rb.velocity = Vector2.zero;

		if (_laserObject != null)
		{
			_laserObject.SetActive(true);
		}

		float timer = 0f;

		while (timer < _readyTime)
		{
			if (_laserObject != null)
			{
				_laserObject.transform.Rotate(Vector3.forward, _laserRotationSpeed * Time.fixedDeltaTime);
			}

			timer += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		if (_laserAnimator != null)
		{
			_laserAnimator.SetTrigger(_fireParam);
		}

		timer = 0f;
		float actualFireDuration = duration - _readyTime - _disappearTime;

		while (timer < actualFireDuration)
		{
			if (_laserObject != null)
			{
				_laserObject.transform.Rotate(Vector3.forward, _laserRotationSpeed * Time.fixedDeltaTime);
			}

			timer += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		if (_laserAnimator != null)
		{
			_laserAnimator.SetTrigger(_disappearParam);
		}

		yield return new WaitForSeconds(_disappearTime);

		if (_laserObject != null)
		{
			_laserObject.SetActive(false);
			_laserObject.transform.localRotation = Quaternion.identity;
		}

		IsFiringLaser = false;
	}

	public bool CheckLaserCooldown()
	{
		return Time.time >= _lastFiringTime + _laserCooldown && !IsFiringLaser;
	}

	private void ConstructBehaviourTree()
	{
		CNode checkLaser = new CCheckLaserConditionNode(this);

		CNode fireLaser = new CSpinLaserActionNode(this, _laserDuration);

		CNode chaseAction = new CChaseNode(this);

		CSequence laserSequence = new CSequence(new List<CNode> { checkLaser, fireLaser });

		_rootNode = new CSelector(new List<CNode> { laserSequence, chaseAction });
	}
}
