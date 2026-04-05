using UnityEngine;

/// <summary>
/// 단일 사운드 이펙트의 모든 재생 파라미터를 담는 직렬화 데이터 클래스입니다.
/// ScriptableObject 필드로 선언되며, CAudioManager.Play()에 직접 전달합니다.
/// _clips 배열이 비어있으면 IsValid()가 false를 반환하여 재생이 차단됩니다.
///
/// [사용 방법]
///   SO 인스펙터에서 [Header("사운드")] 아래 CSoundData 필드를 추가한 뒤
///   CAudioManager.Instance.Play(soundData, transform.position) 호출.
/// </summary>
[System.Serializable]
public class CSoundData
{
    #region InspectorVariables

    [Header("클립")]
    [SerializeField] private AudioClip[] _clips;                              // 랜덤 재생용 클립 배열. 여러 개일수록 자연스러운 사운드 다양성 확보

    [Header("볼륨 / 피치")]
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 1f;                              // 기본 재생 볼륨 (0~1). 전역 SFX 볼륨 및 덕킹 배율과 곱해져 최종 볼륨 결정
    [SerializeField] private Vector2 _pitchRange = new Vector2(0.9f, 1.1f);   // 랜덤 피치 범위. x=최솟값, y=최댓값. 0.9~1.1이면 미세한 변주, 0.5~1.5면 극적인 변주

    [Header("3D 공간 음향")]
    [SerializeField] private bool _is3D = true;                               // true면 위치 기반 3D 공간음. false면 화면 전역에서 동일 볼륨인 2D 사운드
    [Range(0f, 1f)]
    [SerializeField] private float _spatialBlend = 1f;                        // 0 = 완전 2D, 1 = 완전 3D. _is3D = false 이면 0으로 강제
    [SerializeField] private float _minDistance = 1f;                         // 이 거리 이하에서는 항상 최대 볼륨 유지
    [SerializeField] private float _maxDistance = 20f;                        // 이 거리 이상에서는 볼륨이 0에 수렴 (Linear 롤오프 기준)

    [Header("재생 제어")]
    [SerializeField] private bool _allowOverlap = true;                       // false면 이미 재생 중인 경우 신규 요청 무시 (단일 인스턴스 사운드)
    [SerializeField] private float _cooldown = 0.05f;                         // 동일 SoundData 최소 재생 간격 (초). 0이면 제한 없음

    #endregion

    #region Properties

    /// <summary>랜덤 재생용 클립 배열</summary>
    public AudioClip[] Clips        => _clips;

    /// <summary>기본 재생 볼륨 (0~1)</summary>
    public float       Volume       => _volume;

    /// <summary>랜덤 피치 범위 (x = min, y = max)</summary>
    public Vector2     PitchRange   => _pitchRange;

    /// <summary>3D 공간음 여부</summary>
    public bool        Is3D         => _is3D;

    /// <summary>SpatialBlend 값 (0 = 2D, 1 = 3D)</summary>
    public float       SpatialBlend => _spatialBlend;

    /// <summary>최소 감쇠 거리 (이하는 최대 볼륨)</summary>
    public float       MinDistance  => _minDistance;

    /// <summary>최대 감쇠 거리 (이상은 무음)</summary>
    public float       MaxDistance  => _maxDistance;

    /// <summary>동시 중복 재생 허용 여부</summary>
    public bool        AllowOverlap => _allowOverlap;

    /// <summary>동일 사운드 최소 재생 간격 (초)</summary>
    public float       Cooldown     => _cooldown;

    #endregion

    #region PublicMethods

    /// <summary>
    /// _clips 배열에서 무작위 AudioClip 하나를 반환합니다.
    /// 배열이 null이거나 비어있으면 null을 반환합니다.
    /// </summary>
    public AudioClip GetRandomClip()
    {
        if (_clips == null || _clips.Length == 0) return null;
        return _clips[Random.Range(0, _clips.Length)];
    }

    /// <summary>
    /// _pitchRange 범위 내 무작위 피치 값을 반환합니다.
    /// x = 최솟값, y = 최댓값. 매 호출마다 다른 값이 반환되어 사운드 다양성을 확보합니다.
    /// </summary>
    public float GetRandomPitch() => Random.Range(_pitchRange.x, _pitchRange.y);

    /// <summary>
    /// 재생 가능한 유효 데이터인지 검증합니다.
    /// _clips 배열이 null이거나 비어있으면 false를 반환합니다.
    /// CAudioManager.Play() 진입 전에 반드시 통과해야 재생됩니다.
    /// </summary>
    public bool IsValid() => _clips != null && _clips.Length > 0;

    #endregion
}
