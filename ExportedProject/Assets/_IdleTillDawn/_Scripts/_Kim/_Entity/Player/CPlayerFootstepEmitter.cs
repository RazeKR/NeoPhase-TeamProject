using UnityEngine;

/// <summary>
/// 플레이어 이동 시 일정 거리마다 발자국을 생성하는 이미터
/// CFootstepPoolManager를 통해 풀 기반으로 발자국을 스폰한다
///
/// [확장 포인트 — 발자국 소리]
///   OnFootstepSpawned 이벤트를 구독하면 발자국 소리를 추가할 수 있다
///   예시:
///     _emitter.OnFootstepSpawned += (pos, dir) => AudioManager.Play("footstep", pos);
/// </summary>
public class CPlayerFootstepEmitter : MonoBehaviour
{
    #region 인스펙터
    [Header("생성 거리")]
    [Tooltip("마지막 발자국 기준 이 거리 이상 이동 시 새 발자국을 생성한다")]
    [SerializeField] private float _spawnDistance = 0.4f;

    [Header("위치 오프셋")]
    [Tooltip("이동 방향 반대쪽으로 밀어내는 거리 (발 뒷부분 느낌)")]
    [SerializeField] private float _backOffset = 0.1f;
    [Tooltip("좌우 발 사이 간격")]
    [SerializeField] private float _sideOffset = 0.08f;
    [Tooltip("자연스러움을 위한 랜덤 흔들림 범위")]
    [SerializeField] private float _jitter = 0.02f;
    #endregion

    #region 내부 변수
    private Vector2 _lastPos;
    private float   _accumulatedDistance;
    private bool    _isLeftFoot;
    #endregion

    #region 이벤트 (확장 포인트 — 발자국 소리 등)
    /// <summary>
    /// 발자국이 생성될 때마다 호출된다
    /// 파라미터: (월드 좌표, 이동 방향)
    /// </summary>
    public event System.Action<Vector2, Vector2> OnFootstepSpawned;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _lastPos = transform.position;
    }

    private void Update()
    {
        Vector2 currentPos = transform.position;
        float   dist       = Vector2.Distance(_lastPos, currentPos);

        // 사실상 정지 상태면 무시
        if (dist < 0.001f) return;

        _accumulatedDistance += dist;

        if (_accumulatedDistance >= _spawnDistance)
        {
            Vector2 dir = (currentPos - _lastPos).normalized;
            SpawnFootstep(currentPos, dir);
            _accumulatedDistance = 0f;
        }

        _lastPos = currentPos;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 발자국 생성 위치를 계산하고 풀 매니저에 스폰을 요청한다
    /// </summary>
    private void SpawnFootstep(Vector2 currentPos, Vector2 dir)
    {
        // 이동 방향 뒤쪽에 생성 (발이 뒤에 찍히는 느낌)
        Vector2 spawnPos = currentPos - dir * _backOffset;

        // 수직 방향 오프셋 — 좌우 발 번갈아, 미세 랜덤 추가
        Vector2 side     = new Vector2(-dir.y, dir.x);
        float   sideSign = _isLeftFoot ? 1f : -1f;
        spawnPos += side * (_sideOffset * sideSign + Random.Range(-_jitter, _jitter));

        CFootstepPoolManager.Instance?.Spawn(spawnPos, dir, _isLeftFoot);
        OnFootstepSpawned?.Invoke(spawnPos, dir);

        _isLeftFoot = !_isLeftFoot;
    }
    #endregion
}
