using UnityEngine;

public class StateMachine : MonoBehaviour
{
	protected State _currentState;

	public virtual State CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			Transition(value);
		}
	}

	public virtual T GetState<T>() where T : State
	{
		T val = GetComponent<T>();
		if (val == null)
		{
			val = base.gameObject.AddComponent<T>();
		}
		return val;
	}

	public virtual void ChangeState<T>() where T : State
	{
		CurrentState = GetState<T>();
	}

	protected virtual void Transition(State value)
	{
		if (!(_currentState == value))
		{
			_currentState?.Exit();
			_currentState = value;
			_currentState?.Enter();
		}
	}
}
