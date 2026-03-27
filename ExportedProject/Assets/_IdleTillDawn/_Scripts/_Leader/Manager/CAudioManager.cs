using System.Collections;
using UnityEngine;

/// <summary>
/// BGM / SFX 오디오 매니저 싱글톤 (DontDestroyOnLoad)
///
/// [BGM 씬 전환 흐름]
///   각 씬에 CSceneBGM 컴포넌트를 놓고 클립을 연결
///   → CSceneBGM.Start() 에서 PlayBGM() 호출
///   → 이전 BGM 페이드 아웃 후 새 BGM 페이드 인
///
/// [볼륨 연동]
///   CSettingsManager 이벤트 구독 → 옵션 버튼 조작 시 자동 반영
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
    }

    #endregion

    #region Inspector

    [Header("BGM 전환 페이드")]
    [Tooltip("이전 BGM이 페이드 아웃되는 시간 (초). 0이면 즉시 전환")]
    [SerializeField] private float _fadeOutDuration = 0.5f;

    [Tooltip("새 BGM이 페이드 인되는 시간 (초)")]
    [SerializeField] private float _fadeInDuration  = 0.8f;

    [Header("오디오 소스 (자동 생성 — 비워두면 됨)")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    #endregion

    #region Private

    private float     _targetBGMVolume = 1f; // CSettingsManager 볼륨 반영 기준
    private Coroutine _fadeCoroutine;

    #endregion

    #region Unity

    private void Start()
    {
        if (CSettingsManager.Instance != null)
        {
            CSettingsManager.Instance.OnMusicVolumeChanged += SetMusicVolume;
            CSettingsManager.Instance.OnSFXVolumeChanged   += SetSFXVolume;

            SetMusicVolume(CSettingsManager.Instance.MusicVolume);
            SetSFXVolume(CSettingsManager.Instance.SFXVolume);
        }
    }

    private void OnDestroy()
    {
        if (CSettingsManager.Instance != null)
        {
            CSettingsManager.Instance.OnMusicVolumeChanged -= SetMusicVolume;
            CSettingsManager.Instance.OnSFXVolumeChanged   -= SetSFXVolume;
        }
    }

    #endregion

    #region Public API — BGM

    /// <summary>
    /// BGM 전환 (페이드 아웃 → 페이드 인)
    /// 같은 클립이면 무시. CSceneBGM에서 호출
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(Co_CrossFadeBGM(clip));
    }

    /// <summary>BGM 즉시 정지</summary>
    public void StopBGM()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _bgmSource.Stop();
    }

    #endregion

    #region Public API — SFX

    /// <summary>효과음 단발 재생</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }

    #endregion

    #region Public API — Volume

    /// <summary>음악 볼륨 설정 (0~100). CSettingsManager 이벤트에서 자동 호출</summary>
    public void SetMusicVolume(int percent)
    {
        _targetBGMVolume  = percent / 100f;
        // 페이드 중이 아닐 때만 즉시 반영 (페이드 중이면 코루틴이 목표값을 참조)
        if (_fadeCoroutine == null)
            _bgmSource.volume = _targetBGMVolume;
    }

    /// <summary>SFX 볼륨 설정 (0~100). CSettingsManager 이벤트에서 자동 호출</summary>
    public void SetSFXVolume(int percent)
    {
        _sfxSource.volume = percent / 100f;
    }

    #endregion

    #region Fade Coroutine

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

    #region Private Init

    private void InitAudioSources()
    {
        _bgmSource             = GetComponent<AudioSource>();
        _bgmSource.playOnAwake = false;
        _bgmSource.loop        = true;

        _sfxSource             = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop        = false;
    }

    #endregion
}
