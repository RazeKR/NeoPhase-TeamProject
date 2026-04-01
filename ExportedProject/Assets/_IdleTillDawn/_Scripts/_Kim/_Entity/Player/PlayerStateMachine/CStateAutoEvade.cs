using UnityEngine;

public class CStateAutoEvade : IPlayerState
{
	private CPlayerController _player;

	public CStateAutoEvade(CPlayerController player)
	{
		_player = player;
	}

	public void Enter()
	{
		Debug.Log("CStateAutoEvade : Enter");
	}

	public void Exit()
	{
		Debug.Log("CStateAutoEvade : Exit");
	}

	public void Update()
	{
		if (_player.InputHandler.IsManualMove)
		{
			_player.StateMachine.ChangeState(_player.StateManual);
		}
	}

	public void FixedUpdate()
	{
		float safeRadius = _player.EvadeRadius * 1.3f;

		Collider2D[] threats = Physics2D.OverlapCircleAll
		(
			_player.transform.position,
			safeRadius,
			_player.HazardLayer
		);

		if (threats.Length == 0)
		{
			_player.Rb.velocity = Vector2.zero;
			_player.StateMachine.ChangeState(_player.StateAutoChase);
			return;
		}

		Vector2 escapeVector = Vector2.zero;

		foreach (Collider2D threat in threats)
		{
			Vector2 closestPoint = threat.ClosestPoint(_player.transform.position);

			Vector2 dirAwayFromThreat = (Vector2)_player.transform.position - closestPoint;
			float distance = Mathf.Max(dirAwayFromThreat.magnitude, 0.1f);

			float repulsionForce = 1f / distance;
			escapeVector += dirAwayFromThreat.normalized * repulsionForce;
		}

		_player.Rb.velocity = escapeVector.normalized * _player.MoveSpeed;
	}
}
