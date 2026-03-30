public interface IPlayerState
{
    /// <summary>
    /// 상태 진입 시 1회 호출
    /// </summary>
    void Enter();
    void Update();
    void FixedUpdate();
    /// <summary>
    /// 상태 종료 시 1회 호출
    /// </summary>
    void Exit();
}