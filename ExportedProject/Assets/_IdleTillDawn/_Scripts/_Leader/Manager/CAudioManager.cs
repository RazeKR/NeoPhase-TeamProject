using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// BGM / SFX 통합 오디오 매니저 싱글톤 (DontDestroyOnLoad)
///
/// [BGM 씬 전환 흐름]
///   각 씬에 CSceneBGM 컴포넌트를 놓고 클립을 연결
///   → CSceneBGM.Start() 에서 PlayBGM() 호출
///   → 이전 BGM 페이드 아웃 후 새 BGM 페이드 인
///
/// [SFX 재생 흐름]
///   각 오브젝트에서 AudioSource를 직접 다루지 않음.
///   → CAudioManager.Instance.Play(soundData, transform.position) 으로 통일
///   → 내부적으로 AudioSource 풀에서 소스를 대여하여 재생, 종료 후 자동 반납
///
/// [볼륨 연동]
///   CSettingsManager 이벤트 구독 → 옵션 버튼 조작 시 자동 반영
///
/// [풀링]
///   초기 _initialPoolSize 개의 AudioSource를 자식 GameObject에 미리 생성.
///   부족하면 _maxPoolSize 까지 동적 확장. 초과 시 해당 재생은 스킵.
///
/// [볼륨 덕킹]
///   동시 재생 수가 _maxConcurrentSFX 초과 시 초과 비율에 따라 볼륨 자동 감소.
///   최소 배율은 _minDuckMultiplier (기본 0.3) 로 제한.
///
/// [쿨다운 / 중복 방지]
///   CSoundData.Cooldown : 동일 SoundData 최소 재생 간격 체크
///   CSoundData.AllowOverlap = false : 이미 재생 중이면 신규 요청 스킵
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CAudioManager : MonoBehaviour
{
    #region Singleton

    public static CAudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitAudioSources();
        InitPool();
    }

    #endregion

    #region Inspector — BGM

    [Header("BGM 전환 페이드")]
    [Tooltip("이전 BGM이 페이드 아웃되는 시간 (초). 0이면 즉시 전환")]
    [SerializeField] private float _fadeOutDuration = 0.5f;  // BGM 페이드 아웃 시간 (초)

    [Tooltip("새 BGM이 페이드 인되는 시간 (초)")]
    [SerializeField] private float _fadeInDuration  = 0.8f;  // BGM 페이드 인 시간 (초)

    [Header("오디오 소스 (자동 생성 — 비워두면 됨)")]
    [SerializeField] private AudioSource _bgmSource;          // BGM 전용 오디오 소스
    [SerializeField] private AudioSource _sfxSource;          // 단순 SFX 레거시 소스 (PlaySFX 호환용)

    #endregion

    #region Inspector — SFX Pool

    [Header("SFX 풀 설정")]
    [Tooltip("게임 시작 시 미리 생성해 둘 AudioSource 수")]
    [SerializeField] private int _initialPoolSize   = 16;    // 초기 풀 크기
    [Tooltip("풀 최대 크기. 초과 시 재생 스킵")]
    [SerializeField] private int _maxPoolSize       = 32;    // 풀 최대 크기
    [Tooltip("이 수를 초과하면 볼륨 덕킹 시작")]
    [SerializeField] private int _maxConcurrentSFX  =  8;    // 동시 재생 최대 수 (덕킹 기준)
    [Tooltip("덕킹 시 볼륨의 최소 배율 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float _minDuckMultiplier = 0.3f; // 덕킹 최소 볼륨 배율

    #endregion

    #region Inspector — AudioMixer (선택)

    [Header("AudioMixer (없으면 무시)")]
    [Tooltip("연결하면 MixerGroup 기반 SFX/BGM 분리 가능. 없어도 정상 동작")]
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;  // SFX 믹서 그룹 (선택)
    [SerializeField] private AudioMixerGroup _bgmMixerGroup;  // BGM 믹서 그룹 (선택)

    #endregion

    #region Private — BGM

    private float     _targetBGMVolume = 1f; // CSettingsManager 볼륨 반영 기준
    private Coroutine _fadeCoroutine;         // 현재 진행 중인 크로스 페이드 코루틴

    #endregion

    #region Private — SFX Volume

    private float _sfxVolumeMultiplier = 1f; // SetSFXVolume()으로 설정되는 전역 SFX 볼륨 배율

    #endregion

    #region Private — Pool

    private List<AudioSource> _pool = new List<AudioSource>(); // AudioSource 풀

    // CSoundData 인스턴스를 키로 사용 — SO 에셋 참조는 씬 내에서 고유하므로 키로 사용 가능
    private Dictionary<CSoundData, float> _lastPlayTime  = new Dictionary<CSoundData, float>(); // 마지막 재생 시각 (쿨다운 체크용)
    private Dictionary<CSoundData, int>   _playingCount  = new Dictionary<CSoundData, int>();   // SoundData 별 현재 재생 중 인스턴스 수

    private int _totalPlayingCount = 0; // 전체 동시 재생 수 (덕킹 계산용)

    #endregion

    #region Unity

    private void Start()
    {
        if (CSettingsManager.Instance == null) return;

        CSettingsManager.Instance.OnMusicVolumeChanged += SetMusicVolume;
        CSettingsManager.Instance.OnSFXVolumeChanged   += SetSFXVolume;

        SetMusicVolume(CSettingsManager.Instance.MusicVolume);
        SetSFXVolume(CSettingsManager.Instance.SFXVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (CSettingsManager.Instance == null) return;

        CSettingsManager.Instance.OnMusicVolumeChanged -= SetMusicVolume;
        CSettingsManager.Instance.OnSFXVolumeChanged   -= SetSFXVolume;
    }

    #endregion

    #region Public API — BGM

    /// <summary>
    /// BGM을 페이드 아웃 → 페이드 인으로 전환합니다.
    /// 동일 클립이 이미 재생 중이면 무시합니다.
    /// CSceneBGM 컴포넌트에서 호출합니다.
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(Co_CrossFadeBGM(clip));
    }

    /// <summary>BGM을 즉시 정지합니다.</summary>
    public void StopBGM()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _bgmSource.Stop();
    }

    #endregion

    #region Public API — SFX Legacy

    /// <summary>
    /// AudioClip 단발 재생 (레거시 호환).
    /// 기존 코드(CButtonManager 등)에서 계속 사용 가능합니다.
    /// SoundData 기반 사운드와 혼용 시 _sfxSource 볼륨이 별도로 관리됩니다.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }

    #endregion

    #region Public API — SFX (SoundData 기반)

    /// <summary>
    /// CSoundData 기반 SFX 재생의 진입점입니다.
    /// 모든 쿨다운 / 중복 / 덕킹 처리를 내부에서 수행합니다.
    ///
    /// 사용 예시:
    ///   Weapon.Fire()   → CAudioManager.Instance.Play(weaponSO.fireSFX, transform.position)
    ///   Monster.OnHit() → CAudioManager.Instance.Play(monsterSO.hitSFX, transform.position)
    ///
    /// position 생략 시 Vector3.zero (2D 사운드와 동일 효과)
    /// </summary>
    /// <param name="data">재생할 사운드 데이터 (null이면 무시)</param>
    /// <param name="position">3D 재생 기준 월드 좌표</param>
    public void Play(CSoundData data, Vector3 position = default)
    {
        if (data == null || !data.IsValid()) return;
        if (!CheckCooldown(data)) return;
        if (!data.AllowOverlap && IsCurrentlyPlaying(data)) return;

        AudioClip clip = data.GetRandomClip();
        if (clip == null) return;

        AudioSource source = GetPooledSource();
        if (source == null) return; // 풀이 꽉 찼으면 재생 스킵

        ConfigureSource(source, data, clip, position);
        source.Play();

        TrackPlayStart(data);
        StartCoroutine(Co_ReturnToPool(source, data, clip.length / Mathf.Max(Mathf.Abs(source.pitch), 0.01f)));
    }

    #endregion

    #region Public API — Volume

    /// <summary>음악 볼륨 설정 (0~100). CSettingsManager 이벤트에서 자동 호출됩니다.</summary>
    public void SetMusicVolume(int percent)
    {
        _targetBGMVolume  = percent / 100f;
        // 페이드 중이 아닐 때만 즉시 반영 (페이드 중이면 코루틴이 목표값을 참조)
        if (_fadeCoroutine == null)
            _bgmSource.volume = _targetBGMVolume;
    }

    /// <summary>SFX 볼륨 설정 (0~100). CSettingsManager 이벤트에서 자동 호출됩니다.</summary>
    public void SetSFXVolume(int percent)
    {
        _sfxVolumeMultiplier = percent / 100f;
        _sfxSource.volume    = _sfxVolumeMultiplier; // 레거시 PlaySFX 소스에도 즉시 반영
    }

    #endregion

    #region Private — BGM Fade Coroutine

    /// <summary>
    /// 이전 BGM 페이드 아웃 후 새 클립을 페이드 인하는 크로스 페이드 코루틴입니다.
    /// _fadeOutDuration = 0이면 즉시 전환, _fadeInDuration = 0이면 즉시 최대 볼륨.
    /// </summary>
    private IEnumerator Co_CrossFadeBGM(AudioClip nextClip)
    {
        // ── 페이드 아웃 ──
        if (_bgmSource.isPlaying && _fadeOutDuration > 0f)
        {
            float startVol = _bgmSource.volume;
            float t = 0f;
            while (t < _fadeOutDuration)
            {
                t += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVol, 0f, t / _fadeOutDuration);
                yield return null;
            }
        }

        // ── 클립 교체 ──
        _bgmSource.Stop();
        _bgmSource.clip   = nextClip;
        _bgmSource.loop   = true;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        // ── 페이드 인 ──
        if (_fadeInDuration > 0f)
        {
            float t = 0f;
            while (t < _fadeInDuration)
            {
                t += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(0f, _targetBGMVolume, t / _fadeInDuration);
                yield return null;
            }
        }

        _bgmSource.volume = _targetBGMVolume;
        _fadeCoroutine    = null;
    }

    #endregion

    #region Private — Pool

    /// <summary>
    /// _initialPoolSize 만큼 AudioSource를 자식 GameObject에 미리 생성합니다.
    /// 각 소스는 비활성 상태로 대기하며 Play() 요청 시 대여됩니다.
    /// </summary>
    private void InitPool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
            CreatePooledSource();
    }

    /// <summary>
    /// 새 AudioSource를 자식 GameObject에 생성하여 풀에 추가합니다.
    /// DontDestroyOnLoad 오브젝트의 자식이므로 씬 전환 후에도 유지됩니다.
    /// </summary>
    private AudioSource CreatePooledSource()
    {
        var go     = new GameObject("PooledSFX");
        go.transform.SetParent(transform); // AudioManager의 자식으로 등록

        var source            = go.AddComponent<AudioSource>();
        source.playOnAwake    = false;
        source.loop           = false;
        source.outputAudioMixerGroup = _sfxMixerGroup; // null이면 기본 출력

        _pool.Add(source);
        return source;
    }

    /// <summary>
    /// 풀에서 재생 중이지 않은 AudioSource를 반환합니다.
    /// 모두 사용 중이면 풀을 확장합니다. 최대 크기 초과 시 null 반환 (재생 스킵).
    /// </summary>
    private AudioSource GetPooledSource()
    {
        foreach (var source in _pool)
        {
            if (!source.isPlaying) return source;
        }

        // 풀 확장 가능하면 생성
        if (_pool.Count < _maxPoolSize)
            return CreatePooledSource();

        CDebug.LogWarning("[CAudioManager] SFX 풀이 꽉 찼습니다. 재생이 스킵됩니다. _maxPoolSize 증가를 고려하세요.");
        return null;
    }

    /// <summary>
    /// AudioSource에 CSoundData 파라미터를 적용합니다.
    /// 3D 사운드이면 position으로 소스를 이동시킵니다.
    /// 2D 사운드이면 SpatialBlend를 0으로 강제합니다.
    /// </summary>
    private void ConfigureSource(AudioSource source, CSoundData data, AudioClip clip, Vector3 position)
    {
        // 3D이면 월드 위치로 이동, 2D이면 AudioManager 위치 유지
        source.transform.position = data.Is3D ? position : transform.position;

        source.clip              = clip;
        source.volume            = data.Volume * _sfxVolumeMultiplier * GetDuckingMultiplier();
        source.pitch             = data.GetRandomPitch();
        source.spatialBlend      = data.Is3D ? data.SpatialBlend : 0f;
        source.minDistance       = data.MinDistance;
        source.maxDistance       = data.MaxDistance;
        source.rolloffMode       = AudioRolloffMode.Linear; // 선형 감쇠 (직관적인 거리 계산)
        source.outputAudioMixerGroup = _sfxMixerGroup;
    }

    /// <summary>
    /// clip.length / pitch 초 후에 카운터를 감소시키고 소스를 풀로 반납합니다.
    /// pitch가 빠를수록 실제 재생 시간이 짧으므로 나눗셈으로 보정합니다.
    /// </summary>
    private IEnumerator Co_ReturnToPool(AudioSource source, CSoundData data, float duration)
    {
        yield return new WaitForSeconds(duration);
        TrackPlayEnd(data);
        source.Stop();
        source.transform.localPosition = Vector3.zero; // 자식 위치 초기화
    }

    #endregion

    #region Private — Cooldown / Overlap Check

    /// <summary>
    /// 동일 SoundData의 마지막 재생 시각과 현재 시각을 비교하여 쿨다운 통과 여부를 반환합니다.
    /// 처음 재생이거나 _cooldown = 0이면 항상 true.
    /// </summary>
    private bool CheckCooldown(CSoundData data)
    {
        if (!_lastPlayTime.TryGetValue(data, out float lastTime)) return true;
        return Time.time - lastTime >= data.Cooldown;
    }

    /// <summary>
    /// 해당 SoundData가 현재 1개 이상 재생 중인지 확인합니다.
    /// AllowOverlap = false 일 때 중복 재생 차단에 사용합니다.
    /// </summary>
    private bool IsCurrentlyPlaying(CSoundData data)
    {
        return _playingCount.TryGetValue(data, out int count) && count > 0;
    }

    #endregion

    #region Private — Play Count Tracking

    /// <summary>재생 시작 시 카운터를 증가시키고 마지막 재생 시각을 기록합니다.</summary>
    private void TrackPlayStart(CSoundData data)
    {
        _lastPlayTime[data] = Time.time;

        if (!_playingCount.ContainsKey(data)) _playingCount[data] = 0;
        _playingCount[data]++;
        _totalPlayingCount++;
    }

    /// <summary>재생 종료 시 카운터를 감소시킵니다. 음수로 내려가지 않도록 보호합니다.</summary>
    private void TrackPlayEnd(CSoundData data)
    {
        if (_playingCount.ContainsKey(data) && _playingCount[data] > 0)
            _playingCount[data]--;

        if (_totalPlayingCount > 0) _totalPlayingCount--;
    }

    #endregion

    #region Private — Volume Ducking

    /// <summary>
    /// 동시 재생 수 기반 볼륨 덕킹 배율을 계산합니다.
    /// _maxConcurrentSFX 이하이면 1.0 (변화 없음).
    /// 초과 시 maxConcurrent / total 비율로 선형 감소, 최솟값은 _minDuckMultiplier.
    /// 예) max=8, total=16 → 0.5 (50% 볼륨). max=8, total=32 → 0.3 (최소값 적용).
    /// </summary>
    private float GetDuckingMultiplier()
    {
        if (_totalPlayingCount <= _maxConcurrentSFX) return 1f;

        float ratio = (float)_maxConcurrentSFX / _totalPlayingCount;
        return Mathf.Clamp(ratio, _minDuckMultiplier, 1f);
    }

    #endregion

    #region Private — Init

    /// <summary>
    /// BGM 소스와 레거시 SFX 소스를 초기화합니다.
    /// BGM 소스는 RequireComponent로 자동 생성된 AudioSource를 재사용합니다.
    /// </summary>
    private void InitAudioSources()
    {
        _bgmSource                    = GetComponent<AudioSource>();
        _bgmSource.playOnAwake        = false;
        _bgmSource.loop               = true;
        _bgmSource.outputAudioMixerGroup = _bgmMixerGroup;

        _sfxSource                    = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake        = false;
        _sfxSource.loop               = false;
        _sfxSource.outputAudioMixerGroup = _sfxMixerGroup;
    }

    #endregion
}
