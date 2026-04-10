using UnityEngine;

public class CStateManual : IPlayerState
{
    private CPlayerController _player;
    private float _idleTimer = 0f;

    public CStateManual(CPlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        //CDebug.Log("CStateManual : Enter");
        _idleTimer = 0f;
    }

    public void Exit()
    {
        //CDebug.Log("CStateManual : Exit");
        _idleTimer = 0f;
    }

    public void Update()
    {
        if (_player.InputHandler == null) return;

        if (_player.InputHandler.IsManualMove)
        {
            _idleTimer = 0f;
        }
        else
        {
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= _player.AutoModeDelay)
            {
                _player.StateMachine.ChangeState(_player.StateAutoChase);
            }
        }
    }

    public void FixedUpdate()
    {
        if (_player.InputHandler == null) return;

        _player.Rb.velocity = _player.InputHandler.MoveInput * _player.MoveSpeed;
    }
}
