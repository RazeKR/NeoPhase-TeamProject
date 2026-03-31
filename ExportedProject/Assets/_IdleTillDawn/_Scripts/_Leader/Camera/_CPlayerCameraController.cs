using UnityEngine;

/// <summary>
/// 지정된 대상(플레이어)을 부드럽게 추적하는 2D 카메라 컨트롤러 클래스
/// LateUpdate에서 Lerp 보간을 적용하여 플레이어의 이동이 완전히 반영된 이후에 카메라를 갱신한다
/// Z축은 카메라 원본 값을 유지하여 2D 렌더링 뎁스가 변경되지 않도록 한다
/// </summary>
public class _CPlayerCameraController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Transform _target;      // 카메라가 추적할 대상 Transform (일반적으로 플레이어)
    [SerializeField] private float _smoothSpeed = 5f; // Lerp 보간 속도 (값이 높을수록 카메라가 빠르게 따라감)

    #endregion

    #region Public Methods

    /// <summary>카메라 추적 대상을 런타임에 설정합니다.</summary>
    public void SetTarget(Transform target) => _target = target;

    #endregion

    #region Unity Methods

    /// <summary>
    /// 모든 Update 및 FixedUpdate 처리 이후 매 프레임 호출된다
    /// 대상의 XY 좌표로 목표 위치를 계산한 뒤 현재 위치에서 Lerp로 보간 이동하여
    /// 카메라가 플레이어를 부드럽게 따라가는 효과를 구현한다
    /// </summary>
    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desired = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
    }

    #endregion
}
