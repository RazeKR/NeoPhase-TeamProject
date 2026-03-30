public class CPlayerStateMachine
{
    public IPlayerState CurrentState { get; private set; }

    /// <summary>
    /// 게임 시작 시 최초 상태 세팅
    /// </summary>
    /// <param name="startingState"></param>
    public void Initialize(IPlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    /// <summary>
    /// 현재 상태를 다른 상태로 교체
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(IPlayerState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    public void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
}
