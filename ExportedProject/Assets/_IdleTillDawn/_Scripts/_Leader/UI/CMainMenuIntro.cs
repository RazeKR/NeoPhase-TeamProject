using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenu_KSH 씬 시작 시 UI 연출 시퀀스
///
/// [연출 순서]
/// 1. 나뭇잎(좌/우)  — 카메라 밖 → 안으로 슬라이드 인
/// 2. 로고           — Alpha 0 → 1 페이드 인
/// 3. 메뉴 텍스트    — 아래에서 위로 슬라이드 + Alpha 페이드 (순차)
/// 4. EyeBlink       — 아래에서 위로 슬라이드 + Alpha 페이드 후
///                     스프라이트 3장 순환 + 위아래 핑퐁 보브
///
/// [실시간 미리보기]
/// 플레이 중 인스펙터에서 EyeBlink 수치를 바꾸면 즉시 반영됩니다.
/// Inspector 우클릭 → "EyeBlink 미리보기" 로 EyeBlink만 단독 재생 가능합니다.
/// </summary>
public class CMainMenuIntro : MonoBehaviour
{
    #region Inspector

    [Header("나뭇잎 (좌 / 우)")]
    [Tooltip("왼쪽 나뭇잎 이미지의 RectTransform")]
    [SerializeField] private RectTransform _leftLeaf;

    [Tooltip("오른쪽 나뭇잎 이미지의 RectTransform")]
    [SerializeField] private RectTransform _rightLeaf;

    [Tooltip("나뭇잎이 시작하는 화면 밖 오프셋 거리 (px). 클수록 더 멀리서 등장")]
    [SerializeField] private float _leafSlideDistance = 500f;

    [Tooltip("나뭇잎 슬라이드 인 완료까지 걸리는 시간 (초)")]
    [SerializeField] private float _leafDuration = 1.2f;

    [Header("로고")]
    [Tooltip("로고 이미지에 붙은 CanvasGroup (Alpha 제어용)")]
    [SerializeField] private CanvasGroup _logoCG;

    [Tooltip("로고가 완전히 나타나는 데 걸리는 시간 (초)")]
    [SerializeField] private float _logoDuration = 1.0f;

    [Header("메뉴 텍스트 (게임시작 / 옵션 / 게임종료 순서)")]
    [Tooltip("각 메뉴 텍스트 오브젝트에 붙은 CanvasGroup 배열 (순서 맞춤)")]
    [SerializeField] private CanvasGroup[] _menuTextCGs;

    [Tooltip("각 메뉴 텍스트 오브젝트의 RectTransform 배열 (CanvasGroup과 동일 순서)")]
    [SerializeField] private RectTransform[] _menuTextRects;

    [Tooltip("텍스트가 시작하는 아래 오프셋 거리 (px). 클수록 더 아래에서 올라옴")]
    [SerializeField] private float _textSlideOffset = 60f;

    [Tooltip("텍스트 1개가 완전히 나타나는 데 걸리는 시간 (초)")]
    [SerializeField] private float _textFadeDuration = 0.5f;

    [Tooltip("텍스트 간 등장 간격 (초). 0이면 동시 등장")]
    [SerializeField] private float _textStagger = 0.15f;

    [Header("── EyeBlink ──────────────────────")]
    [Tooltip("EyeBlink 스프라이트를 표시할 Image 컴포넌트")]
    [SerializeField] private Image _eyeBlinkImage;

    [Tooltip("EyeBlink 오브젝트의 RectTransform (위치 제어용)")]
    [SerializeField] private RectTransform _eyeBlinkRect;

    [Tooltip("눈 깜빡임 스프라이트 3장을 순서대로 배열. [0]=열림 [1]=반눈 [2]=닫힘")]
    [SerializeField] private Sprite[] _eyeBlinkSprites;

    [Tooltip("EyeBlink가 등장 시작하는 아래 오프셋 거리 (px)")]
    [SerializeField] private float _eyeSlideOffset = 80f;

    [Tooltip("EyeBlink가 완전히 나타나는 데 걸리는 시간 (초)")]
    [SerializeField] private float _eyeFadeDuration = 0.8f;

    [Tooltip("스프라이트 프레임 전환 간격 (초). 작을수록 빠르게 깜빡임 ★ 플레이 중 실시간 반영")]
    [SerializeField] private float _eyeFrameInterval = 0.12f;

    [Tooltip("위아래 핑퐁 진폭 (px). 클수록 더 크게 움직임 ★ 플레이 중 실시간 반영")]
    [SerializeField] private float _eyeBobAmount = 6f;

    [Tooltip("위아래 핑퐁 속도. 클수록 빠르게 흔들림 ★ 플레이 중 실시간 반영")]
    [SerializeField] private float _eyeBobSpeed = 1.5f;

    [Header("시퀀스 타이밍")]
    [Tooltip("나뭇잎 슬라이드 시작 후 텍스트가 등장하기까지의 대기 시간 (초)")]
    [SerializeField] private float _delayBeforeText = 0.6f;

    [Tooltip("텍스트 등장 완료 후 EyeBlink가 등장하기까지의 대기 시간 (초)")]
    [SerializeField] private float _delayBeforeEyeBlink = 0.3f;

    #endregion

    #region Private Fields

    private Vector2[] _menuTextOrigins;
    private Vector2   _eyeBlinkOrigin;

    // 실시간 반영을 위한 이전 값 캐시
    private float     _prevFrameInterval;
    private Coroutine _blinkSpriteCoroutine;
    private bool      _eyeBlinkActive = false;

    #endregion

    #region Unity

    private void Start()
    {
        InitPositions();
        _prevFrameInterval = _eyeFrameInterval;
        StartCoroutine(Co_PlayIntro());
    }

    private void Update()
    {
        if (!_eyeBlinkActive) return;

        // 프레임 인터벌이 바뀌면 스프라이트 코루틴 즉시 재시작
        if (!Mathf.Approximately(_prevFrameInterval, _eyeFrameInterval))
        {
            _prevFrameInterval = _eyeFrameInterval;

            if (_blinkSpriteCoroutine != null)
                StopCoroutine(_blinkSpriteCoroutine);
            _blinkSpriteCoroutine = StartCoroutine(Co_BlinkSprite());
        }
    }

    #endregion

    #region Init

    private void InitPositions()
    {
        if (_leftLeaf  != null) _leftLeaf.anchoredPosition  -= new Vector2(_leafSlideDistance, 0f);
        if (_rightLeaf != null) _rightLeaf.anchoredPosition += new Vector2(_leafSlideDistance, 0f);

        if (_logoCG != null) _logoCG.alpha = 0f;

        _menuTextOrigins = new Vector2[_menuTextRects.Length];
        for (int i = 0; i < _menuTextRects.Length; i++)
        {
            _menuTextOrigins[i] = _menuTextRects[i].anchoredPosition;
            _menuTextRects[i].anchoredPosition -= new Vector2(0f, _textSlideOffset);
            if (_menuTextCGs[i] != null) _menuTextCGs[i].alpha = 0f;
        }

        if (_eyeBlinkRect != null)
        {
            _eyeBlinkOrigin = _eyeBlinkRect.anchoredPosition;
            _eyeBlinkRect.anchoredPosition -= new Vector2(0f, _eyeSlideOffset);
        }

        if (_eyeBlinkImage != null)
        {
            Color c = _eyeBlinkImage.color; c.a = 0f;
            _eyeBlinkImage.color = c;

            if (_eyeBlinkSprites != null && _eyeBlinkSprites.Length > 0)
                _eyeBlinkImage.sprite = _eyeBlinkSprites[0];
        }
    }

    #endregion

    #region Intro Sequence

    private IEnumerator Co_PlayIntro()
    {
        StartCoroutine(Co_SlideLeaf(_leftLeaf,  Vector2.right * _leafSlideDistance, _leafDuration));
        StartCoroutine(Co_SlideLeaf(_rightLeaf, Vector2.left  * _leafSlideDistance, _leafDuration));
        StartCoroutine(Co_FadeCanvasGroup(_logoCG, 0f, 1f, _logoDuration));

        yield return new WaitForSeconds(_delayBeforeText);

        for (int i = 0; i < _menuTextCGs.Length; i++)
        {
            StartCoroutine(Co_SlideAndFadeText(i));
            yield return new WaitForSeconds(_textStagger);
        }

        float textTotalDuration = _textFadeDuration + _textStagger * (_menuTextCGs.Length - 1);
        yield return new WaitForSeconds(textTotalDuration + _delayBeforeEyeBlink);

        StartCoroutine(Co_ShowEyeBlink());
    }

    #endregion

    #region Leaf

    private IEnumerator Co_SlideLeaf(RectTransform leaf, Vector2 slideDir, float duration)
    {
        if (leaf == null) yield break;

        Vector2 start  = leaf.anchoredPosition;
        Vector2 target = start + slideDir;
        float   t      = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            leaf.anchoredPosition = Vector2.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }

        leaf.anchoredPosition = target;
    }

    #endregion

    #region Text

    private IEnumerator Co_SlideAndFadeText(int index)
    {
        RectTransform rect   = _menuTextRects[index];
        CanvasGroup   cg     = _menuTextCGs[index];
        Vector2       target = _menuTextOrigins[index];
        Vector2       start  = rect.anchoredPosition;
        float         t      = 0f;

        while (t < _textFadeDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.SmoothStep(0f, 1f, t / _textFadeDuration);
            rect.anchoredPosition = Vector2.Lerp(start, target, ratio);
            if (cg != null) cg.alpha = ratio;
            yield return null;
        }

        rect.anchoredPosition = target;
        if (cg != null) cg.alpha = 1f;
    }

    #endregion

    #region EyeBlink

    private IEnumerator Co_ShowEyeBlink()
    {
        if (_eyeBlinkImage == null || _eyeBlinkRect == null) yield break;

        Vector2 start  = _eyeBlinkRect.anchoredPosition;
        Vector2 target = _eyeBlinkOrigin;
        float   t      = 0f;

        while (t < _eyeFadeDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.SmoothStep(0f, 1f, t / _eyeFadeDuration);
            _eyeBlinkRect.anchoredPosition = Vector2.Lerp(start, target, ratio);
            Color c = _eyeBlinkImage.color; c.a = ratio;
            _eyeBlinkImage.color = c;
            yield return null;
        }

        _eyeBlinkRect.anchoredPosition = target;
        Color final = _eyeBlinkImage.color; final.a = 1f;
        _eyeBlinkImage.color = final;

        _eyeBlinkActive = true;
        _blinkSpriteCoroutine = StartCoroutine(Co_BlinkSprite());
        StartCoroutine(Co_BobLoop());
    }

    /// <summary>
    /// 스프라이트 3장을 _eyeFrameInterval 간격으로 순환
    /// WaitForSeconds 대신 타이머 방식 → 플레이 중 인터벌 변경 즉시 반영
    /// </summary>
    private IEnumerator Co_BlinkSprite()
    {
        if (_eyeBlinkSprites == null || _eyeBlinkSprites.Length == 0) yield break;

        int   frameIdx = 0;
        float timer    = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= _eyeFrameInterval) // 매 프레임 _eyeFrameInterval을 읽어 실시간 반영
            {
                timer -= _eyeFrameInterval;
                _eyeBlinkImage.sprite = _eyeBlinkSprites[frameIdx];
                frameIdx = (frameIdx + 1) % _eyeBlinkSprites.Length;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Sin 곡선 핑퐁 — _eyeBobAmount, _eyeBobSpeed 매 프레임 참조 → 실시간 반영
    /// </summary>
    private IEnumerator Co_BobLoop()
    {
        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;
            float offsetY = Mathf.Sin(elapsed * _eyeBobSpeed * Mathf.PI) * _eyeBobAmount;
            _eyeBlinkRect.anchoredPosition = _eyeBlinkOrigin + new Vector2(0f, offsetY);
            yield return null;
        }
    }

    #endregion

    #region Utility

    private IEnumerator Co_FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;
        cg.alpha = from;
        float t  = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }

        cg.alpha = to;
    }

    #endregion

    #region Context Menu (에디터 전용 미리보기)

    /// <summary>
    /// 인스펙터 우클릭 → "EyeBlink 미리보기"
    /// 플레이 중 EyeBlink 연출만 단독으로 다시 재생
    /// </summary>
    [ContextMenu("EyeBlink 미리보기")]
    private void PreviewEyeBlink()
    {
        if (!Application.isPlaying) return;

        _eyeBlinkActive = false;
        StopAllCoroutines();

        if (_eyeBlinkRect != null)
            _eyeBlinkRect.anchoredPosition = _eyeBlinkOrigin - new Vector2(0f, _eyeSlideOffset);

        if (_eyeBlinkImage != null)
        {
            Color c = _eyeBlinkImage.color; c.a = 0f;
            _eyeBlinkImage.color = c;
        }

        StartCoroutine(Co_ShowEyeBlink());
    }

    /// <summary>
    /// 인스펙터 우클릭 → "전체 인트로 다시 재생"
    /// </summary>
    [ContextMenu("전체 인트로 다시 재생")]
    private void ReplayIntro()
    {
        if (!Application.isPlaying) return;

        _eyeBlinkActive = false;
        StopAllCoroutines();
        InitPositions();
        _prevFrameInterval = _eyeFrameInterval;
        StartCoroutine(Co_PlayIntro());
    }

    #endregion
}
