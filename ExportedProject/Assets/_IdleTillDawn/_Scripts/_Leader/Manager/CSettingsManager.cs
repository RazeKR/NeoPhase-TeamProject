using System;
using UnityEngine;

/// <summary>
/// 게임 전역 설정 관리 싱글톤 (DontDestroyOnLoad)
///
/// [관리 항목]
/// - 음악 볼륨 / SFX 볼륨 (0, 25, 50, 75, 100 %)
///   → 0% = 완전 음소거. 실제 AudioSource 볼륨 적용은 CAudioManager 담당
/// - 전체화면 / 창모드
/// - 해상도 (800x450 / 1200x675 / 1600x900 / 1920x1080)
///
/// [저장]  PlayerPrefs — 씬 전환 후에도 유지, 재실행 시에도 복원
/// [이벤트] OnMusicVolumeChanged / OnSFXVolumeChanged / OnFullscreenChanged / OnResolutionChanged
///          → CAudioManager, COptionUI 등이 구독하여 즉시 반영
/// </summary>
public class CSettingsManager : MonoBehaviour
{
    #region Singleton

    public static CSettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    #endregion

    #region Constants

    private const string KEY_MUSIC      = "MusicVol";
    private const string KEY_SFX        = "SFXVol";
    private const string KEY_FULLSCREEN = "Fullscreen";
    private const string KEY_RESOLUTION = "ResolutionIdx";

    // 볼륨 단계: 0 → 25 → 50 → 75 → 100 → 0 ...
    // 0% = 완전 음소거 (CAudioManager에서 AudioSource.volume = 0 처리)
    private static readonly int[] VolumeSteps = { 0, 25, 50, 75, 100 };

    // 지원 해상도 목록 (창모드 전용)
    public static readonly Vector2Int[] Resolutions =
    {
        new Vector2Int(800,  450),
        new Vector2Int(1200, 675),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
    };

    #endregion

    #region Events

    public event Action<int>  OnMusicVolumeChanged;   // 0~100
    public event Action<int>  OnSFXVolumeChanged;     // 0~100
    public event Action<bool> OnFullscreenChanged;    // true=전체화면
    public event Action<int>  OnResolutionChanged;    // Resolutions 인덱스

    #endregion

    #region Properties

    public int  MusicVolume     { get; private set; } = 100;
    public int  SFXVolume       { get; private set; } = 100;
    public bool IsFullscreen    { get; private set; } = true;
    public int  ResolutionIndex { get; private set; } = 3; // 기본 1920x1080

    /// <summary>음악이 완전 음소거 상태인지 여부</summary>
    public bool IsMusicMuted => MusicVolume == 0;

    /// <summary>SFX가 완전 음소거 상태인지 여부</summary>
    public bool IsSFXMuted => SFXVolume == 0;

    /// <summary>인덱스에 해당하는 해상도 텍스트 "1920x1080" 형식</summary>
    public static string ResolutionLabel(int index) =>
        $"{Resolutions[index].x}x{Resolutions[index].y}";

    #endregion

    #region Public API

    /// <summary>음악 볼륨 한 단계 순환 (0 → 25 → 50 → 75 → 100 → 0 → ...)</summary>
    public void CycleMusicVolume()
    {
        int next = GetNextVolumeStep(MusicVolume);
        MusicVolume = next;
        PlayerPrefs.SetInt(KEY_MUSIC, next);
        OnMusicVolumeChanged?.Invoke(next);
    }

    /// <summary>SFX 볼륨 한 단계 순환 (0 → 25 → 50 → 75 → 100 → 0 → ...)</summary>
    public void CycleSFXVolume()
    {
        int next = GetNextVolumeStep(SFXVolume);
        SFXVolume = next;
        PlayerPrefs.SetInt(KEY_SFX, next);
        OnSFXVolumeChanged?.Invoke(next);
    }

    /// <summary>전체화면 토글 — 창모드로 바뀌면 현재 해상도 즉시 적용</summary>
    public void ToggleFullscreen()
    {
        IsFullscreen = !IsFullscreen;
        PlayerPrefs.SetInt(KEY_FULLSCREEN, IsFullscreen ? 1 : 0);

        if (IsFullscreen)
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else
            ApplyResolution(ResolutionIndex);

        OnFullscreenChanged?.Invoke(IsFullscreen);
    }

    /// <summary>해상도 한 단계 순환 (마지막 → 처음) — 창모드일 때만 실제 적용</summary>
    public void CycleResolution()
    {
        int next = (ResolutionIndex + 1) % Resolutions.Length;
        ResolutionIndex = next;
        PlayerPrefs.SetInt(KEY_RESOLUTION, next);

        if (!IsFullscreen)
            ApplyResolution(next);

        OnResolutionChanged?.Invoke(next);
    }

    #endregion

    #region Private

    private void LoadAll()
    {
        MusicVolume     = ClampToValidStep(PlayerPrefs.GetInt(KEY_MUSIC,      100));
        SFXVolume       = ClampToValidStep(PlayerPrefs.GetInt(KEY_SFX,        100));
        IsFullscreen    = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
        ResolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt(KEY_RESOLUTION, 3), 0, Resolutions.Length - 1);

        if (IsFullscreen)
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else
            ApplyResolution(ResolutionIndex);

        // 로드 직후 이벤트 발송 → CAudioManager 등 구독자에게 초기값 전달
        OnMusicVolumeChanged?.Invoke(MusicVolume);
        OnSFXVolumeChanged?.Invoke(SFXVolume);
    }

    private void ApplyResolution(int index)
    {
        Vector2Int r = Resolutions[index];
        Screen.SetResolution(r.x, r.y, FullScreenMode.Windowed);
    }

    private int GetNextVolumeStep(int current)
    {
        for (int i = 0; i < VolumeSteps.Length; i++)
        {
            if (VolumeSteps[i] == current)
                return VolumeSteps[(i + 1) % VolumeSteps.Length];
        }
        return VolumeSteps[0]; // 유효하지 않은 저장값 → 최솟값(0)으로
    }

    /// <summary>저장된 볼륨 값이 유효한 단계인지 검증 — 손상된 PlayerPrefs 방어</summary>
    private static int ClampToValidStep(int value)
    {
        foreach (int step in VolumeSteps)
            if (step == value) return value;
        return 100; // 유효하지 않으면 기본값 100
    }

    #endregion
}
